using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Mvvm.Model.ComPort;
using Mvvm.Model.Exceptions;
using Mvvm.ViewModels;
using MySqlX.XDevAPI.Common;
using NModbus;
using NModbus.Serial;
using ScottPlot;
using System.Diagnostics;
using Mvvm.Converters;
using System.Collections.ObjectModel;
using NLog;

namespace Mvvm.Model
{
    public class ModbusConnect
    {
        public event Action<bool> ConnectionStatusChanged;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        #region 필드
        private bool autoReconnect = true;

        private readonly int MAX_RECONNECT_ATTEMPTS = 3;
        private readonly Dictionary<ushort, DataType> dataTypeMap = new Dictionary<ushort, DataType>();
        public CommunicationStatistics statistics { get; set; }

        private DateTime lastDataReceived;
        private IModbusMaster ?master = null;
        private SerialPort ?port = null;

        private StringCollection descriptionList;
        private StringCollection unitList;
        private StringCollection indexList;
        private StringCollection sizeList;
        private StringCollection defaultValueList;
        private StringCollection noteList;
        private StringCollection funcList;
        private StringCollection endList;
        private StringCollection symbolList;
        private StringCollection StyleList;
        private StringCollection normalRangeList;

        #endregion

        public string portName;
        public CommunicationStatistics Statistics => statistics;
        public SerialPortConfig serialPortConfig { get; set; }
        public readonly ModbusDataBuffer dataBuffer;

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


        private ushort[] ConvertToUShortArray(double[] input)
        {
            return input.Select(x => (ushort)x).ToArray();
        }


   public void UpdateStatistics(bool errorCount)
{
    statistics.TotalReads++;
    if (errorCount) // 성공 조건 만들어야 함.
    {
        statistics.SuccessfulReads++;
        statistics.LastSuccessfulRead = DateTime.Now;
    }
    else
    {
        statistics.ErrorCount++;
        statistics.LastError = DateTime.Now;
    }

}

        public async Task ExecuteGenerateParameters(ObservableCollection<ParameterModel> parameters)
        {
            try
            {
                int start = Properties.Settings.Default.StartAddress;
                int end = Properties.Settings.Default.EndAddress;
                int numberOfPoints = end - start + 1;

                var parame = await ReadModbusData(start, numberOfPoints);
                foreach (var param in parame)
                {
                    var parameter = parameters.FirstOrDefault(p => p.Address == param.Address);
                    if (parameter != null)
                    {
                        parameter.DefaultActual = param.DefaultActual;
                    }
                }
                double[] Temp = { 0, 0 };

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var parameter in parameters)
                    {
                        if (parameter.Endian == "H")
                        {
                            if (Temp != null && Temp.Length > 0)
                                Temp[0] = parameter.DefaultActual;
                            parameter.DefaultActual = 0;
                            continue;
                        }
                        else if (parameter.Endian == "L")
                        {
                            if (Temp != null && Temp.Length > 1)
                                Temp[1] = parameter.DefaultActual;

                            var ushortTemp = ConvertToUShortArray(Temp);

                            ReadRegisterAsType(ushortTemp, DataType.Int32).ContinueWith(task =>
                            {
                                if (task.IsFaulted)
                                {
                                    Logger.Error(task.Exception, "파라미터 생성 중 오류 발생");
                                    MessageBox.Show($"파라미터 생성 중 오류가 발생했습니다: {task.Exception.Message}",
                                    "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                                var result = task.Result;

                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    parameter.DefaultActual = result;
                                });
                            });
                        }

                        if (parameter.Func != "N")
                        {
                            string[] funcSplit = parameter.Func.Split('_');

                            switch (funcSplit[0])
                            {
                                case "+":
                                    break;
                                case "-":
                                    break;
                                case "*":
                                    parameter.DefaultActual = parameter.DefaultActual * double.Parse(funcSplit[1]);
                                    break;
                                case "/":
                                    parameter.DefaultActual = parameter.DefaultActual / double.Parse(funcSplit[1]);
                                    break;
                                default:
                                    break;
                            }
                        }

                        parameter.NotifyPropertyChanged(nameof(parameter.DefaultActual));
                    }
                });

