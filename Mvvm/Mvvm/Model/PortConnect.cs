using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NModbus;
using System.Windows;
using Mvvm.Model.ComPort;

namespace Mvvm.Model
{
    public class PortConnect 
    {




        private SerialPort port = null;

        public SerialPortConfig serialPortConfig { get; set; }



        public async Task ConnectToPort(string portName)
        {
            try
            {
                if (port != null && port.IsOpen)
                {
                    port.Close();
                }

                port = new SerialPort(portName)
                {
                    BaudRate = serialPortConfig.BaudRate,
                    DataBits = serialPortConfig.DataBits,
                    Parity = serialPortConfig.Parity,
                    StopBits = serialPortConfig.StopBits,
                    ReadTimeout = serialPortConfig.ReadTimeout,
                    WriteTimeout = serialPortConfig.WriteTimeout
                };



                await Task.Run(() =>
                {
                    try
                    {
                        port.Open();
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        throw new Exception($"포트 {portName}에 접근할 수 없습니다. (다른 프로그램에서 사용 중일 가능성 있음)\n{ex.Message}");
                    }
                    catch (IOException ex)
                    {
                        throw new Exception($"포트 {portName}를 열 수 없습니다. (존재하지 않거나 응답 없음)\n{ex.Message}");
                    }
                });

                var factory = new ModbusFactory();
               // master = factory.CreateRtuMaster(port);
               // master.Transport.ReadTimeout = 2000;
              //  master.Transport.WriteTimeout = 2000;

                MessageBox.Show("연결에 성공했습니다.", "정보", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"포트 연결 실패: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




    }
}
