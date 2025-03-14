using DevExpress.Data.TreeList;
using DevExpress.DirectX.Common.Direct2D;
using DevExpress.Utils.VisualEffects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsanSimulatorGUI.Modules
{
    public enum fcu_modbus
    {
        version,
        ctrl_code,
        fire_time,
        voltage_fcu,
        current,
        power,
        resistor,
        voltage_fire,
        voltage_fault,
        voltage_adj,
        custom_code
    }
    enum enum_ctrl
    {
        remote,
        time_measure
    }
    enum enum_custom
    {
        relay00,
        relay01,
        relay02,
        relay03,
        relay04,
        relay05,
        photo_trig,
        power_on,
        custom08
    }
    public class FCUData
    {
        public string version {  get; set; }
        public UInt16 ctrl_code { get; set; }

        public bool ctrl_remote {  get; set; }
        public bool ctrl_time_measuring { get; set; }

        public UInt16 time_fire {  get; set; }
        public UInt16 voltage_fcu {  get; set; }
        public UInt16 current {  get; set; }
        public UInt16 power { get; set; }
        public UInt16 resistor { get; set; }
        public UInt16 voltage_fault { get; set; }
        public UInt16 voltage_fire { get; set; }
        public UInt16 voltage_fcu_adj { get; set; }
        public UInt16 custom_code { get; set; }

        public bool relay_00 {  get; set; }// true -> ON false -> OFF
        public bool relay_01 {  get; set; }// bit relay ON/OFF 화재반응 측정 시 사용가능
        public bool relay_02 {  get; set; }// 1.0kΩ
        public bool relay_03 {  get; set; }// 2.0kΩ
        public bool relay_04 {  get; set; }// 2.2kΩ
        public bool relay_05 {  get; set; }// 2.4kΩ
        public bool photo_trig {  get; set; }//응답성 시험용
        public bool fcu_power_on {  get; set; }// true -> ON false -> OFF
        public bool custom_code_08 { get; set; }// fire alram 반응시간 측정 relay_01은 무시하고 이걸로 하면 됨

        
        public FCUData()
        {
        
        }

        public void set_relay(int resister)
        {

         #region 처음에 이렇게 초기화 해 줘야함
         relay_02 = false;
         relay_03 = false;
         relay_04 = false;
         relay_05 = false;
         relay_00 = false;
         custom_code_08 = false;
         #endregion

         fcu_power_on = true;
            switch (resister)
            {
                case 0://just power on 다른 것들도 해당사항이므로 switch이전에 power on해놓음

                    break;
                case 1000: //1kΩ
                    relay_02 = true;
                    break;
                case 2000: //2kΩ
                    relay_03 = true;
                    break;
                case 2200: //2.2kΩ
                    relay_04 = true;
                    break;
                case 2400: //2.4kΩ
                    relay_05 = true;
                    break;
                case 10000: //단선 fault relay
                    relay_00 = true;
                    break;
                case 20000: //fire bit   //반응시간 검사시 사용
                    custom_code_08 = true;
                    break;
            }
        }

      public void get_fcu_version(Span<byte> recvdatas) {
         // 통신이 새로 연결 될 때 마다 사용함
         byte[] bytes = recvdatas.ToArray();
         byte[] data = new byte[2];

         Array.Copy(bytes,0,data,0,data.Length);
         data = flip_2byte(data);

         int major = (data[1] >> 4) & 0x0F;
         int minor1 = data[1] & 0x0F;
         int minor2 = (data[0] >> 4) & 0x0F;
         int minor3 = data[0] & 0x0F;

         int versionNumber;
         if(minor2 == 0 && minor3 == 0) {
            versionNumber = major * 10000 + minor1 * 100;
            } else {
            versionNumber = major * 10000 + minor1 * 100 + minor2 * 10 + minor3;
            }
         version = (versionNumber / 100.0).ToString("F2");

         }


      public void get_fcu_data(Span<byte> recvdatas)
        {
            byte[] bytes = recvdatas.ToArray();
            int start_index = 0;
            byte[] data = new byte[2];

            Array.Copy(bytes, start_index, data, 0, data.Length);
            data = flip_2byte(data);
            ctrl_code = BitConverter.ToUInt16(data, 0);
            ctrl_remote = get_bit(ctrl_code, (UInt16)enum_ctrl.remote);
            ctrl_time_measuring = get_bit(ctrl_code, (UInt16)enum_ctrl.time_measure);
            start_index += sizeof(UInt16);

            Array.Copy(bytes, start_index, data, 0, data.Length);
            data = flip_2byte(data);
            time_fire = BitConverter.ToUInt16(data, 0);
            start_index += sizeof(UInt16);

            Array.Copy(bytes, start_index, data, 0, data.Length);
            data = flip_2byte(data);
            voltage_fcu = BitConverter.ToUInt16(data, 0);
            start_index += sizeof(UInt16);

            Array.Copy(bytes, start_index, data, 0, data.Length);
            data = flip_2byte(data);
            current = BitConverter.ToUInt16(data, 0);
            start_index += sizeof(UInt16);

            Array.Copy(bytes, start_index, data, 0, data.Length);
            data = flip_2byte(data);
            power = BitConverter.ToUInt16(data, 0);
            start_index += sizeof(UInt16);

            Array.Copy(bytes, start_index, data, 0, data.Length);
            data = flip_2byte(data);
            resistor = BitConverter.ToUInt16(data, 0);
            start_index += sizeof(UInt16);

            Array.Copy(bytes, start_index, data, 0, data.Length);
            data = flip_2byte(data);
         voltage_fire = BitConverter.ToUInt16(data,0);
         start_index += sizeof(UInt16);

            Array.Copy(bytes, start_index, data, 0, data.Length);
            data = flip_2byte(data);
     
         voltage_fault = BitConverter.ToUInt16(data,0);
         start_index += sizeof(UInt16);

            Array.Copy(bytes, start_index, data, 0, data.Length);
            data = flip_2byte(data);
            voltage_fcu_adj = BitConverter.ToUInt16(data, 0);
            start_index += sizeof(UInt16);

            Array.Copy(bytes, start_index, data, 0, data.Length);
            data = flip_2byte(data);
            custom_code = BitConverter.ToUInt16(data, 0);
            
            relay_00 = get_bit(custom_code, (UInt16)enum_custom.relay00);
            relay_01 = get_bit(custom_code, (UInt16)enum_custom.relay01);
            relay_02 = get_bit(custom_code, (UInt16)enum_custom.relay02);
            relay_03 = get_bit(custom_code, (UInt16)enum_custom.relay03);
            relay_04 = get_bit(custom_code, (UInt16)enum_custom.relay04);
            relay_05 = get_bit(custom_code, (UInt16)enum_custom.relay05);
            photo_trig = get_bit(custom_code, (UInt16)enum_custom.photo_trig);
            fcu_power_on = get_bit(custom_code, (UInt16)enum_custom.power_on);
            custom_code_08 = get_bit(custom_code, (UInt16)enum_custom.custom08);

        }

        public byte[] set_fcu_data()
        {
            List<byte> data = new List<byte>();
            byte[] bytes = new byte[2];

            bytes = flip_2byte(BitConverter.GetBytes((UInt16)(voltage_fcu_adj)));
            data.Add(bytes[0]);
            data.Add(bytes[1]);


            custom_code = set_bit(custom_code, (UInt16)enum_custom.relay00, relay_00);
            custom_code = set_bit(custom_code, (UInt16)enum_custom.relay01, relay_01);
            custom_code = set_bit(custom_code, (UInt16)enum_custom.relay02, relay_02);
            custom_code = set_bit(custom_code, (UInt16)enum_custom.relay03, relay_03);
            custom_code = set_bit(custom_code, (UInt16)enum_custom.relay04, relay_04);
            custom_code = set_bit(custom_code, (UInt16)enum_custom.relay05, relay_05);
            custom_code = set_bit(custom_code, (UInt16)enum_custom.photo_trig, photo_trig);
            custom_code = set_bit(custom_code, (UInt16)enum_custom.power_on, fcu_power_on);
            custom_code = set_bit(custom_code, (UInt16)enum_custom.custom08, custom_code_08);
            bytes = flip_2byte(BitConverter.GetBytes((UInt16)(custom_code)));
            data.Add(bytes[0]);
            data.Add(bytes[1]);

            return data.ToArray();
        }

        byte[] flip_2byte(byte[] bytes)
        {
            Array.Reverse(bytes);
            return bytes;
        }

        UInt16 set_bit(UInt16 value, UInt16 loc, bool status)
        {
            int temp = value;
            if (status)
            {
                temp |= 1 << loc;
            }
            else
            {
                temp &= ~(1 << loc);
            }
            return (UInt16)temp;
        }

        bool get_bit(UInt16 data, UInt16 index)
        {
            if (((data >> index) & 0x0001) == 1) return true;
            else return false;
        }
    }
}
