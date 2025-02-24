using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Mvvm.Model.ComPort;
using Mvvm.ViewModels;
using NModbus;
using NModbus.Serial;

namespace Mvvm.Model;
internal class ModbusConnect
{
    private SerialPort port = null;
    private IModbusMaster master = null;
    public SerialPortConfig serialPortConfig { get; set; }

    private DispatcherTimer blinkTimer;
    private System.Timers.Timer timer;


    public ModbusConnect()
    {
        serialPortConfig = new SerialPortConfig();

        serialPortConfig.BaudRate = 9600;
        serialPortConfig.DataBits = 8;
        serialPortConfig.Parity = Parity.None;
        serialPortConfig.StopBits = StopBits.One;
        serialPortConfig.ReadTimeout = 1000;
        serialPortConfig.WriteTimeout = 1000;




        //blinkTimer = new DispatcherTimer();
        //blinkTimer.Interval = TimeSpan.FromMilliseconds(1000);
       // blinkTimer.Tick += BlinkTimer_Tick;

        timer = new System.Timers.Timer(500);
        timer.Elapsed += Timer_Elapsed;



    }

    public void StartTimer() => timer.Start();
    public void StopTimer()=> timer.Stop();
    //포트 이름 로드
    public void LoadAvailablePorts(ComboBox portComBox) =>        portComBox.ItemsSource = SerialPort.GetPortNames();

    public async Task ConnectToPort(string portName)
    {
        try
        {
            if (port != null && port.IsOpen)
            {
                port.Close();
                Thread.Sleep(100);
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

            await Task.Run(() => port.Open());

            var factory = new ModbusFactory();
            master = factory.CreateRtuMaster(port);
            master.Transport.ReadTimeout = 2000;
            master.Transport.WriteTimeout = 2000;

            Application.Current.Dispatcher.Invoke(() =>
                MessageBox.Show("연결에 성공했습니다.", "정보", MessageBoxButton.OK, MessageBoxImage.Information));
        }
        catch (Exception ex)
        {
            Application.Current.Dispatcher.Invoke(() =>
                MessageBox.Show($"포트 연결 실패: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error));
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


    private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        timer.Stop();
        await ReadRegistersAsync();
        timer.Start();
    }


    public async Task ReadRegistersAsync()
    {
        if (master == null || port == null || !port.IsOpen)
        {
            Application.Current.Dispatcher.Invoke(() =>
                MessageBox.Show("Modbus 마스터가 초기화되지 않았거나 포트가 열려 있지 않습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error));
            return;
        }


        try
        {
            byte slaveId = serialPortConfig.slaveId;
            ushort startAddress = serialPortConfig.startAddress;
            ushort numberOfPoints = serialPortConfig.numberOfPoints;
            ushort[] registers;

            switch (serialPortConfig.FunctionCode)
            {
                case 3:
                    registers = master.ReadHoldingRegisters(slaveId, startAddress, numberOfPoints);
                    var status = registers[1];  //일정 숫자 부터 숫자 까지; 
                    Console.WriteLine($"status: {status}");
                    break;

                case 4:
                    registers = master.ReadInputRegisters(slaveId, startAddress, numberOfPoints);
                    break;
                default:
                    Application.Current.Dispatcher.Invoke(() =>
                             MessageBox.Show("잘못된 기능 코드입니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error));
                    return;
            }

            foreach (var reg in registers)
            {
                Console.WriteLine($"Register: {reg}");
            }
        }

        catch (Exception ex)
        {
            Application.Current.Dispatcher.Invoke(() =>
                MessageBox.Show($"레지스터 읽기 실패: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error));
        }
    }

    public List<ViewModels.ParameterModel> ReadModbusData(int startAddress, int numberOfPoints) {

        var parameters = new List<ParameterModel>();
        for(int i = 0; i < numberOfPoints; i++)
        {
            double v = i + 1;
            parameters.Add(new ParameterModel
            {
                Label = $"ModbusTest {i + 1}",
                DefaultValue = "TestValue",
                DefaultActual = v
            });
        }

        return parameters;
    }


}
