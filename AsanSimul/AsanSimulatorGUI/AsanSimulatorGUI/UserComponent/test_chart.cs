using DevExpress.XtraCharts;
using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AsanSimulatorGUI.UserComponent {
   public partial class test_chart:DevExpress.XtraEditors.XtraUserControl {
      XYDiagram diagram;
      AxisX axisx;
      AxisY axisy_1st;
      SecondaryAxisY axisy_2nd;
      SeriesCollection series;


      public test_chart() {
         InitializeComponent();
         diagram = (XYDiagram)chartControl1.Diagram;
         axisx = diagram.AxisX;
         axisy_1st = diagram.AxisY;
         axisy_2nd = diagram.SecondaryAxesY[0];
         series = chartControl1.Series;
         }

      public void set_normal_test() {
         for(int i = series.Count - 1;i >= 0;i--) {
            series[i].Points.Clear();
            }
         foreach(ConstantLine constantLine in axisx.ConstantLines) {
            constantLine.Visible = false;
            }
         axisx.Strips[0].Visible = false;
         axisx.Title.Text = "시간(ms)";
         axisy_2nd.Visibility = DevExpress.Utils.DefaultBoolean.False;
         }

      public void set_abnormal_test() {
         for(int i = series.Count - 1;i >= 0;i--) {
            series[i].Points.Clear();
            }
         foreach(ConstantLine constantLine in axisx.ConstantLines) {
            constantLine.Visible = false;
            }

         axisx.Strips[0].Visible = false;
         axisx.Title.Text = "시간(sec)";
         axisy_2nd.Visibility = DevExpress.Utils.DefaultBoolean.False;
         }

      public void set_time_test() {
         for(int i = series.Count - 1;i >= 0;i--) {
            series[i].Points.Clear();
            }
         foreach(ConstantLine constantLine in axisx.ConstantLines) {
            constantLine.Visible = true;
            }

         axisx.Strips[0].Visible = true;
         axisx.Title.Text = "시간(ms)";
         axisy_2nd.Visibility = DevExpress.Utils.DefaultBoolean.True;
         }

      public void reset_chart() {
         series["전 압"].Points.Clear();
         series["화재저항"].Points.Clear();

         axisx.Visibility = DevExpress.Utils.DefaultBoolean.False;
         axisx.Strips[0].Visible = false;
         axisx.ConstantLines["fire_time_start"].Visible = false;
         axisx.ConstantLines["fire_time_end"].Visible = false;
         }


      //6,7,8 시험항목 
      /// <summary>
      /// 
      /// </summary>
      /// <param name="time"></param>
      /// <param name="voltage"></param>
      /// <param name="resistor"></param>
      public void make_chart(int time,Double voltage,Double resistor) {
         series["전 압"].Points.Add(new SeriesPoint(time,voltage));
         series["화재저항"].Points.Add(new SeriesPoint(time,resistor));
         }



      //반응시간 차트 
      public void make_time_chart(Double time,Double voltage,Double resistor) {
         axisx.Visibility = DevExpress.Utils.DefaultBoolean.True;
         series["전 압"].Points.Add(new SeriesPoint(0,0));
         series["전 압"].Points.Add(new SeriesPoint(time * 2 - 0.0001,0));
         series["전 압"].Points.Add(new SeriesPoint(time * 2,voltage));
         series["전 압"].Points.Add(new SeriesPoint(time * 3,voltage));

         series["화재저항"].Points.Add(new SeriesPoint(0,0));
         series["화재저항"].Points.Add(new SeriesPoint(time - 0.0001,0));
         series["화재저항"].Points.Add(new SeriesPoint(time,resistor));
         series["화재저항"].Points.Add(new SeriesPoint(time * 3,resistor));

         axisx.Strips[0].Visible = true;
         axisx.Strips[0].MinLimit.AxisValue = time;
         axisx.Strips[0].MaxLimit.AxisValue = time * 2;
         axisx.ConstantLines["fire_time_start"].Visible = true;
         axisx.ConstantLines["fire_time_start"].Title.Text = $"화재 발생 시간차: {time}ms";
         axisx.ConstantLines["fire_time_start"].AxisValue = time;
         axisx.ConstantLines["fire_time_end"].Visible = true;
         axisx.ConstantLines["fire_time_end"].AxisValue = time * 2;
         }

      public void make_time_chart(Double time,Double voltage) {
         series["전 압"].Points.Add(new SeriesPoint(0,0));
         series["전 압"].Points.Add(new SeriesPoint(time * 2 - 0.0001,0));
         series["전 압"].Points.Add(new SeriesPoint(time * 2,voltage));
         series["전 압"].Points.Add(new SeriesPoint(time * 3,voltage));

         series["화재저항"].Points.Add(new SeriesPoint(0,0));
         series["화재저항"].Points.Add(new SeriesPoint(time - 0.0001,0));
         series["화재저항"].Points.Add(new SeriesPoint(time,1));
         series["화재저항"].Points.Add(new SeriesPoint(time * 3,1));

         axisx.Strips[0].Visible = true;
         axisx.Strips[0].MinLimit.AxisValue = time;
         axisx.Strips[0].MaxLimit.AxisValue = time * 2;
         axisx.ConstantLines["fire_time_start"].Visible = true;
         axisx.ConstantLines["fire_time_start"].Title.Text = $"화재 발생 시간차: {time}ms";
         axisx.ConstantLines["fire_time_start"].AxisValue = time;
         axisx.ConstantLines["fire_time_end"].Visible = true;
         axisx.ConstantLines["fire_time_end"].AxisValue = time * 2;
         }

      public void test_make_time_chart() {
         //set_time_test();
         //make_time_chart(4.55, 27.9);
         if(IsHandleCreated) {
            BeginInvoke(new Action(() => {
               set_time_test();
               make_time_chart(4.55,27.9);
            }));
            } else {
            set_time_test();
            make_time_chart(4.55,27.9);
            }
         }
      }
   }
