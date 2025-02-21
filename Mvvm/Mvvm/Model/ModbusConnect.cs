using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Mvvm.Model.ComPort;
using NModbus;
using NModbus.Serial;

namespace Mvvm.Model;
internal class ModbusConnect
{
    private SerialPort port = null;
    private IModbusMaster master = null;
    public SerialPortConfig serialPortConfig { get; set; }


    public ModbusConnect()
    {
        serialPortConfig = new SerialPortConfig();

        serialPortConfig.BaudRate = 9600;
        serialPortConfig.DataBits = 8;
        serialPortConfig.Parity = Parity.None;
        serialPortConfig.StopBits = StopBits.One;
        serialPortConfig.ReadTimeout = 1000;
        serialPortConfig.WriteTimeout = 1000;

    }


    //포트 이름 로드
    public void LoadAvailablePorts(ComboBox portComBox)
    {
        var ports = SerialPort.GetPortNames();
        portComBox.ItemsSource = ports;
    }

    public async Task ConnectToPort(string portName)
    {

      int baudrate = 9600;
        int databits = 8;
        Parity parity = Parity.None;
        StopBits stopbits = StopBits.One;
        int readTimeout = 1000;
        int writeTimeout = 1000;

        try
        {
            if (port != null && port.IsOpen)
            {
                port.Close();
            }

            port = new SerialPort(portName)
            {
                BaudRate = baudrate,
                DataBits = databits,
                Parity = Parity.None,
                StopBits = StopBits.One,
                ReadTimeout = 1000,
                WriteTimeout = 1000
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
            master = factory.CreateRtuMaster(port);
            master.Transport.ReadTimeout = 2000;
            master.Transport.WriteTimeout = 2000;

            MessageBox.Show("연결에 성공했습니다.", "정보", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"포트 연결 실패: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void PortReset_Click()
    {
        try
        {
            if (port != null)
            {
                if (port.IsOpen)
                {
                    port.Close();
                    Thread.Sleep(100);
                }
                else
                {
                    return;
                }
                port.Dispose();
                port = null;
            }
            else
            {

                return;
            }
            MessageBox.Show("포트가 정상적으로 닫혔습니다.", "정보", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (IOException ex)
        {
            MessageBox.Show($"입출력 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"알 수 없는 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public async Task ReadRegistersAsync()
    {
        if (master == null)
        {
            MessageBox.Show("모드버스 읽기관련", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        byte slaveId = serialPortConfig.slaveId;
        ushort startAddress = serialPortConfig.startAddress;
        ushort numberOfPoints = serialPortConfig.numberOfPoints;


        switch(serialPortConfig.FunctionCode)
        {
            case 3:
                var registers = await Task.Run(() => master.ReadHoldingRegisters(slaveId, startAddress, numberOfPoints));
                break;
            case 4:
                 registers = await Task.Run(() => master.ReadInputRegisters(slaveId, startAddress, numberOfPoints));
                break;
            default:
                MessageBox.Show("잘못된 기능 코드입니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
        }


        await Task.Delay(100);
    }

}
