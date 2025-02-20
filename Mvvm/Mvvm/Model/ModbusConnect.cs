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
using NModbus;
using NModbus.Serial;

namespace Mvvm.Model;
internal class ModbusConnect
{
    private SerialPort port = null;
    private IModbusMaster master = null;

    //포트 이름 로드
    public void LoadAvailablePorts(ComboBox portComBox)
    {
        var ports = SerialPort.GetPortNames();
        portComBox.ItemsSource = ports;
    }

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
                BaudRate = 9600,
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One,
                ReadTimeout = 2000,
                WriteTimeout = 2000
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

        byte slaveId = 2;
        ushort startAddress = 0x0000;
        ushort numberOfPoints = 0x000E;
        var registers = await Task.Run(() => master.ReadHoldingRegisters(slaveId, startAddress, numberOfPoints));

        if (registers.Length == 14)
        {
            //var status = registers[1];
            //this.Sensor.LeakR = (status & 0x02) != 0;
            //this.Sensor.LeakS = (status & 0x04) != 0;
            //this.Sensor.LeakT = (status & 0x08) != 0;
            //this.Sensor.OverCurrent = (status & 0x10) != 0;
            //this.Sensor.OverTemperature = (status & 0x20) != 0;

            //CheckAlarms(status);

            //this.Sensor.LeakRVoltage = registers[2];
            //this.Sensor.LeakSVoltage = registers[3];
            //this.Sensor.LeakTVoltage = registers[4];
            //// this.Sensor.Frequency = registers[5] / 10m;
            //// this.Sensor.Current = registers[6] / 10m;
            //this.Sensor.Current = ConvertToSignedValue(registers[6]);
            //this.Sensor.TemperatureValue = registers[7] / 10m;
            //// this.Sensor.LeakRStandardVoltage = registers[8];
            //// this.Sensor.LeakSStandardVoltage = registers[9];
            //// this.Sensor.LeakTStandardVoltage = registers[10];
            //// this.Sensor.MaxCT = registers[11];
            //// this.Sensor.OverCurrentStandard = registers[12];
            ////this.Sensor.OverTemperatureStandard = registers[13];

            //// Update colors
            //this.Sensor.LeakRTextColor = this.Sensor.LeakRVoltage < 220 ? Brushes.Blue : Brushes.Red;
            //this.Sensor.LeakSTextColor = this.Sensor.LeakSVoltage < 220 ? Brushes.Blue : Brushes.Red;
            //this.Sensor.LeakTTextColor = this.Sensor.LeakTVoltage < 220 ? Brushes.Blue : Brushes.Red;
            //this.Sensor.CurrentTextColor = this.Sensor.Current < 50 ? Brushes.Blue : Brushes.Red;

            //if (this.Sensor.TemperatureValue < 50)
            //{
            //    this.Sensor.TemperatureTextColor = Brushes.Green;
            //}
            //else if (this.Sensor.TemperatureValue < 70)
            //{
            //    this.Sensor.TemperatureTextColor = Brushes.Yellow;
            //}
            //else if (this.Sensor.TemperatureValue < 90)
            //{
            //    this.Sensor.TemperatureTextColor = Brushes.Orange;
            //}
            //else
            //{
            //    this.Sensor.TemperatureTextColor = Brushes.Red;
            //}

            //this.Sensor.LeakRColor = this.Sensor.LeakRVoltage > 220 ? Brushes.Green : Brushes.Red;
            //this.Sensor.LeakSColor = this.Sensor.LeakSVoltage > 220 ? Brushes.Green : Brushes.Red;
            //this.Sensor.LeakTColor = this.Sensor.LeakTVoltage > 220 ? Brushes.Green : Brushes.Red;
            //this.Sensor.CurrentColor = this.Sensor.Current > 50 ? Brushes.Yellow :
            //                           this.Sensor.Current > 100 ? Brushes.Red : Brushes.Green;
            //this.Sensor.TemperatureColor = this.Sensor.TemperatureValue < 50 ? Brushes.Green :
            //                          this.Sensor.TemperatureValue < 70 ? Brushes.Yellow :
            //                          this.Sensor.TemperatureValue < 90 ? Brushes.Orange : Brushes.Red;
        }
        await Task.Delay(100);
    }

}
