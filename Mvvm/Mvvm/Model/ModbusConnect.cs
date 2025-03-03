using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NModbus;
using System.Linq;
using Mvvm.ViewModels;
using Mvvm.Model.ComPort;
using NModbus.Serial;

namespace Mvvm.Model
{
    public class ModbusConnect
    {
        public event Action<bool> ConnectionStatusChanged;
        public event Action<List<ParameterModel>> DataReceived;

        private SerialPort port = null;
        private IModbusMaster master = null;
        public SerialPortConfig serialPortConfig { get; set; }

        public ModbusConnect()
        {
            serialPortConfig = new SerialPortConfig();
            LoadDefaultConfig();
        }

        private void LoadDefaultConfig()
        {
            serialPortConfig.LoadSerialPortConfig();
            if (serialPortConfig.BaudRate == 0)
            {
                serialPortConfig.BaudRate = 9600;
                serialPortConfig.DataBits = 8;
                serialPortConfig.Parity = Parity.None;
                serialPortConfig.StopBits = StopBits.One;
                serialPortConfig.ReadTimeout = 1000;
                serialPortConfig.WriteTimeout = 1000;
                serialPortConfig.slaveId = 1;
            }
        }

        public void LoadAvailablePorts(ComboBox portComBox)
        {
            portComBox.ItemsSource = SerialPort.GetPortNames();
        }

        public async Task ConnectToPort(string portName)
        {
            try
            {
                await DisconnectIfConnected();
                await OpenNewConnection(portName);

                ConnectionStatusChanged?.Invoke(true);
                ShowMessage("연결에 성공했습니다.", "정보");
            }
            catch (Exception ex)
            {
                ConnectionStatusChanged?.Invoke(false);
                ShowMessage($"포트 연결 실패: {ex.Message}", "오류", true);
            }
        }

        private async Task DisconnectIfConnected()
        {
            if (port != null && port.IsOpen)
            {
                port.Close();
                await Task.Delay(100);
            }
        }

        private async Task OpenNewConnection(string portName)
        {
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
        }

        public async Task<List<ParameterModel>> ReadModbusData(int startAddress, int numberOfPoints)
        {
            if (!IsConnected())
                return CreateDummyData(numberOfPoints);

            try
            {
                var registers = await Task.Run(() =>
                    master.ReadHoldingRegisters(
                        serialPortConfig.slaveId,
                        (ushort)startAddress,
                        (ushort)numberOfPoints));

                return ConvertToParameters(registers, startAddress);
            }
            catch (Exception ex)
            {
                ShowMessage($"데이터 읽기 실패: {ex.Message}", "오류", true);
                return CreateDummyData(numberOfPoints);
            }
        }

        public async Task WriteRegister(ParameterModel parameter, int value)
        {
            if (!IsConnected())
                throw new InvalidOperationException("연결되지 않았습니다.");

            try
            {
                await Task.Run(() =>
                    master.WriteSingleRegister(
                        serialPortConfig.slaveId,
                        (ushort)parameter.Address,
                        (ushort)value));
            }
            catch (Exception ex)
            {
                throw new Exception($"쓰기 실패: {ex.Message}");
            }
        }

        private bool IsConnected()
        {
            return master != null && port != null && port.IsOpen;
        }

        private List<ParameterModel> ConvertToParameters(ushort[] registers, int startAddress)
        {
            return registers.Select((value, index) => new ParameterModel
            {
                Address = startAddress + index,
                Label = $"Register {startAddress + index}",
                DefaultActual = value,
                DefaultValue = value.ToString(),
                ModbusUnit = "Raw"
            }).ToList();
        }

        private List<ParameterModel> CreateDummyData(int count)
        {
            return Enumerable.Range(0, count)
                .Select(i => new ParameterModel
                {
                    Label = $"Offline {i + 1}",
                    DefaultValue = "N/A",
                    DefaultActual = 0,
                    ModbusUnit = "N/A"
                }).ToList();
        }

        private void ShowMessage(string message, string title, bool isError = false)
        {
            Application.Current.Dispatcher.Invoke(() =>
                MessageBox.Show(message, title,
                    MessageBoxButton.OK,
                    isError ? MessageBoxImage.Error : MessageBoxImage.Information));
        }
    }
}
