using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Mvvm.Model.ComPort;
using Mvvm.Model.Exceptions;
using Mvvm.ViewModels;
using NModbus;
using NModbus.Serial;

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

          LoadDefaultConfig();
        }

        private void LoadDefaultConfig()
        {
            if (int.TryParse(Properties.Settings.Default.BaudRate, out int baudRate))
            {
                serialPortConfig.BaudRate = baudRate;
            }
            else
            {
                ShowMessage("Invalid baud rate in settings.", "Error", true);
            }

            if (int.TryParse(Properties.Settings.Default.DataBits, out int dataBits))
            {
                serialPortConfig.DataBits = dataBits;
            }
            else
            {
                ShowMessage("Invalid data bits in settings.", "Error", true);
            }

            if (Enum.TryParse(Properties.Settings.Default.Parity, out Parity parity))
            {
                serialPortConfig.Parity = parity;
            }
            else
            {
                ShowMessage("Invalid parity in settings.", "Error", true);
            }

            if (Enum.TryParse(Properties.Settings.Default.StopBits, out StopBits stopBits))
            {
                serialPortConfig.StopBits = stopBits;
            }
            else
            {
                ShowMessage("Invalid stop bits in settings.", "Error", true);
            }

            serialPortConfig.ReadTimeout = Properties.Settings.Default.ReadTimeout;
            serialPortConfig.WriteTimeout = Properties.Settings.Default.WriteTimeout;
            serialPortConfig.slaveId = Properties.Settings.Default.SlaveId;
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
             //   MessageBox.Show("포트 연결 시도 중...", "연결", MessageBoxButton.OK, MessageBoxImage.Information);
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

            MessageBox.Show(serialPortConfig.BaudRate.ToString(), "BaudRate", MessageBoxButton.OK, MessageBoxImage.Information);



            await Task.Run(() => 
            
            port.Open());

            var factory = new ModbusFactory();
            master = factory.CreateRtuMaster(port);
            master.Transport.ReadTimeout = 2000;
            master.Transport.WriteTimeout = 2000;
        }

        public async Task<List<ParameterModel>> ReadModbusData(int startAddress, int numberOfPoints)
        {
            if (!IsConnected())
            {

            //연결 안될경우에는 가장 최근값 가져올것
                var lastValues = dataBuffer.GetLastValues(numberOfPoints);  
                return lastValues;
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

                var lastValues = dataBuffer.GetLastValues(numberOfPoints);
                DataReceived?.Invoke(parameters);
                return lastValues;
            }
            catch (Exception ex)
            {
                statistics.RecordError();
                ShowMessage($"데이터 읽기 실패: {ex.Message}", "오류", true);
                var lastValues = dataBuffer.GetLastValues(numberOfPoints);
                return lastValues;
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

                statistics.RecordSuccessfulRead();
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
            List<ParameterModel> result = new List<ParameterModel>();

            var indexList = Properties.Settings.Default.Index;
            var descriptionList = Properties.Settings.Default.Description;
            var unitList = Properties.Settings.Default.Unit;
            var defaultValueList = Properties.Settings.Default.DefaultValue;



            for (int i = 0; i < registers.Length; i++)
            {
                ushort value = registers[i]; 
                int address = startAddress + i;

                DataType dataType = DataType.UInt16;
                dataTypeMap.TryGetValue((ushort)address, out dataType);

                ParameterModel parameter = new ParameterModel
                {
                    Address = address,
                    Label = $"Register {address}",
                    Description =  descriptionList.Cast<string>().ElementAt(indexList.IndexOf(address.ToString())),
                    DefaultActual = value,
                    DefaultValue = defaultValueList.Cast<string>().ElementAt(indexList.IndexOf(address.ToString())),
                    ModbusUnit = unitList.Cast<string>().ElementAt(indexList.IndexOf(address.ToString())),
                    Index = address
                };

                result.Add(parameter);
            }

            return result;
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
            master?.Dispose();
            port?.Dispose();
        }
    }
}
namespace Mvvm.Model
{
    public class ModbusDataBuffer
    {
        private readonly int bufferSize = 1000;
        private readonly Dictionary<int, Queue<DataPoint>> dataBuffer = new Dictionary<int, Queue<DataPoint>>();
        private readonly object lockObject = new object();

        public void StoreValues(List<ParameterModel> parameters)
        {
            lock (lockObject)
            {
                foreach (var parameter in parameters)
                {
                    if (!dataBuffer.ContainsKey(parameter.Address))
                    {
                        dataBuffer[parameter.Address] = new Queue<DataPoint>(bufferSize);
                    }

                    var queue = dataBuffer[parameter.Address];
                    if (queue.Count >= bufferSize)
                    {
                        queue.Dequeue();
                    }

                    queue.Enqueue(new DataPoint
                    {
                        Timestamp = DateTime.Now,
                        Value = parameter.DefaultActual

                    });
                }
            }
        }

        public List<ParameterModel> GetLastValues(int count)
        {
            lock (lockObject)
            {
                return dataBuffer.Values
                    .SelectMany(queue => queue)
                    .OrderByDescending(dp => dp.Timestamp)
                    .Take(count)
                    .Select(dp => new ParameterModel
                    {
                        DefaultActual = dp.Value,
                     //   DefaultValue = dp.Value.ToString(),
                        
                    })
                    .ToList();
            }
        }

        public DataPoint[] GetHistoricalData(int address, TimeSpan timeSpan)
        {
            lock (lockObject)
            {
                if (!dataBuffer.ContainsKey(address))
                {
                    return Array.Empty<DataPoint>();
                }

                var cutoff = DateTime.Now - timeSpan;
                return dataBuffer[address]
                    .Where(dp => dp.Timestamp >= cutoff)
                    .ToArray();
            }
        }
    }

    public class CommunicationStatistics
    {
        public int TotalReads { get; private set; }
        public int SuccessfulReads { get; private set; }
        public int ErrorCount { get; private set; }
        public int ReconnectAttempts { get; private set; }
        public int SuccessfulReconnects { get; private set; }
        public DateTime LastSuccessfulRead { get; private set; }
        public DateTime LastError { get; private set; }

        public double SuccessRate => TotalReads == 0 ? 0 : (double)SuccessfulReads / TotalReads * 100;
        public double ReconnectSuccessRate => ReconnectAttempts == 0 ? 0 : (double)SuccessfulReconnects / ReconnectAttempts * 100;

        public void RecordSuccessfulRead()
        {
            TotalReads++;
            SuccessfulReads++;
            LastSuccessfulRead = DateTime.Now;
        }

        public void RecordError()
        {
            TotalReads++;
            ErrorCount++;
            LastError = DateTime.Now;
        }

        public void RecordReconnectAttempt()
        {
            ReconnectAttempts++;
        }

        public void RecordReconnectSuccess()
        {
            SuccessfulReconnects++;
        }

        public void RecordReconnectFailure()
        {
            // 재연결 실패 통계 기록
        }

        public void Reset()
        {
            TotalReads = 0;
            SuccessfulReads = 0;
            ErrorCount = 0;
            ReconnectAttempts = 0;
            SuccessfulReconnects = 0;
        }
    }

    public class DataPoint
    {
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
    }

    public enum DataType
    {
        UInt16,
        Int16,
        UInt32,
        Int32,
        Float
    }
}
