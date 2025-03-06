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
using System.Timers;
using Mvvm.Model.Exceptions;

namespace Mvvm.Model
{
    public class ModbusConnect
    {
        public event Action<bool> ConnectionStatusChanged;
        public event Action<List<ParameterModel>> DataReceived;

        private SerialPort port = null;
        private IModbusMaster master = null;
        private readonly int MAX_RECONNECT_ATTEMPTS = 3;
        private readonly Dictionary<ushort, DataType> dataTypeMap = new Dictionary<ushort, DataType>();
        private readonly CommunicationStatistics statistics;
        private readonly ModbusDataBuffer dataBuffer;
        private bool autoReconnect = true;
        private System.Timers.Timer reconnectTimer;
        private DateTime lastDataReceived;

        public SerialPortConfig serialPortConfig { get; set; }
        public CommunicationStatistics Statistics => statistics;



        protected virtual void OnConnectionStatusChanged(bool isConnected)
        {
            ConnectionStatusChanged?.Invoke(isConnected);
        }


        public ModbusConnect()
        {
            serialPortConfig = new SerialPortConfig();
            statistics = new CommunicationStatistics();
            dataBuffer = new ModbusDataBuffer();



            InitializeReconnectTimer();
            LoadDefaultConfig();
        }

        private void LoadDefaultConfig()
        {
         
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

        private void InitializeReconnectTimer()
        {
            reconnectTimer = new System.Timers.Timer(5000); // 5초마다 재시도
            reconnectTimer.Elapsed += async (s, e) => await TryReconnect();
            reconnectTimer.AutoReset = true;
        }

        public void LoadAvailablePorts(ComboBox portComBox)
        {
            portComBox.ItemsSource = SerialPort.GetPortNames();
        }


        public string portName;
        public async Task ConnectToPort(string _portName)
        {
            portName = _portName;

            try
            {
                await DisconnectIfConnected();
                await OpenNewConnection(_portName);
                ConnectionStatusChanged?.Invoke(true);
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

        private async Task TryReconnect()
        {
            if (!autoReconnect || IsConnected()) return;

            for (int attempt = 1; attempt <= MAX_RECONNECT_ATTEMPTS; attempt++)
            {
                try
                {
                    await ConnectToPort(port?.PortName);
                    statistics.RecordReconnectSuccess();
                    return;
                }
                catch (Exception ex)
                {
                    statistics.RecordReconnectFailure();
                    if (attempt == MAX_RECONNECT_ATTEMPTS)
                    {
                        ShowMessage($"재연결 실패 ({attempt}/{MAX_RECONNECT_ATTEMPTS}): {ex.Message}", "오류", true);
                    }
                    await Task.Delay(1000 * attempt); // 지수 백오프
                }
            }
        }

        public async Task<List<ParameterModel>> ReadModbusData(int startAddress, int numberOfPoints)
        {
            if (!IsConnected())
            {
                if (autoReconnect && !reconnectTimer.Enabled)
                {
                    reconnectTimer.Start();
                }
                return dataBuffer.GetLastValues(numberOfPoints);
            }

            try
            {
                var registers = await Task.Run(() =>
                    master.ReadHoldingRegisters(
                        serialPortConfig.slaveId,
                        (ushort)startAddress,
                        (ushort)numberOfPoints));

                statistics.RecordSuccessfulRead();
                lastDataReceived = DateTime.Now;

                var parameters = ConvertToParameters(registers, startAddress);
                dataBuffer.StoreValues(parameters);

                DataReceived?.Invoke(parameters);
                return parameters;
            }
            catch (Exception ex)
            {
                statistics.RecordError();
                ShowMessage($"데이터 읽기 실패: {ex.Message}", "오류", true);
                return dataBuffer.GetLastValues(numberOfPoints);
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

                statistics.RecordSuccessfulRead(); // 쓰기 성공도 기록
            }
            catch (Exception ex)
            {
                statistics.RecordError();
                throw new Exception($"쓰기 실패: {ex.Message}");
            }
        }

        public async Task<double> ReadRegisterAsType(ushort address, DataType dataType)
        {
            try
            {
                switch (dataType)
                {
                    case DataType.Float:
                        var registers = await Task.Run(() =>
                            master.ReadHoldingRegisters(serialPortConfig.slaveId, address, 2));
                        return ModbusDataConverter.ToFloat(registers);

                    case DataType.Int32:
                        registers = await Task.Run(() =>
                            master.ReadHoldingRegisters(serialPortConfig.slaveId, address, 2));
                        return ModbusDataConverter.ToInt32(registers);

                    default:
                        var register = await Task.Run(() =>
                            master.ReadHoldingRegisters(serialPortConfig.slaveId, address, 1));
                        return register[0];
                }
            }
            catch (Exception ex)
            {
                statistics.RecordError();
                throw new ModbusException($"레지스터 {address} 읽기 실패: {ex.Message}", ex);
            }
        }

        public bool IsConnected()
        {
            return master != null && port != null && port.IsOpen;
        }

        private List<ParameterModel> ConvertToParameters(ushort[] registers, int startAddress)
        {
            return registers.Select((value, index) =>
            {
                var address = startAddress + index;
                DataType dataType = DataType.UInt16;
                dataTypeMap.TryGetValue((ushort)address, out dataType);

                return new ParameterModel
                {
                    Address = address,
                    Label = $"Register {address}",
                    DefaultActual = value,
                    DefaultValue = value.ToString(),
                    ModbusUnit = dataType.ToString()
                };
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

        public void RegisterDataType(ushort address, DataType dataType)
        {
            dataTypeMap[address] = dataType;
        }

        private void ShowMessage(string message, string title, bool isError = false)
        {
            Application.Current.Dispatcher.Invoke(() =>
                MessageBox.Show(message, title,
                    MessageBoxButton.OK,
                    isError ? MessageBoxImage.Error : MessageBoxImage.Information));
        }




        public void Dispose()
        {
            reconnectTimer?.Dispose();
            master?.Dispose();
            port?.Dispose();
        }
    }
}
