using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Configuration;
using System.ServiceModel.Channels;
using System.Windows;
using Google.Protobuf.WellKnownTypes;


namespace Mvvm.Model.ComPort
{
    public class SerialPortConfig
    {

        public int BaudRate
        {
            get;
            set;

        }

        public int DataBits
        {
            get;
            set;

        }

        public Parity Parity
        {
            get;


            set;

        }

        public StopBits StopBits
        {
            get;
            set;

        }

        public int ReadTimeout
        {
            get; set;

        }
        public int WriteTimeout
        {
            get;

            set;

        }



        //임시 
        public byte slaveId = 1;
        public ushort startAddress = 0x0000;
        public ushort numberOfPoints = 0x0064;
        public int FunctionCode = 3;
        //



        public void SaveSerialPortconfig()
        {

            Properties.Settings.Default.BaudRate = BaudRate;
            Properties.Settings.Default.DataBits = DataBits;
            Properties.Settings.Default.Parity = Parity.ToString();
            Properties.Settings.Default.StopBits = StopBits.ToString();
            Properties.Settings.Default.ReadTimeout = ReadTimeout;
            Properties.Settings.Default.WriteTimeout = WriteTimeout;
            Properties.Settings.Default.Save();

        }


        public void LoadSerialPortConfig()
        {
            BaudRate = Properties.Settings.Default.BaudRate;
            DataBits = Properties.Settings.Default.DataBits;
            Parity = (Parity)System.Enum.Parse(typeof(Parity), Properties.Settings.Default.Parity);
            StopBits = (StopBits)System.Enum.Parse(typeof(StopBits), Properties.Settings.Default.StopBits);
            ReadTimeout = Properties.Settings.Default.ReadTimeout;
            WriteTimeout = Properties.Settings.Default.WriteTimeout;
        }


        //      public static readonly Dictionary<baudEnum, (string Name, string Hex)> Colors = new()
        //{
        //    { baudEnum.Baud9600, ("Orange", "#FFFF00") },
        //    { baudEnum.Baud19200, ("Green", "#7853DE") },
        //    { baudEnum.Baud38400, ("Yellow", "#7853DE") },
        //    { baudEnum.Baud57600, ("Blue", "#7853DE") },
        //    { baudEnum.Baud115200, ("Red", "#7853DE") },


        //};


    }
}