                Logger.Info("파라미터 값 갱신 완료");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "파라미터 갱신 중 오류 발생");
                MessageBox.Show($"파라미터 갱신 중 오류가 발생했습니다: {ex.Message}",
                    "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        public void LoadDefaultConfig()
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

        public async Task DisconnectIfConnected()
        {
            if (port != null && port.IsOpen)
            {
                port.Close();
                await Task.Delay(100);
            }
        }

        public void LoadAvailablePorts(ComboBox portComBox)
        {
            portComBox.ItemsSource = SerialPort.GetPortNames();
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


            try
            {
                await Task.Run(() => port.Open());

            }
            catch (Exception ex)
            {

                ShowMessage($"포트를 열 수가 없습니다 : {ex.Message}", "오류", true);
                throw;
            }
            var factory = new ModbusFactory();
            master = factory.CreateRtuMaster(port);
            master.Transport.ReadTimeout = 2000;
            master.Transport.WriteTimeout = 2000;

            descriptionList = Properties.Settings.Default.Description;
            unitList = Properties.Settings.Default.Unit;
            indexList = Properties.Settings.Default.Index;
            sizeList = Properties.Settings.Default.Size;
            defaultValueList = Properties.Settings.Default.DefaultValue;
            noteList = Properties.Settings.Default.Note;
            funcList = Properties.Settings.Default.Func;
            endList = Properties.Settings.Default.Endian;
            symbolList = Properties.Settings.Default.Symbols;
            StyleList = Properties.Settings.Default.Style;
            normalRangeList = Properties.Settings.Default.NormalRange;

        }


        public async Task<List<ParameterModel>> ReadModbusData(int startAddress, int numberOfPoints)
        {
            try
            {
                var registers = new ushort[numberOfPoints];

                try
                {
                    UpdateStatistics(true);
                    registers = await Task.Run(() =>
                               master.ReadHoldingRegisters(
                                   serialPortConfig.slaveId,
                                   (ushort)startAddress,
                                   (ushort)numberOfPoints));

                }
                catch (Exception ex)
                {
                    UpdateStatistics(false);
                    MessageBox.Show($"{ex.Message} 데이터를 읽을수 없습니다.");
                    throw;
                }
                statistics.RecordSuccessfulRead();
                lastDataReceived = DateTime.Now;

                var parameters = ConvertToParameters(registers, startAddress);

                dataBuffer.StoreValues(parameters);


                return parameters;
            }
            catch (Exception ex)
            {
                statistics.RecordError();
                ShowMessage($"데이터 읽기 실패: {ex.Message}", "오류", true);
                UpdateStatistics(false);
                var lastValues = dataBuffer.GetLastValues(numberOfPoints);
                return lastValues;
            }
        }


        public async Task WriteRegister(int addr, int value)
        {
            if (!IsConnected())
                throw new InvalidOperationException("연결되지 않았습니다.");
            try
            {
                await Task.Run(() =>
                    master.WriteSingleRegister(serialPortConfig.slaveId,
                        (ushort)addr,
                        (ushort)value));

                statistics.RecordSuccessfulRead();
            }
            catch (Exception ex)
            {
                statistics.RecordError();
                throw new Exception($"쓰기 실패: {ex.Message}");
            }
        }




        public async Task<double> ReadRegisterAsType(ushort[] temp, DataType dataType)
        {
            try
            {
                switch (dataType)
                {
                    case DataType.Float:
                        return ModbusDataConverter.ToFloat(temp);
                    case DataType.Int32:
                        return ModbusDataConverter.ToInt32Big(temp);
                    default:
                        throw new NotSupportedException($"Data type {dataType} is not supported.");
                }
            }
            catch (Exception ex)
            {
                statistics.RecordError();
                throw new ModbusException($"레지스터 읽기 실패: {ex.Message}", ex);
            }
        }


        public bool IsConnected()
        {
            return master != null && port != null && port.IsOpen;
        }


        private List<ParameterModel> ConvertToParameters(ushort[] registers, int startAddress)
        {
            List<ParameterModel> result = new List<ParameterModel>();

            for (int i = 0; i < registers.Length; i++)
            {
                ushort value = registers[i];
                int address = startAddress + i;

                DataType dataType = DataType.UInt16;
                dataTypeMap.TryGetValue((ushort)address, out dataType);

                ParameterModel parameter = new ParameterModel
                {
                    Address = address,
                    Description = descriptionList.Cast<string>().ElementAt(indexList.IndexOf(address.ToString())),
                    DefaultActual = value,
                    DefaultValue = defaultValueList.Cast<string>().ElementAt(indexList.IndexOf(address.ToString())),
                    ModbusUnit = unitList.Cast<string>().ElementAt(indexList.IndexOf(address.ToString())),
                    Index = address,
                    Note = noteList.Cast<string>().ElementAt(indexList.IndexOf(address.ToString())),
                    Func = funcList.Cast<string>().ElementAt(indexList.IndexOf(address.ToString())),
                    Endian = endList.Cast<string>().ElementAt(indexList.IndexOf(address.ToString())),
                    Symbols = symbolList.Cast<string>().ElementAt(indexList.IndexOf(address.ToString())),
                    NormalRange = normalRangeList.Cast<string>().ElementAt(indexList.IndexOf(address.ToString()))
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


    }
}
namespace Mvvm.Model
{
    public class ModbusDataBuffer
    {
        private readonly int bufferSize = 100;
        private readonly Dictionary<int, Queue<DataPointWithMetadata>> dataBuffer = new Dictionary<int, Queue<DataPointWithMetadata>>();
        private readonly object lockObject = new object();

        public void StoreValues(List<ParameterModel> parameters)
        {
            lock (lockObject)
            {
                foreach (var parameter in parameters)
                {
                    var parameterCopy = new ParameterModel
                    {
                        Address = parameter.Address,
                        Description = parameter.Description,
                        DefaultActual = parameter.DefaultActual,
                        DefaultValue = parameter.DefaultValue,
                        ModbusUnit = parameter.ModbusUnit,
                        Index = parameter.Index,
                        Note = parameter.Note,
                        Func = parameter.Func,
                        Endian = parameter.Endian,
                        Symbols = parameter.Symbols,
                        NormalRange = parameter.NormalRange
                    };

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

                        Address = dp.Metadata.Address,
                        Description = dp.Metadata.Description,
                        DefaultActual = dp.Value,
                        DefaultValue = dp.Metadata.DefaultValue,
                        ModbusUnit = dp.Metadata.ModbusUnit,
                        Index = dp.Metadata.Index,
                        Note = dp.Metadata.Note,
                        Func = dp.Metadata.Func,
                        Endian = dp.Metadata.Endian,
                        Symbols = dp.Metadata.Symbols,
                        NormalRange = dp.Metadata.NormalRange
                    })
                    .ToList();
            }
        }

        #region 차트에 쓸 데이터 버퍼


        public DataPointWithMetadata[] GetHistoricalData(int address, TimeSpan timeSpan)
        {
            lock (lockObject)
            {
                if (!dataBuffer.ContainsKey(address))
                {
                    return Array.Empty<DataPointWithMetadata>();
                }

                var cutoff = DateTime.Now - timeSpan;
                return dataBuffer[address]
                    .Where(dp => dp.Timestamp >= cutoff)
                    .Select(dp => new DataPointWithMetadata
                    {
                        Timestamp = dp.Timestamp,
                        Value = dp.Value
                    })
                    .ToArray();
            }
        }
        #endregion

    }

    public class CommunicationStatistics
    {
        public int TotalReads { get; set; }
        public int SuccessfulReads { get;  set; }
        public int ErrorCount { get;  set; }
        public int ReconnectAttempts { get; set; }
        public int SuccessfulReconnects { get;  set; }
        public DateTime LastSuccessfulRead { get; set; }
        public DateTime LastError { get; set; }

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

    public class DataPointWithMetadata
    {
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
        public ParameterModel Metadata { get; set; }
    }
    public enum DataType
    {
        UInt16,
        Int16,
        UInt32,
        Int32,
        Float,
        Long

    }
}
