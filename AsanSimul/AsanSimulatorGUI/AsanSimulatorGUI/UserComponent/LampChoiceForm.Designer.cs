namespace AsanSimulatorGUI.UserComponent
{
    partial class LampChoiceForm
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
            this.tablePanel1 = new DevExpress.Utils.Layout.TablePanel();
            this.tablePanel2 = new DevExpress.Utils.Layout.TablePanel();
            this.tablePanel3 = new DevExpress.Utils.Layout.TablePanel();
            this.btn_ok = new DevExpress.XtraEditors.SimpleButton();
            this.groupControl1 = new DevExpress.XtraEditors.GroupControl();
            this.groupControl2 = new DevExpress.XtraEditors.GroupControl();
            this.radio_acoustic = new DevExpress.XtraEditors.RadioGroup();
            this.radio_fire = new DevExpress.XtraEditors.RadioGroup();
            ((System.ComponentModel.ISupportInitialize)(this.tablePanel1)).BeginInit();
            this.tablePanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tablePanel2)).BeginInit();
            this.tablePanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tablePanel3)).BeginInit();
            this.tablePanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).BeginInit();
            this.groupControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).BeginInit();
            this.groupControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.radio_acoustic.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radio_fire.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // tablePanel1
            // 
            this.tablePanel1.Columns.AddRange(new DevExpress.Utils.Layout.TablePanelColumn[] {
            new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 55F)});
            this.tablePanel1.Controls.Add(this.tablePanel3);
            this.tablePanel1.Controls.Add(this.tablePanel2);
            this.tablePanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tablePanel1.Location = new System.Drawing.Point(0, 0);
            this.tablePanel1.Name = "tablePanel1";
            this.tablePanel1.Rows.AddRange(new DevExpress.Utils.Layout.TablePanelRow[] {
            new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 341F),
            new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 100F)});
            this.tablePanel1.Size = new System.Drawing.Size(406, 290);
            this.tablePanel1.TabIndex = 0;
            this.tablePanel1.UseSkinIndents = true;
            // 
            // tablePanel2
            // 
            this.tablePanel1.SetColumn(this.tablePanel2, 0);
            this.tablePanel2.Columns.AddRange(new DevExpress.Utils.Layout.TablePanelColumn[] {
            new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 55F),
            new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 100F)});
            this.tablePanel2.Controls.Add(this.btn_ok);
            this.tablePanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tablePanel2.Location = new System.Drawing.Point(13, 181);
            this.tablePanel2.Name = "tablePanel2";
            this.tablePanel1.SetRow(this.tablePanel2, 1);
            this.tablePanel2.Rows.AddRange(new DevExpress.Utils.Layout.TablePanelRow[] {
            new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 100F)});
            this.tablePanel2.Size = new System.Drawing.Size(380, 96);
            this.tablePanel2.TabIndex = 0;
            this.tablePanel2.UseSkinIndents = true;
            // 
            // tablePanel3
            // 
            this.tablePanel1.SetColumn(this.tablePanel3, 0);
            this.tablePanel3.Columns.AddRange(new DevExpress.Utils.Layout.TablePanelColumn[] {
            new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 5F),
            new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 5F)});
            this.tablePanel3.Controls.Add(this.groupControl2);
            this.tablePanel3.Controls.Add(this.groupControl1);
            this.tablePanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tablePanel3.Location = new System.Drawing.Point(13, 12);
            this.tablePanel3.Name = "tablePanel3";
            this.tablePanel1.SetRow(this.tablePanel3, 0);
            this.tablePanel3.Rows.AddRange(new DevExpress.Utils.Layout.TablePanelRow[] {
            new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 26F)});
            this.tablePanel3.Size = new System.Drawing.Size(380, 165);
            this.tablePanel3.TabIndex = 1;
            this.tablePanel3.UseSkinIndents = true;
            // 
            // btn_ok
            // 
            this.tablePanel2.SetColumn(this.btn_ok, 1);
            this.btn_ok.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_ok.Location = new System.Drawing.Point(271, 12);
            this.btn_ok.Name = "btn_ok";
            this.tablePanel2.SetRow(this.btn_ok, 0);
            this.btn_ok.Size = new System.Drawing.Size(96, 71);
            this.btn_ok.TabIndex = 0;
            this.btn_ok.Text = "확인 완료";
            this.btn_ok.Click += new System.EventHandler(this.btn_ok_Click);
            // 
            // groupControl1
            // 
            this.tablePanel3.SetColumn(this.groupControl1, 0);
            this.groupControl1.Controls.Add(this.radio_acoustic);
            this.groupControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupControl1.GroupStyle = DevExpress.Utils.GroupStyle.Light;
            this.groupControl1.Location = new System.Drawing.Point(13, 12);
            this.groupControl1.Name = "groupControl1";
            this.tablePanel3.SetRow(this.groupControl1, 0);
            this.groupControl1.Size = new System.Drawing.Size(175, 140);
            this.groupControl1.TabIndex = 0;
            this.groupControl1.Text = "Acoustic Alarm";
            // 
            // groupControl2
            // 
            this.tablePanel3.SetColumn(this.groupControl2, 1);
            this.groupControl2.Controls.Add(this.radio_fire);
            this.groupControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupControl2.GroupStyle = DevExpress.Utils.GroupStyle.Light;
            this.groupControl2.Location = new System.Drawing.Point(192, 12);
            this.groupControl2.Name = "groupControl2";
            this.tablePanel3.SetRow(this.groupControl2, 0);
            this.groupControl2.Size = new System.Drawing.Size(175, 140);
            this.groupControl2.TabIndex = 1;
            this.groupControl2.Text = "Fire Record";
            // 
            // radio_acoustic
            // 
            this.radio_acoustic.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radio_acoustic.Location = new System.Drawing.Point(2, 23);
            this.radio_acoustic.Name = "radio_acoustic";
            this.radio_acoustic.Properties.Items.AddRange(new DevExpress.XtraEditors.Controls.RadioGroupItem[] {
            new DevExpress.XtraEditors.Controls.RadioGroupItem(null, "미점등", true, null, "lamp_off"),
            new DevExpress.XtraEditors.Controls.RadioGroupItem(null, "점등", true, null, "lamp_on")});
            this.radio_acoustic.Size = new System.Drawing.Size(171, 115);
            this.radio_acoustic.TabIndex = 0;
            // 
            // radio_fire
            // 
            this.radio_fire.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radio_fire.Location = new System.Drawing.Point(2, 23);
            this.radio_fire.Name = "radio_fire";
            this.radio_fire.Properties.Items.AddRange(new DevExpress.XtraEditors.Controls.RadioGroupItem[] {
            new DevExpress.XtraEditors.Controls.RadioGroupItem(null, "미점등", true, null, "lamp_off"),
            new DevExpress.XtraEditors.Controls.RadioGroupItem(null, "점등", true, null, "lamp_on")});
            this.radio_fire.Size = new System.Drawing.Size(171, 115);
            this.radio_fire.TabIndex = 1;
            // 
            // LampChoiceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(406, 290);
            this.ControlBox = false;
            this.Controls.Add(this.tablePanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "LampChoiceForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "LampChoiceForm";
            ((System.ComponentModel.ISupportInitialize)(this.tablePanel1)).EndInit();
            this.tablePanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tablePanel2)).EndInit();
            this.tablePanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tablePanel3)).EndInit();
            this.tablePanel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).EndInit();
            this.groupControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).EndInit();
            this.groupControl2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.radio_acoustic.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radio_fire.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.Utils.Layout.TablePanel tablePanel1;
        private DevExpress.Utils.Layout.TablePanel tablePanel3;
        private DevExpress.Utils.Layout.TablePanel tablePanel2;
        private DevExpress.XtraEditors.SimpleButton btn_ok;
        private DevExpress.XtraEditors.GroupControl groupControl2;
        private DevExpress.XtraEditors.GroupControl groupControl1;
        private DevExpress.XtraEditors.RadioGroup radio_fire;
        private DevExpress.XtraEditors.RadioGroup radio_acoustic;
    }
}