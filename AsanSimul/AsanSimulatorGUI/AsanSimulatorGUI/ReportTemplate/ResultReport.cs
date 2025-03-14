using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using AsanSimulatorGUI.Modules;
using DevExpress.XtraPrinting.Drawing;

namespace AsanSimulatorGUI.ReportTemplate
{
    public partial class ResultReport : DevExpress.XtraReports.UI.XtraReport
    {

      #region
      public DevExpress.XtraEditors.TextEdit textEdit_supervisor;
      string connectionString = @"XpoProvider=SQLite;Data Source=C:\asanSNT\FCU\db\fcu_simul.db";
      
      
      IDataLayer DB_FCU;


        TestDBControl testdb_control;
        UnitOfWork UOW;
        XPCollection test_collection;
      #endregion

      public ResultReport()
        {
            InitializeComponent();
        }

        public void set_xpo_db(IDataLayer DB_FCU, UnitOfWork UOW, XPCollection test_collection)
        {
            this.DB_FCU = DB_FCU;
            this.UOW = UOW;
            this.test_collection = test_collection;

            this.DataSource = this.test_collection;
        }
     
      public void update_report_detail(string supervisorName) {
         xrTableCell_name.Text = $"{supervisorName}                (확인)";
         xrTableCell_datetime.Text = DateTime.Now.ToString("yy-MM-dd");
         xrTableCell_reportnum.Text = $"ASAN-QT-FCU-{DateTime.Now.ToString("yyMMdd-HHmmss")}";
         xrTableCell_serialnum.Text = $"FCU-{DateTime.Now.ToString("yyMMdd-HHmmss")}";

         // 이미지 설정 
       //   xrPictureBox1.ImageSource = ImageSource.FromFile(@"D:\project\project\ASAN SnTech\FCUsimulator\PC_app\debug1.png");
        //  xrPictureBox2.ImageSource = ImageSource.FromFile(@"D:\project\project\ASAN SnTech\FCUsimulator\PC_app\debug2.png");
          //xrPictureBox3.ImageSource = ImageSource.FromFile(@"D:\project\project\ASAN SnTech\FCUsimulator\PC_app\debug3.png");
          //xrPictureBox4.ImageSource = ImageSource.FromFile(@"D:\project\project\ASAN SnTech\FCUsimulator\PC_app\debug4.png");
         }
      }
}
