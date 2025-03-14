using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AsanSimulatorGUI.FCU_ORM;
using AsanSimulatorGUI.Modules;
using AsanSimulatorGUI.UserComponent;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using multimediatimer;
using pdfscreenshottest;
using PDFmaker;
using static DevExpress.XtraCharts.Native.RangeDataBase;
using DevExpress.XtraPrinting;
using static iTextSharp.text.pdf.PdfSigGenericPKCS;
using AsanSimulatorGUI.ReportTemplate;
using System.Collections.Concurrent;

namespace AsanSimulatorGUI
{
    public partial class Form1 : DevExpress.XtraEditors.XtraForm
    {
        FCUData fcudata = new FCUData();
        ConcurrentQueue<FCUData> write_queue = new ConcurrentQueue<FCUData>();
        ConcurrentQueue<bool> new_fcudata_queue = new ConcurrentQueue<bool>();
        ModbusControl modbus_control;

        //DB/ORM
        TestDBControl testdb_control;
        IDataLayer DB_FCU;
        string connectionString = @"XpoProvider=SQLite;Data Source=C:\asanSNT\FCU\db\fcu_simul.db";
        UnitOfWork UOW;
        XPCollection test_collection;


        //test_scenario
        string result_path;
        string now_test_name;
        TestControl testcontrol;

        //report
        ImgCapture capture = new ImgCapture();


        public Form1()
        {
            InitializeComponent();
            init_UI();
            init_system();

            grid_list1.Initialize(testcontrol);


         ////chart control test function
         //test_chart1.test_make_time_chart();
         //PdfMaker pdfMaker = new PdfMaker();
         //pdfMaker.CreatePDF();
         }

        public void init_UI()
        {
            comboBox_comport.Properties.Items.Clear();
            string[] comports = System.IO.Ports.SerialPort.GetPortNames();
            foreach(string comport in comports)
            {
                comboBox_comport.Properties.Items.Add(comport);
            }
        }

        private void init_system()
        {
            string[] split = connectionString.Split(new char[] { '\\' });
            string db_path = @"C:\";
            for(int i = 1; i <= split.Length - 2; i++)
            {
                db_path = Path.Combine(db_path, split[i]);
                if (!File.Exists(db_path))
                {
                    Directory.CreateDirectory(db_path);
                }
            }
            DB_FCU = XpoDefault.GetDataLayer(connectionString, AutoCreateOption.DatabaseAndSchema);
            UOW = new UnitOfWork(DB_FCU);


            test_collection = new XPCollection(typeof(FCUTest));
            test_collection.Session = UOW;
            //test_collection.DisplayableProperties = "test_status;test_class;fire_result;fault_result;other_result";
            test_collection.DisplayableProperties = "SMALL_TEST";
            test_data_check();

            grid_list1.btn_click_event += Grid_list1_btn_click_event;
            grid_list1.set_unitofwork(UOW);
            grid_list1.set_fcudata(fcudata);

           gauge_list1.set_fcu_data(fcudata);

            modbus_control = new ModbusControl(fcudata, write_queue);
            modbus_control.update_comport += Modbus_control_update_comport;
            modbus_control.firmware_ver_get += Modbus_control_firmware_ver_get;
            modbus_control.update_real += Modbus_control_update_real;

            testcontrol = new TestControl(new_fcudata_queue, fcudata, write_queue);
            testcontrol.draw_time_graph_event += Testcontrol_draw_time_graph_event;
            testcontrol.need_lamp_event += Testcontrol_need_lamp_event;
            testcontrol.update_graph_event += Testcontrol_update_graph_event;
            testcontrol.reset_graph_event += Testcontrol_reset_graph_event;
            testcontrol.order_screenshot_event += Testcontrol_order_screenshot_event;
            testcontrol.test_done_event += Testcontrol_test_done_event;
        }

        private void Testcontrol_test_done_event()
        {
            UOW.CommitChanges();
        }

        private void Testcontrol_order_screenshot_event()
        {
            //TODO 스크린 샷 찍기
        }

        int time_interval = 0;
        private void Testcontrol_reset_graph_event()
        {
            time_interval = 0;
            Invoke(new Action(() =>
            {
                test_chart1.reset_chart();
            }));
        }

        private void Testcontrol_update_graph_event(int time, int resistor)
        {
            Invoke(new Action(() =>
            {
                test_chart1.make_chart(time_interval, ((Double)fcudata.voltage_fire / 100), ((Double)resistor / 1000));
            }));
        }

