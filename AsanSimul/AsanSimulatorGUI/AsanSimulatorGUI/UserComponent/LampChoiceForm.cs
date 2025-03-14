using AsanSimulatorGUI.FCU_ORM;
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
    public partial class LampChoiceForm : DevExpress.XtraEditors.XtraForm
    {
        public FCUTest fcutest;

        public LampChoiceForm()
        {
            InitializeComponent();
        }

        public void set_fcutest(FCUTest fcutest)
        {
            this.fcutest = fcutest;
        }

        private void btn_ok_Click(object sender, EventArgs e)
        {
            string message = $"선택하신 램프상태는 아래와 같습니다.{Environment.NewLine}" +
                $"Acoustic Lamp : {check_radio_group_index(radio_acoustic.SelectedIndex)}{Environment.NewLine}" +
                $"Fire Lamp : {check_radio_group_index(radio_fire.SelectedIndex)}{Environment.NewLine}" +
                $"램프 점등 선택이 맞다면 Yes를 누르고 다음단계로 진행해주시기 바랍니다.";
            if (
                XtraMessageBox.Show(message, "램프 재확인", MessageBoxButtons.YesNo)
                == DialogResult.Yes
                )
            {
                fcutest.SMALL_TESTs[0].measure_value = (UInt16)radio_acoustic.SelectedIndex;
                fcutest.SMALL_TESTs[1].measure_value = (UInt16)radio_fire.SelectedIndex;
                this.Close();
                this.Dispose();
            }
        }

        string check_radio_group_index(int index)
        {
            string status = "";
            switch (index)
            {
                case 1:
                    status = "점등";
                    break;
                case 0:
                    status = "미점등";
                    break;
            }
            return status;
        }
    }
}