namespace AsanSimulatorGUI.UserComponent
{
    partial class test_chart
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            DevExpress.XtraCharts.XYDiagram xyDiagram1 = new DevExpress.XtraCharts.XYDiagram();
            DevExpress.XtraCharts.ConstantLine constantLine1 = new DevExpress.XtraCharts.ConstantLine();
            DevExpress.XtraCharts.ConstantLine constantLine2 = new DevExpress.XtraCharts.ConstantLine();
            DevExpress.XtraCharts.Strip strip1 = new DevExpress.XtraCharts.Strip();
            DevExpress.XtraCharts.HatchFillOptions hatchFillOptions1 = new DevExpress.XtraCharts.HatchFillOptions();
            DevExpress.XtraCharts.SecondaryAxisY secondaryAxisY1 = new DevExpress.XtraCharts.SecondaryAxisY();
            DevExpress.XtraCharts.Series series1 = new DevExpress.XtraCharts.Series();
            DevExpress.XtraCharts.LineSeriesView lineSeriesView1 = new DevExpress.XtraCharts.LineSeriesView();
            DevExpress.XtraCharts.Series series2 = new DevExpress.XtraCharts.Series();
            DevExpress.XtraCharts.LineSeriesView lineSeriesView2 = new DevExpress.XtraCharts.LineSeriesView();
            this.chartControl1 = new DevExpress.XtraCharts.ChartControl();
            this.splashScreenManager1 = new DevExpress.XtraSplashScreen.SplashScreenManager(this, typeof(global::AsanSimulatorGUI.UserComponent.WaitForm1), true, true, typeof(System.Windows.Forms.UserControl));
            ((System.ComponentModel.ISupportInitialize)(this.chartControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(xyDiagram1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(strip1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(secondaryAxisY1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(series1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(lineSeriesView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(series2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(lineSeriesView2)).BeginInit();
            this.SuspendLayout();
            // 
            // chartControl1
            // 
            constantLine1.AxisValueSerializable = "3.4";
            constantLine1.Color = System.Drawing.Color.Transparent;
            constantLine1.Name = "fire_time_start";
            constantLine1.ShowInLegend = false;
            constantLine1.Title.DXFont = new DevExpress.Drawing.DXFont("Tahoma", 14F);
            constantLine1.Title.Text = "화재인식 반응시간: xx ms";
            constantLine1.Title.TextColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            constantLine2.AxisValueSerializable = "7";
            constantLine2.Color = System.Drawing.Color.Transparent;
            constantLine2.Name = "fire_time_end";
            constantLine2.ShowInLegend = false;
            constantLine2.Title.Visible = false;
            xyDiagram1.AxisX.ConstantLines.AddRange(new DevExpress.XtraCharts.ConstantLine[] {
            constantLine1,
            constantLine2});
            xyDiagram1.AxisX.GridLines.MinorVisible = true;
            xyDiagram1.AxisX.GridLines.Visible = true;
            xyDiagram1.AxisX.Label.DXFont = new DevExpress.Drawing.DXFont("Tahoma", 14F);
            strip1.FillStyle.FillMode = DevExpress.XtraCharts.FillMode.Hatch;
            hatchFillOptions1.HatchStyle = System.Drawing.Drawing2D.HatchStyle.ForwardDiagonal;
            strip1.FillStyle.Options = hatchFillOptions1;
            strip1.MaxLimit.AxisValueSerializable = "7";
            strip1.MinLimit.AxisValueSerializable = "3.4";
            strip1.Name = "time_diff_range";
            strip1.ShowInLegend = false;
            xyDiagram1.AxisX.Strips.AddRange(new DevExpress.XtraCharts.Strip[] {
            strip1});
            xyDiagram1.AxisX.Title.DXFont = new DevExpress.Drawing.DXFont("Tahoma", 14F);
            xyDiagram1.AxisX.Title.Text = "시간";
            xyDiagram1.AxisX.Title.Visibility = DevExpress.Utils.DefaultBoolean.True;
            xyDiagram1.AxisX.VisibleInPanesSerializable = "-1";
            xyDiagram1.AxisY.Label.DXFont = new DevExpress.Drawing.DXFont("Tahoma", 14F);
            xyDiagram1.AxisY.Title.DXFont = new DevExpress.Drawing.DXFont("Tahoma", 14F);
            xyDiagram1.AxisY.Title.Text = "전압(V)";
            xyDiagram1.AxisY.Title.Visibility = DevExpress.Utils.DefaultBoolean.True;
            xyDiagram1.AxisY.VisibleInPanesSerializable = "-1";
            secondaryAxisY1.AxisID = 0;
            secondaryAxisY1.Label.DXFont = new DevExpress.Drawing.DXFont("Tahoma", 14F);
            secondaryAxisY1.Name = "Secondary AxisY 1";
            secondaryAxisY1.NumericScaleOptions.AutoGrid = false;
            secondaryAxisY1.Title.DXFont = new DevExpress.Drawing.DXFont("Tahoma", 14F);
            secondaryAxisY1.Title.Text = "화재 저항";
            secondaryAxisY1.Title.Visibility = DevExpress.Utils.DefaultBoolean.True;
            secondaryAxisY1.VisibleInPanesSerializable = "-1";
            secondaryAxisY1.VisualRange.Auto = false;
            secondaryAxisY1.VisualRange.MaxValueSerializable = "3";
            secondaryAxisY1.VisualRange.MinValueSerializable = "0";
            secondaryAxisY1.WholeRange.Auto = false;
            secondaryAxisY1.WholeRange.MaxValueSerializable = "3";
            secondaryAxisY1.WholeRange.MinValueSerializable = "0";
            xyDiagram1.SecondaryAxesY.AddRange(new DevExpress.XtraCharts.SecondaryAxisY[] {
            secondaryAxisY1});
            this.chartControl1.Diagram = xyDiagram1;
            this.chartControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartControl1.Legend.DXFont = new DevExpress.Drawing.DXFont("Tahoma", 14F);
            this.chartControl1.Location = new System.Drawing.Point(0, 0);
            this.chartControl1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chartControl1.Name = "chartControl1";
            series1.Name = "전 압";
            series1.View = lineSeriesView1;
            series2.Name = "화재저항";
            lineSeriesView2.AxisYName = "Secondary AxisY 1";
            series2.View = lineSeriesView2;
            this.chartControl1.SeriesSerializable = new DevExpress.XtraCharts.Series[] {
        series1,
        series2};
            this.chartControl1.Size = new System.Drawing.Size(822, 601);
            this.chartControl1.TabIndex = 0;
            // 
            // splashScreenManager1
            // 
            this.splashScreenManager1.ClosingDelay = 500;
            // 
            // test_chart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chartControl1);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "test_chart";
            this.Size = new System.Drawing.Size(822, 601);
            ((System.ComponentModel.ISupportInitialize)(strip1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(secondaryAxisY1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(xyDiagram1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(lineSeriesView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(series1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(lineSeriesView2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(series2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartControl1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraCharts.ChartControl chartControl1;
        private DevExpress.XtraSplashScreen.SplashScreenManager splashScreenManager1;
    }
}