        private void Testcontrol_need_lamp_event(FCUTest fcutest)
        {
            LampChoiceForm lampchoice = new LampChoiceForm();
            lampchoice.set_fcutest(fcutest);
            lampchoice.FormClosed += Lampchoice_FormClosed;
            lampchoice.ShowDialog();
        }

        private void Lampchoice_FormClosed(object sender, FormClosedEventArgs e)
        {
            //lampchoice close는 lamp값이 업데이트 되었거나 아니거나 둘 중 하나이다. 그래서 그냥 업데이트 해준다.
            UOW.CommitChanges();
            testcontrol.set_flag_lamp();
        }

        private void Testcontrol_draw_time_graph_event(SMALL_TEST smalltest)
        {
            //smalltest : fire time측정만 들어옴
            Invoke(new Action(() =>
            {
                test_chart1.make_time_chart(smalltest.measure_value, ((Double)fcudata.voltage_fire / 100), 2.2);
            }));
        }

        private void Grid_list1_btn_click_event(string tag)
        {
            switch(tag)
            {
                case "start":
                    if (!fcudata.ctrl_remote)
                    {
                        MessageBox.Show("시험기를 Remote로 전환하세요", "Remote Error", MessageBoxButtons.OK);
                        return;
                    }


                    testdb_control.reset_testdb();
                    testcontrol.set_test_queue(grid_list1.get_test_queue());
                    testcontrol.start_test();

                    
                    break;
                case "pause":
                    testcontrol.pause_test();
                    break;
                case "stop":
                    testcontrol.stop_test();
                    break;
            }
        }

        //시험항목DB 검사 및 생성
        private void test_data_check()
        {
            testdb_control = new TestDBControl(DB_FCU, UOW, test_collection);
            testdb_control.initital_testclass();

            grid_list1.set_testlist(test_collection);
        }

        private void Modbus_control_update_real()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    if (fcudata.ctrl_remote)
                    {
                        textEdit_status.Text = "REMOTE";
                    }
                    else
                    {
                        textEdit_status.Text = "MANUAL";
                    }
                    gauge_list1.set_gauge_data();
                }));
            }
            else
            {
                if (fcudata.ctrl_remote)
                {
                    textEdit_status.Text = "REMOTE";
                }
                else
                {
                    textEdit_status.Text = "MANUAL";
                }
                gauge_list1.set_gauge_data();
            }
        }

        private void Modbus_control_firmware_ver_get()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    textEdit_firmware.Text = fcudata.version;
                    
                }));
            }
            else
            {
                textEdit_firmware.Text = fcudata.version;
            }
        }

        private void Modbus_control_update_comport(string[] comports)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    comboBox_comport.Properties.Items.Clear();
                    for(int i = 0;  i < comports.Length - 1; i++)
                    {
                        comboBox_comport.Properties.Items.Add(comports[i]);
                    }
                }));
            }
            else
            {
                comboBox_comport.Properties.Items.Clear();
                for (int i = 0; i < comports.Length - 1; i++)
                {
                    comboBox_comport.Properties.Items.Add(comports[i]);
                }
            }
        }

        private void btn_connect_Click(object sender, EventArgs e)
        {
            //testdb_control.test_pdf_db();
            if (comboBox_comport.Text == "") return;
            if (!modbus_control.modbus_isconnection())
            {
                if (modbus_control.connect_modbus(comboBox_comport.Text))
                {
                    label_modbus_connection.Text = "통신 연결됨";
                    btn_connect.Text = "통신 해제";
                }
            }
            else
            {
                if (modbus_control.disconnect_modbus())
                {
                    label_modbus_connection.Text = "통신 해제됨";
                    btn_connect.Text = "통신 연결";
                }
            }
        }

        private void btn_pdf_Click(object sender, EventArgs e)
        {
         //TODO pdf 만들기
#if DEBUG
         //PdfMaker pdfMaker = new PdfMaker();
         //pdfMaker.set_fcudata(UOW.Query<FCUTest>().ToList());
         //pdfMaker.make_report();

         string supervisorName = textEdit_supervisor?.Text ?? "홍길동";
         ResultReport report = new ResultReport();
            report.set_xpo_db(DB_FCU, UOW, test_collection);
            report.update_report_detail(supervisorName);
            report.ExportToPdf(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads\Report1.pdf");
            System.Diagnostics.Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads\Report1.pdf");
#endif
        }
    }
}
