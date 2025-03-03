using System;
using System.IO;
using System.IO.Ports;
using System.Text.Json;
using System.Windows;

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

        public void SaveSerialPortconfig()
        {
            try
            {
                var json = JsonSerializer.Serialize(this);
                File.WriteAllText(CONFIG_FILE, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"설정 저장 실패: {ex.Message}", "오류",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void LoadSerialPortConfig()
        {
            try
            {
                if (File.Exists(CONFIG_FILE))
                {
                    var json = File.ReadAllText(CONFIG_FILE);
                    var config = JsonSerializer.Deserialize<SerialPortConfig>(json);
                    CopyFrom(config);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"설정 로드 실패: {ex.Message}", "오류",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CopyFrom(SerialPortConfig config)
        {
            BaudRate = config.BaudRate;
            DataBits = config.DataBits;
            Parity = config.Parity;
            StopBits = config.StopBits;
            ReadTimeout = config.ReadTimeout;
            WriteTimeout = config.WriteTimeout;
            slaveId = config.slaveId;
            startAddress = config.startAddress;
            numberOfPoints = config.numberOfPoints;
            FunctionCode = config.FunctionCode;
        }
    }
}
