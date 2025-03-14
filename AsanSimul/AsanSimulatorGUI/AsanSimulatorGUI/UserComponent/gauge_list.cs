using AsanSimulatorGUI.Modules;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AsanSimulatorGUI.UserComponent
{
    public partial class gauge_list : DevExpress.XtraEditors.XtraUserControl
    {
        FCUData fcudata;
        public gauge_list()
        {
            InitializeComponent();
        }

        public void set_fcu_data(FCUData fCUData)
        {
            this.fcudata = fCUData;
        }

        //public void set_gauge_data()
        //{
        //    digitalGauge1.Text = ((float)fcudata.voltage_fcu / 100 + 0.0005f).ToString();
        //    digitalGauge2.Text = ((float)fcudata.current / 1000 + 0.0005f).ToString();
        //    digitalGauge3.Text = ((float)fcudata.power / 100 + 0.0005f).ToString();

        //    if (fcudata.resistor == 9999) digitalGauge4.Text = "OPEN";
        //    else digitalGauge4.Text = ((float)fcudata.resistor / 1000 + 0.0005f).ToString("0.0");

        //    digitalGauge5.Text = ((float)fcudata.voltage_fire / 100 + 0.0005f).ToString();
        //    digitalGauge6.Text = ((float)fcudata.voltage_fault / 100 + 0.0005f).ToString();
        //}


      public void set_gauge_data() {
         digitalGauge1.Text = ((float)fcudata.voltage_fcu / 100).ToString("F2");
         digitalGauge2.Text = ((float)fcudata.current / 1000).ToString("F2");
         digitalGauge3.Text = ((float)fcudata.power / 100).ToString("F2");

         if(fcudata.resistor == 9999)
            digitalGauge4.Text = "OPEN";
         else
            digitalGauge4.Text = ((float)fcudata.resistor / 1000).ToString("F2");

         digitalGauge5.Text = ((float)fcudata.voltage_fire / 100).ToString("F2");
         //digitalGauge6.Text = ((float)fcudata.voltage_fault / 100).ToString("F2");


         if(true) {
            digitalGauge6.Text = "ON "; 
            }else
            digitalGauge6.Text = "OFF ";
         }

      }
   }
