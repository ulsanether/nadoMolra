using System;
using System.IO;
using System.IO.Ports;
using System.Text.Json;
using System.Windows;
using Mvvm.Model.IniFileRead;


namespace Mvvm.Model.ComPort
{



    public class SerialPortConfig
    {
        public int BaudRate { get; set; }
        public int DataBits { get; set; }
        public Parity Parity { get; set; }
        public StopBits StopBits { get; set; }
        public int ReadTimeout { get; set; }
        public int WriteTimeout { get; set; }
        public byte slaveId { get; set; } = 1;
        public ushort startAddress { get; set; } = 0x0000;
        public ushort numberOfPoints { get; set; } = 0x0064;
        public int FunctionCode { get; set; } = 3;

        private const string CONFIG_FILE = "serialport.config";

  
    }
}
