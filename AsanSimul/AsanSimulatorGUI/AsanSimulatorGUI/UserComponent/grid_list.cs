using AsanSimulatorGUI.FCU_ORM;
using DevExpress.Xpo;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AsanSimulatorGUI.Modules;
using DevExpress.XtraGrid.Views.Grid;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;

namespace AsanSimulatorGUI.UserComponent
{
    public partial class grid_list : DevExpress.XtraEditors.XtraUserControl
    {
        public delegate void draw_graph(string tag);
        public event draw_graph draw_graph_event;

        public delegate void need_lamp();
        public event need_lamp need_lamp_event;

        public delegate void btn_click(string tag);
        public event btn_click btn_click_event;

        XPCollection test_list = new XPCollection();
        UnitOfWork UOW;

        public ConcurrentQueue<FCUTest> test_queue = new ConcurrentQueue<FCUTest>();

        public FCUData fcudata;

        int flag_test_status = 0;


      #region constructor

      public grid_list()
        {
            InitializeComponent();
        }

        public void Initialize(TestControl testControl) {


         testControl.update_grid_status_event += UpdateGridStatus;
         }

      private void UpdateGridStatus(FCUTest test,string status) {
         if(InvokeRequired) {
            Invoke(new Action(() => UpdateGridStatus(test,status)));
            return;
            }

         test.small_categ = $"{test.small_categ} ({status})";   
         gridControl1.RefreshDataSource();
         }






      #endregion

      public void set_fcudata(FCUData fcudata)
        {
            this.fcudata = fcudata;
        }

        public void set_unitofwork(UnitOfWork unitOfWork)
        {
            this.UOW = unitOfWork;
        }

        public void set_testlist(XPCollection test_list)
        {
            this.test_list = test_list;
            gridControl1.DataSource = this.test_list;
            GridView gridView = this.gridControl1.MainView as GridView;
            gridView.BestFitColumns();
        }



      

      private void btn_pause_Click(object sender, EventArgs e)
        {
            btn_click_event("pause");

            btn_run.Enabled = true;
            btn_stop.Enabled = true;
            btn_pause.Enabled = false;
        }

        private void btn_stop_Click(object sender, EventArgs e)
        {
            btn_click_event("stop");

            btn_run.Enabled = true;
            btn_stop.Enabled = false;
            btn_pause.Enabled = true;


            gridView1.Columns[0].ColumnEdit.ReadOnly = false;
            FCUTest temp;
            while (test_queue.Count() > 0)
            {
                test_queue.TryDequeue(out temp);
            }
            
        }



      #region 표시 상황 넣을것 

      private void btn_run_Click(object sender,EventArgs e) {
         //UOW.CommitChanges();//시험여부 저장 REVIEW 이부분은 어디에 필요해서 작성해뒀는가?
         //스레드가 없을 경우 생성함

         if(fcudata.ctrl_remote) {
            FCUTest temp;
            while(test_queue.Count() > 0) {
               test_queue.TryDequeue(out temp);

               //
               }

            foreach(FCUTest list in test_list) {
               if(list.test_status) {
                  test_queue.Enqueue(list);
                  }
               }

            btn_run.Enabled = false;
            btn_stop.Enabled = true;
            btn_pause.Enabled = true;

            gridView1.Columns[0].ColumnEdit.ReadOnly = true;
            }

         btn_click_event("start");
         }

      #endregion




      public ConcurrentQueue<FCUTest> get_test_queue()
        {
            return test_queue;
        }

        /// <summary>
        /// 검사항목 전체선택 기능
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkEdit_total_select_CheckedChanged(object sender, EventArgs e)
        {
            //if (flag_test_status == (int)THREAD_STATUS.running) return;
            
            if (checkEdit_total_select.Checked)
            {
                foreach (FCUTest test in test_list)
                {
                    test.test_status = true;
                }
                //gridControl1.RefreshDataSource();
            }
            else
            {
                foreach (FCUTest test in test_list)
                {
                    test.test_status = false;
                }
                //gridControl1.RefreshDataSource();
            }
            UOW.CommitChanges();
        }
    }
}
