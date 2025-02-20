using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using Mvvm.Enum;
namespace Mvvm.Model.ComPort
{
    public class SerialPortConfig
    {

        public int BaudRate { get; set; }
        public int DataBits { get; set; }
        public Parity Parity { get; set; }
        public StopBits StopBits{ get; set; }
        public int ReadTimeout { get; set; }
        public int WriteTimeout { get; set; }


          public static readonly Dictionary<baudEnum, (string Name, string Hex)> Colors = new()
    {
        { baudEnum.Baud9600, ("Orange", "#FFFF00") },
        { baudEnum.Baud19200, ("Green", "#7853DE") },
        { baudEnum.Baud38400, ("Yellow", "#7853DE") },
        { baudEnum.Baud57600, ("Blue", "#7853DE") },
        { baudEnum.Baud115200, ("Red", "#7853DE") },


    };



        public SerialPortConfig()
        {

           

        }

        public SerialPortConfig(int baudRate, int dataBits, Parity parity, StopBits stopBits, int readTimeout, int writeTimeout)
        {
            BaudRate = baudRate;
            DataBits = dataBits;
            Parity = parity;
            StopBits = stopBits;
            ReadTimeout = readTimeout;
            WriteTimeout = writeTimeout;
        }
    }
}



