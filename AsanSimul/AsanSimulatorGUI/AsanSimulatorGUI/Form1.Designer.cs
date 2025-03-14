namespace AsanSimulatorGUI
{
    partial class Form1
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
         this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
         this.label_modbus_connection = new DevExpress.XtraEditors.LabelControl();
         this.textEdit_status = new DevExpress.XtraEditors.TextEdit();
         this.comboBox_comport = new DevExpress.XtraEditors.ComboBoxEdit();
         this.btn_connect = new DevExpress.XtraEditors.SimpleButton();
         this.textEdit_firmware = new DevExpress.XtraEditors.TextEdit();
         this.textEdit_supervisor = new DevExpress.XtraEditors.TextEdit();
         this.btn_setup = new DevExpress.XtraEditors.SimpleButton();
         this.btn_pdf = new DevExpress.XtraEditors.SimpleButton();
         this.Root = new DevExpress.XtraLayout.LayoutControlGroup();
         this.layoutControlItem4 = new DevExpress.XtraLayout.LayoutControlItem();
         this.layoutControlItem5 = new DevExpress.XtraLayout.LayoutControlItem();
         this.emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();
         this.layoutControlItem6 = new DevExpress.XtraLayout.LayoutControlItem();
         this.layoutControlItem7 = new DevExpress.XtraLayout.LayoutControlItem();
         this.layoutControlItem8 = new DevExpress.XtraLayout.LayoutControlItem();
         this.layoutControlItem9 = new DevExpress.XtraLayout.LayoutControlItem();
         this.emptySpaceItem2 = new DevExpress.XtraLayout.EmptySpaceItem();
         this.layoutControlItem10 = new DevExpress.XtraLayout.LayoutControlItem();
         this.layoutControlItem11 = new DevExpress.XtraLayout.LayoutControlItem();
         this.test_chart1 = new AsanSimulatorGUI.UserComponent.test_chart();
         this.gauge_list1 = new AsanSimulatorGUI.UserComponent.gauge_list();
         this.grid_list1 = new AsanSimulatorGUI.UserComponent.grid_list();
         this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
         this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
         this.layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem();
         ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
         this.layoutControl1.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.textEdit_status.Properties)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.comboBox_comport.Properties)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.textEdit_firmware.Properties)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.textEdit_supervisor.Properties)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.Root)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem7)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem8)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem9)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem2)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem10)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem11)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).BeginInit();
         this.SuspendLayout();
         // 
         // layoutControl1
         // 
         this.layoutControl1.Controls.Add(this.label_modbus_connection);
         this.layoutControl1.Controls.Add(this.textEdit_status);
         this.layoutControl1.Controls.Add(this.comboBox_comport);
         this.layoutControl1.Controls.Add(this.btn_connect);
         this.layoutControl1.Controls.Add(this.textEdit_firmware);
         this.layoutControl1.Controls.Add(this.test_chart1);
         this.layoutControl1.Controls.Add(this.textEdit_supervisor);
         this.layoutControl1.Controls.Add(this.btn_setup);
         this.layoutControl1.Controls.Add(this.btn_pdf);
         this.layoutControl1.Controls.Add(this.gauge_list1);
         this.layoutControl1.Controls.Add(this.grid_list1);
         this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.layoutControl1.Location = new System.Drawing.Point(0, 0);
         this.layoutControl1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
         this.layoutControl1.Name = "layoutControl1";
         this.layoutControl1.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(0, 322, 650, 400);
         this.layoutControl1.Root = this.Root;
         this.layoutControl1.Size = new System.Drawing.Size(958, 508);
         this.layoutControl1.TabIndex = 0;
         this.layoutControl1.Text = "layoutControl1";
         // 
         // label_modbus_connection
         // 
         this.label_modbus_connection.Location = new System.Drawing.Point(862, 12);
         this.label_modbus_connection.Name = "label_modbus_connection";
         this.label_modbus_connection.Size = new System.Drawing.Size(84, 21);
         this.label_modbus_connection.StyleController = this.layoutControl1;
         this.label_modbus_connection.TabIndex = 15;
         this.label_modbus_connection.Text = "통신 해제됨";
         // 
         // textEdit_status
         // 
         this.textEdit_status.EditValue = "";
         this.textEdit_status.Location = new System.Drawing.Point(304, 12);
         this.textEdit_status.Name = "textEdit_status";
         this.textEdit_status.Properties.ReadOnly = true;
         this.textEdit_status.Size = new System.Drawing.Size(79, 28);
         this.textEdit_status.StyleController = this.layoutControl1;
         this.textEdit_status.TabIndex = 14;
         // 
         // comboBox_comport
         // 
         this.comboBox_comport.Location = new System.Drawing.Point(667, 12);
         this.comboBox_comport.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
         this.comboBox_comport.Name = "comboBox_comport";
         this.comboBox_comport.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
         this.comboBox_comport.Size = new System.Drawing.Size(116, 28);
         this.comboBox_comport.StyleController = this.layoutControl1;
         this.comboBox_comport.TabIndex = 13;
         // 
         // btn_connect
         // 
         this.btn_connect.Location = new System.Drawing.Point(787, 12);
         this.btn_connect.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
         this.btn_connect.Name = "btn_connect";
         this.btn_connect.Size = new System.Drawing.Size(71, 26);
         this.btn_connect.StyleController = this.layoutControl1;
         this.btn_connect.TabIndex = 12;
         this.btn_connect.Text = "통신연결";
         this.btn_connect.Click += new System.EventHandler(this.btn_connect_Click);
         // 
         // textEdit_firmware
         // 
         this.textEdit_firmware.Location = new System.Drawing.Point(490, 12);
         this.textEdit_firmware.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
         this.textEdit_firmware.Name = "textEdit_firmware";
         this.textEdit_firmware.Properties.ReadOnly = true;
         this.textEdit_firmware.Size = new System.Drawing.Size(93, 28);
         this.textEdit_firmware.StyleController = this.layoutControl1;
         this.textEdit_firmware.TabIndex = 11;
         // 
         // textEdit_supervisor
         // 
         this.textEdit_supervisor.Location = new System.Drawing.Point(633, 44);
         this.textEdit_supervisor.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
         this.textEdit_supervisor.Name = "textEdit_supervisor";
         this.textEdit_supervisor.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 14.25F);
         this.textEdit_supervisor.Properties.Appearance.Options.UseFont = true;
         this.textEdit_supervisor.Size = new System.Drawing.Size(50, 30);
         this.textEdit_supervisor.StyleController = this.layoutControl1;
         this.textEdit_supervisor.TabIndex = 9;
         // 
         // btn_setup
         // 
         this.btn_setup.Appearance.Font = new System.Drawing.Font("Tahoma", 14.25F);
         this.btn_setup.Appearance.Options.UseFont = true;
         this.btn_setup.Location = new System.Drawing.Point(898, 44);
         this.btn_setup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
         this.btn_setup.Name = "btn_setup";
         this.btn_setup.Size = new System.Drawing.Size(48, 28);
         this.btn_setup.StyleController = this.layoutControl1;
         this.btn_setup.TabIndex = 8;
         this.btn_setup.Text = "설 정";
         // 
         // btn_pdf
         // 
         this.btn_pdf.Appearance.Font = new System.Drawing.Font("Tahoma", 14.25F);
         this.btn_pdf.Appearance.Options.UseFont = true;
         this.btn_pdf.Location = new System.Drawing.Point(812, 44);
         this.btn_pdf.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
         this.btn_pdf.Name = "btn_pdf";
         this.btn_pdf.Size = new System.Drawing.Size(82, 28);
         this.btn_pdf.StyleController = this.layoutControl1;
         this.btn_pdf.TabIndex = 7;
         this.btn_pdf.Text = "PDF 출력";
         this.btn_pdf.Click += new System.EventHandler(this.btn_pdf_Click);
         // 
         // Root
         // 
         this.Root.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
         this.Root.GroupBordersVisible = false;
         this.Root.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1,
            this.layoutControlItem2,
            this.layoutControlItem4,
            this.layoutControlItem5,
            this.emptySpaceItem1,
            this.layoutControlItem6,
            this.layoutControlItem7,
            this.layoutControlItem8,
            this.layoutControlItem9,
            this.emptySpaceItem2,
            this.layoutControlItem3,
            this.layoutControlItem10,
            this.layoutControlItem11});
         this.Root.Name = "Root";
         this.Root.Size = new System.Drawing.Size(958, 508);
         this.Root.TextVisible = false;
         // 
         // layoutControlItem4
         // 
         this.layoutControlItem4.Control = this.btn_pdf;
         this.layoutControlItem4.Location = new System.Drawing.Point(800, 32);
         this.layoutControlItem4.Name = "layoutControlItem4";
         this.layoutControlItem4.Size = new System.Drawing.Size(86, 34);
         this.layoutControlItem4.TextSize = new System.Drawing.Size(0, 0);
         this.layoutControlItem4.TextVisible = false;
         // 
         // layoutControlItem5
         // 
         this.layoutControlItem5.Control = this.btn_setup;
         this.layoutControlItem5.Location = new System.Drawing.Point(886, 32);
         this.layoutControlItem5.Name = "layoutControlItem5";
         this.layoutControlItem5.Size = new System.Drawing.Size(52, 34);
         this.layoutControlItem5.TextSize = new System.Drawing.Size(0, 0);
         this.layoutControlItem5.TextVisible = false;
         // 
         // emptySpaceItem1
         // 
         this.emptySpaceItem1.AllowHotTrack = false;
         this.emptySpaceItem1.Location = new System.Drawing.Point(675, 32);
         this.emptySpaceItem1.Name = "emptySpaceItem1";
         this.emptySpaceItem1.Size = new System.Drawing.Size(125, 34);
         this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
         // 
         // layoutControlItem6
         // 
         this.layoutControlItem6.AppearanceItemCaption.Font = new System.Drawing.Font("Tahoma", 14.25F);
         this.layoutControlItem6.AppearanceItemCaption.Options.UseFont = true;
         this.layoutControlItem6.Control = this.textEdit_supervisor;
         this.layoutControlItem6.Location = new System.Drawing.Point(518, 32);
         this.layoutControlItem6.MaxSize = new System.Drawing.Size(0, 34);
         this.layoutControlItem6.MinSize = new System.Drawing.Size(157, 34);
         this.layoutControlItem6.Name = "layoutControlItem6";
         this.layoutControlItem6.Size = new System.Drawing.Size(157, 34);
         this.layoutControlItem6.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
         this.layoutControlItem6.Text = "시험 진행자";
         this.layoutControlItem6.TextSize = new System.Drawing.Size(91, 23);
         // 
         // layoutControlItem7
         // 
         this.layoutControlItem7.Control = this.textEdit_firmware;
         this.layoutControlItem7.Location = new System.Drawing.Point(375, 0);
         this.layoutControlItem7.MaxSize = new System.Drawing.Size(200, 32);
         this.layoutControlItem7.MinSize = new System.Drawing.Size(200, 32);
         this.layoutControlItem7.Name = "layoutControlItem7";
         this.layoutControlItem7.Size = new System.Drawing.Size(200, 32);
         this.layoutControlItem7.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
         this.layoutControlItem7.Text = "펌웨어 버전";
         this.layoutControlItem7.TextSize = new System.Drawing.Size(91, 21);
         // 
         // layoutControlItem8
         // 
         this.layoutControlItem8.Control = this.btn_connect;
         this.layoutControlItem8.Location = new System.Drawing.Point(775, 0);
         this.layoutControlItem8.MaxSize = new System.Drawing.Size(75, 30);
         this.layoutControlItem8.MinSize = new System.Drawing.Size(75, 30);
         this.layoutControlItem8.Name = "layoutControlItem8";
         this.layoutControlItem8.Size = new System.Drawing.Size(75, 32);
         this.layoutControlItem8.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
         this.layoutControlItem8.TextSize = new System.Drawing.Size(0, 0);
         this.layoutControlItem8.TextVisible = false;
         // 
         // layoutControlItem9
         // 
         this.layoutControlItem9.Control = this.comboBox_comport;
         this.layoutControlItem9.Location = new System.Drawing.Point(575, 0);
         this.layoutControlItem9.MaxSize = new System.Drawing.Size(200, 32);
         this.layoutControlItem9.MinSize = new System.Drawing.Size(200, 32);
         this.layoutControlItem9.Name = "layoutControlItem9";
         this.layoutControlItem9.Size = new System.Drawing.Size(200, 32);
         this.layoutControlItem9.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
         this.layoutControlItem9.Text = "COMPORT";
         this.layoutControlItem9.TextAlignMode = DevExpress.XtraLayout.TextAlignModeItem.AutoSize;
         this.layoutControlItem9.TextSize = new System.Drawing.Size(75, 21);
         this.layoutControlItem9.TextToControlDistance = 5;
         // 
         // emptySpaceItem2
         // 
         this.emptySpaceItem2.AllowHotTrack = false;
         this.emptySpaceItem2.Location = new System.Drawing.Point(0, 0);
         this.emptySpaceItem2.Name = "emptySpaceItem2";
         this.emptySpaceItem2.Size = new System.Drawing.Size(255, 32);
         this.emptySpaceItem2.TextSize = new System.Drawing.Size(0, 0);
         // 
         // layoutControlItem10
         // 
         this.layoutControlItem10.Control = this.textEdit_status;
         this.layoutControlItem10.Location = new System.Drawing.Point(255, 0);
         this.layoutControlItem10.MaxSize = new System.Drawing.Size(120, 32);
         this.layoutControlItem10.MinSize = new System.Drawing.Size(120, 32);
         this.layoutControlItem10.Name = "layoutControlItem10";
         this.layoutControlItem10.Size = new System.Drawing.Size(120, 32);
         this.layoutControlItem10.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
         this.layoutControlItem10.Text = "상태";
         this.layoutControlItem10.TextAlignMode = DevExpress.XtraLayout.TextAlignModeItem.AutoSize;
         this.layoutControlItem10.TextSize = new System.Drawing.Size(32, 21);
         this.layoutControlItem10.TextToControlDistance = 5;
         // 
         // layoutControlItem11
         // 
         this.layoutControlItem11.Control = this.label_modbus_connection;
         this.layoutControlItem11.Location = new System.Drawing.Point(850, 0);
         this.layoutControlItem11.Name = "layoutControlItem11";
         this.layoutControlItem11.Size = new System.Drawing.Size(88, 32);
         this.layoutControlItem11.TextSize = new System.Drawing.Size(0, 0);
         this.layoutControlItem11.TextVisible = false;
         // 
         // test_chart1
         // 
         this.test_chart1.Location = new System.Drawing.Point(530, 351);
         this.test_chart1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
         this.test_chart1.Name = "test_chart1";
         this.test_chart1.Size = new System.Drawing.Size(416, 145);
         this.test_chart1.TabIndex = 10;
         // 
         // gauge_list1
         // 
         this.gauge_list1.Location = new System.Drawing.Point(530, 78);
         this.gauge_list1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
         this.gauge_list1.Name = "gauge_list1";
         this.gauge_list1.Size = new System.Drawing.Size(416, 269);
         this.gauge_list1.TabIndex = 5;
         // 
         // grid_list1
         // 
         this.grid_list1.Location = new System.Drawing.Point(12, 44);
         this.grid_list1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
         this.grid_list1.Name = "grid_list1";
         this.grid_list1.Size = new System.Drawing.Size(514, 452);
         this.grid_list1.TabIndex = 4;
         // 
         // layoutControlItem1
         // 
         this.layoutControlItem1.Control = this.grid_list1;
         this.layoutControlItem1.Location = new System.Drawing.Point(0, 32);
         this.layoutControlItem1.Name = "layoutControlItem1";
         this.layoutControlItem1.Size = new System.Drawing.Size(518, 456);
         this.layoutControlItem1.TextSize = new System.Drawing.Size(0, 0);
         this.layoutControlItem1.TextVisible = false;
         // 
         // layoutControlItem2
         // 
         this.layoutControlItem2.Control = this.gauge_list1;
         this.layoutControlItem2.Location = new System.Drawing.Point(518, 66);
         this.layoutControlItem2.Name = "layoutControlItem2";
         this.layoutControlItem2.Size = new System.Drawing.Size(420, 273);
         this.layoutControlItem2.TextSize = new System.Drawing.Size(0, 0);
         this.layoutControlItem2.TextVisible = false;
         // 
         // layoutControlItem3
         // 
         this.layoutControlItem3.Control = this.test_chart1;
         this.layoutControlItem3.Location = new System.Drawing.Point(518, 339);
         this.layoutControlItem3.Name = "layoutControlItem3";
         this.layoutControlItem3.Size = new System.Drawing.Size(420, 149);
         this.layoutControlItem3.TextSize = new System.Drawing.Size(0, 0);
         this.layoutControlItem3.TextVisible = false;
         // 
         // Form1
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(958, 508);
         this.Controls.Add(this.layoutControl1);
         this.Margin = new System.Windows.Forms.Padding(6);
         this.Name = "Form1";
         this.Text = "FCU simulator";
         this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
         ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
         this.layoutControl1.ResumeLayout(false);
         this.layoutControl1.PerformLayout();
         ((System.ComponentModel.ISupportInitialize)(this.textEdit_status.Properties)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.comboBox_comport.Properties)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.textEdit_firmware.Properties)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.textEdit_supervisor.Properties)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.Root)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem7)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem8)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem9)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem2)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem10)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem11)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).EndInit();
         this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraLayout.LayoutControlGroup Root;
        private UserComponent.grid_list grid_list1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private UserComponent.gauge_list gauge_list1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraEditors.SimpleButton btn_setup;
        private DevExpress.XtraEditors.SimpleButton btn_pdf;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem4;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem5;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem1;
        private DevExpress.XtraEditors.TextEdit textEdit_supervisor;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem6;
        private UserComponent.test_chart test_chart1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem3;
        private DevExpress.XtraEditors.ComboBoxEdit comboBox_comport;
        private DevExpress.XtraEditors.SimpleButton btn_connect;
        private DevExpress.XtraEditors.TextEdit textEdit_firmware;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem7;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem8;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem9;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem2;
        private DevExpress.XtraEditors.TextEdit textEdit_status;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem10;
        private DevExpress.XtraEditors.LabelControl label_modbus_connection;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem11;
    }
}

