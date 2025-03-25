using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Timers;
using Prism.Commands;
using Prism.Mvvm;
using ScottPlot;
using ScottPlot.WPF;
using System.Collections.Generic;
using System.Diagnostics;
using DryIoc;
using Mvvm.Model;
using System.Threading;
using NLog;

using Timer = System.Timers.Timer;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Specialized;
using ImTools;
using ScottPlot.Colormaps;
using FluentIcons.Common;
using DevExpress.Xpf.Bars;
using System.Net.Sockets;

namespace Mvvm.ViewModels
{
    public class ParameterWindowViewModel : BindableBase
    {


        private DelegateCommand _addButtonClickCommand;
        public ICommand AddButtonClickCommand =>
        _addButtonClickCommand ??= new DelegateCommand(OnAddButtonClick);


        #region  임시로 놔두는곳

        private void OnAddButtonClick()
        {
            if (SelectedParameter != null)
            {
                var parameter = SelectedParameter;
                //  MessageBox.Show(SelectedParameter.Index.ToString());
                AddPlotAddress(parameter);

            }
            else
            {
                MessageBox.Show("파라미터를 선택하세요.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private ObservableCollection<int> _monitoredAddresses = new ObservableCollection<int>();
        public ObservableCollection<int> MonitoredAddresses
        {
            get => _monitoredAddresses;
            set => SetProperty(ref _monitoredAddresses, value);
        }

        public void AddPlotAddress(ParameterModel addr)
        {
            try
            {
                if (MonitoredAddresses.Contains(addr.Index))
                {
                    MessageBox.Show($"주소 {addr.Index}는 이미 모니터링 중입니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var parameter = Parameters.FirstOrDefault(p => p == addr);



                if (parameter == null)
                {
                    MessageBox.Show($"주소 {addr.Index}에 해당하는 파라미터를 찾을 수 없습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (MonitoredAddresses.Count >= 10)
                {
                    int oldestAddress = MonitoredAddresses.First();
                    MonitoredAddresses.Remove(oldestAddress);

                    var oldestParameter = Parameters.FirstOrDefault(p => p.Address == oldestAddress);
                    if (oldestParameter != null)
                    {
                        oldestParameter.IsMonitoring = false;
                        Logger.Info($"모니터링 제거: 주소 {oldestAddress}, 설명: {oldestParameter.Description}");
                        AddLog("모니터링 제거", $"주소 {oldestAddress}, 파라미터: {oldestParameter.Description}");
                    }
                }

                MonitoredAddresses.Add(addr.Index);
                parameter.IsMonitoring = true;

                Logger.Info($"모니터링 추가: 주소 {addr.Index}, 설명: {parameter.Description}");
                AddLog("모니터링 추가", $"주소 {addr.Index}, 파라미터: {parameter.Description}");

                if (!IsRealTimeUpdate)
                {
                    IsRealTimeUpdate = true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"플롯 주소 추가 중 오류 발생: {addr.Index}");
                MessageBox.Show($"플롯 주소 추가 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void DeletePlotAddress(int addr)
        {
            try
            {
                if (!MonitoredAddresses.Contains(addr))
                {
                    MessageBox.Show($"주소 {addr}는 모니터링 중이 아닙니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                MonitoredAddresses.Remove(addr);

                var parameter = Parameters.FirstOrDefault(p => p.Address == addr);
                if (parameter != null)
                {
                    parameter.IsMonitoring = false;
                    Logger.Info($"모니터링 제거: 주소 {addr}, 설명: {parameter.Description}");
                    AddLog("모니터링 제거", $"주소 {addr}, 파라미터: {parameter.Description}");
                }

                if (MonitoredAddresses.Count == 0 && IsRealTimeUpdate)
                {
                    IsRealTimeUpdate = false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"플롯 주소 삭제 중 오류 발생: {addr}");
                MessageBox.Show($"플롯 주소 삭제 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void AllClearPlotAddress()
        {
            try
            {
                if (MonitoredAddresses.Count == 0)
                {
                    MessageBox.Show("모니터링 중인 주소가 없습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 모니터링 중인 모든 파라미터 처리
                foreach (var addr in MonitoredAddresses.ToList())
                {
                    var parameter = Parameters.FirstOrDefault(p => p.Address == addr);
                    if (parameter != null)
                    {
                        parameter.IsMonitoring = false;
                    }
                }

                MonitoredAddresses.Clear();

                if (IsRealTimeUpdate)
                {
                    IsRealTimeUpdate = false;
                }

                ResetChart();

                Logger.Info("모든 모니터링 주소 제거");
                AddLog("모니터링 초기화", "모든 모니터링 주소가 제거되었습니다.");

                MessageBox.Show("모든 모니터링 주소가 제거되었습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "모든 플롯 주소 제거 중 오류 발생");
                MessageBox.Show($"모든 플롯 주소 제거 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public void UpdatePlot()
        {
            if (_wpfPlot == null) return;

            var plt = _wpfPlot.Plot;
          //  plt.Clear();

            // 모니터링 중인 주소가 없으면, 선택된 파라미터만 표시
            if (MonitoredAddresses.Count == 0 && SelectedParameter != null)
            {
                int parameterAddress = SelectedParameter.Address;
                TimeSpan historySpan = TimeSpan.FromHours(1);
                var historicalData = _modbusConnect.dataBuffer.GetHistoricalData(parameterAddress, historySpan);

                if (historicalData.Length > 0)
                {
                    double[] times = historicalData.Select(dp =>
                        (dp.Timestamp - DateTime.Today).TotalSeconds).ToArray();
                    double[] values = historicalData.Select(dp => dp.Value).ToArray();

                    plt.Title($"{SelectedParameter.Label} 데이터 이력");
                    plt.YLabel($"값 ({SelectedParameter.ModbusUnit})");
                    plt.XLabel("시간 (초)");

                    _dataPlot = plt.Add.ScatterLine(times, values);

                    if (values.Length > 0)
                    {
                        plt.Add.Text(
                            text: values.Last().ToString("F2"),
                            x: times.Last(),
                            y: values.Last()
                        );
                    }

                    if (AutoScale)
                    {
                        plt.Axes.AutoScale();
                    }
                    else
                    {
                        double timeSpan = 300; // 5분
                        double latestTime = times.LastOrDefault();
                        plt.Axes.SetLimits(
                            left: Math.Max(latestTime - timeSpan, times.FirstOrDefault()),
                            right: latestTime,
                            bottom: values.Min() - 1,
                            top: values.Max() + 1
                        );
                    }
                }
                else
                {
                    double[] initialData = { 0 };
                    double[] initialTimes = { 0 };
                    _dataPlot = plt.Add.ScatterLine(initialTimes, initialData);
                    plt.Axes.SetLimits(left: -10, right: 0, bottom: -10, top: 10);
                }
            }
            // 모니터링 중인 주소들의 데이터 표시
            else if (MonitoredAddresses.Count > 0)
            {
                TimeSpan historySpan = TimeSpan.FromHours(1);
                double minValue = double.MaxValue;
                double maxValue = double.MinValue;
                double minTime = double.MaxValue;
                double maxTime = double.MinValue;

                plt.Title("모니터링 중인 파라미터");
                plt.YLabel("값");
                plt.XLabel("시간 (초)");

                // 각 모니터링 주소에 대한 데이터 추가
                foreach (int index in MonitoredAddresses)
                {
                    var parameter = Parameters.FirstOrDefault(p => p.Index == index);
                    if (parameter == null) continue;

                    var historicalData = _modbusConnect.dataBuffer.GetHistoricalData(parameter.Address, historySpan);

                    if (historicalData.Length > 0)
                    {
                        double[] times = historicalData.Select(dp =>
                            (dp.Timestamp - DateTime.Today).TotalSeconds).ToArray();
                        double[] values = historicalData.Select(dp => dp.Value).ToArray();

                        // 각 파라미터마다 다른 색상으로 표시
                        var line = plt.Add.ScatterLine(times, values);

                        // 가장 최근 값에 레이블 추가
                        if (values.Length > 0)
                        {
                            plt.Add.Text(
                                text: $"{parameter.Description}: {values.Last():F2} {parameter.ModbusUnit}",
                                x: times.Last(),
                                y: values.Last()
                            );
                        }

                        // 축 제한을 위한 최소/최대 값 업데이트
                        if (values.Length > 0)
                        {
                            minValue = Math.Min(minValue, values.Min());
                            maxValue = Math.Max(maxValue, values.Max());
                            minTime = Math.Min(minTime, times.First());
                            maxTime = Math.Max(maxTime, times.Last());
                        }
                    }
                }

                // 축 설정
                if (minValue != double.MaxValue && maxValue != double.MinValue)
                {
                    if (AutoScale)
                    {
                        plt.Axes.AutoScale();
                    }
                    else
                    {
                        double timeSpan = 300; // 5분
                        plt.Axes.SetLimits(
                            left: Math.Max(maxTime - timeSpan, minTime),
                            right: maxTime,
                            bottom: minValue - 1,
                            top: maxValue + 1
                        );
                    }
                }
                else
                {
                    // 데이터가 없는 경우 기본 축 설정
                    double[] initialData = { 0 };
                    double[] initialTimes = { 0 };
                    _dataPlot = plt.Add.ScatterLine(initialTimes, initialData);
                    plt.Axes.SetLimits(left: -10, right: 0, bottom: -10, top: 10);
                }
            }
            else
            {
                // 선택된 파라미터도, 모니터링 중인 주소도 없는 경우 빈 차트 표시
                double[] initialData = { 0 };
                double[] initialTimes = { 0 };
                _dataPlot = plt.Add.ScatterLine(initialTimes, initialData);
                plt.Axes.SetLimits(left: -10, right: 0, bottom: -10, top: 10);
            }

            _wpfPlot.Refresh();
        }


        #endregion



        #region Fields
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ModbusConnect _modbusConnect;
        private readonly Timer _updateTimer;
        private readonly Dictionary<int, (string Description, string Unit, double DefaultValue, string Note)> _modbusData;
        private int _columns = 1;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly Timer _dataUpdateTimer;
        private bool _isRealTimeUpdate;
        private ParameterModel _selectedParameter;
        private double _currentValue;
        private readonly int _maxDataPoints = 1000;
        private readonly Queue<double> _timeData = new();
        private readonly Queue<double> _valueData = new();
        private WpfPlot _wpfPlot;
        private readonly object _lockObject = new();
        private IPlottable _dataPlot;
        private bool _autoScale;
        private string _selectedChartType;
        private string _statistics;

        private bool _isCardView;
        private ObservableCollection<string> _chartTypes;
        private string _startAddress;
        private string _endAddress;
        private bool _isListView = true;
        private ObservableCollection<CommunicationLogItem> _communicationLog;

        private DelegateCommand<ParameterModel> _writeCommand;


        #endregion


        #region Properties
        public Action RefreshTemplateAction { get; set; }
        public object LockObject => _lockObject;
        public Queue<double> TimeData => _timeData;
        public Queue<double> ValueData => _valueData;
        public int MaxDataPoints => _maxDataPoints;

        public string StartAddress
        {
            get => _startAddress;
            set => SetProperty(ref _startAddress, value);
        }
        public string EndAddress
        {
            get => _endAddress;
            set => SetProperty(ref _endAddress, value);
        }
        public bool IsListView
        {
            get => _isListView;
            set => SetProperty(ref _isListView, value);
        }

        public bool IsRealTimeUpdate
        {
            get => _isRealTimeUpdate;
            set
            {
                if (SetProperty(ref _isRealTimeUpdate, value))
                {
                    if (value) StartDataCollection();
                    else StopDataCollection();
                }
            }
        }

        public bool IsCardView
        {
            get => _isCardView;
            set => SetProperty(ref _isCardView, value);
        }

        public double CurrentValue
        {
            get => _currentValue;
            set => SetProperty(ref _currentValue, value);
        }

        public ParameterModel SelectedParameter
        {
            get => _selectedParameter;
            set
            {
                if (SetProperty(ref _selectedParameter, value))
                {
                    // ResetChart();
                    if (value != null)
                    {
                        CurrentValue = value.DefaultActual;
                        if (IsRealTimeUpdate)
                        {
                            StartDataCollection();
                        }
                    }
                }
            }
        }

        public bool AutoScale
        {
            get => _autoScale;
            set => SetProperty(ref _autoScale, value);
        }

        public string SelectedChartType
        {
            get => _selectedChartType;
            set => SetProperty(ref _selectedChartType, value);
        }

        public string Statistics
        {
            get => _statistics;
            set => SetProperty(ref _statistics, value);
        }

        private DelegateCommand _generateParametersCommand;
        public DelegateCommand GenerateParametersCommand =>
            _generateParametersCommand ??= new DelegateCommand(
                ExecuteGenerateParameters,
                () => true
            );



        private DelegateCommand<ParameterModel> _modbusWriteCommand;
        public ICommand ModbosWritCommand
        {
            get
            {
                if (_modbusWriteCommand == null)
                {
                    _modbusWriteCommand = new DelegateCommand<ParameterModel>(OnModbusWrite);
                }
                return _modbusWriteCommand;
            }
        }

        private async void OnModbusWrite(ParameterModel parameter)
        {
            if (parameter == null)
            {
                MessageBox.Show("선택된 파라미터가 없습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Index 값 가져오기
            Logger.Info($"Modbus 쓰기 시작 - 인덱스: {parameter.Index:D3}, 설명: {parameter.Description}");
            MessageBox.Show($"인덱스: {parameter.Index:D3}, '{parameter.Description}' 파라미터 설정을 시작합니다.",
            "Modbus Write", MessageBoxButton.OK, MessageBoxImage.Information);

            // 기존 ExecuteWrite 메서드 호출
            ExecuteWrite(parameter);
        }



        public DelegateCommand<ParameterModel> WriteCommand
        {
            get
            {
                if (_writeCommand == null)
                {
                    _writeCommand = new DelegateCommand<ParameterModel>(
                    param => { if (param != null) ExecuteWrite(param); },
                  param => param is ParameterModel && CanExecuteWrite(param)
                );
                }
                return _writeCommand;
            }
        }

        private bool CanExecuteWrite(ParameterModel parameter)
        {

            return parameter != null;
        }




        private ObservableCollection<ParameterModel> _availableParameters;
        public ObservableCollection<ParameterModel> AvailableParameters
        {
            get => _availableParameters;
            set => SetProperty(ref _availableParameters, value);
        }

        public ObservableCollection<CommunicationLogItem> CommunicationLog
        {
            get => _communicationLog;
            set => SetProperty(ref _communicationLog, value);
        }




        private async void ExecuteGenerateParameters()
        {

            try
            {
                if (!int.TryParse(StartAddress, out int start) || !int.TryParse(EndAddress, out int end))
                {
                    MessageBox.Show("시작 주소와 끝 주소는 숫자여야 합니다.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (start > end)
                {
                    MessageBox.Show("시작 주소는 끝 주소보다 작아야 합니다.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int numberOfPoints = end - start + 1;

                double[] Temp = { 0, 0 };

                Parameters.Clear();
                var parameters = await GetReadModbusData(start, numberOfPoints);

                var stackParameterModels = new Stack<ParameterModel>(parameters);
                var description = Properties.Settings.Default.Description;
                var unit = Properties.Settings.Default.Unit;
                var defaultValue = Properties.Settings.Default.DefaultValue;

                var normalRange = Properties.Settings.Default.NormalRange;

                var note = Properties.Settings.Default.Note;
                var func = Properties.Settings.Default.Func;
                var endian = Properties.Settings.Default.Endian;
                var symbols = Properties.Settings.Default.Symbols;



                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    while (stackParameterModels.Count > 0)
                    {
                        var parameter = stackParameterModels.Pop();

                        parameter.Index = start++;
                        parameter.Description = description[parameter.Index];
                        parameter.Unit = unit[parameter.Index];
                        parameter.DefaultValue = defaultValue[parameter.Index];
                        parameter.Endian = endian[parameter.Index];

                        //범위값 /10 해서 표시 할것.
                        parameter.NormalRange = normalRange[parameter.Index];

                        parameter.Symbols = symbols[parameter.Index];
                        parameter.Func = func[parameter.Index];


                        if (parameter.Endian == "H")
                        {
                            //  MessageBox.Show(parameter.DefaultActual.ToString());
                            if (Temp != null && Temp.Length > 0)
                                Temp[0] = parameter.DefaultActual;
                            parameter.DefaultActual = 0;
                            continue;


                        }
                        else if (parameter.Endian == "L")
                        {
                            if (Temp != null && Temp.Length > 1)
                                Temp[1] = parameter.DefaultActual;
                            //      MessageBox.Show(Temp[0].ToString() + "  ||   " + Temp[1].ToString());

                            var ushortTemp = ConvertToUShortArray(Temp);

                            _modbusConnect.ReadRegisterAsType(ushortTemp, DataType.Int32).ContinueWith(task =>
                            {
                                if (task.IsFaulted)
                                {
                                    Logger.Error(task.Exception, "파라미터 생성 중 오류 발생");
                                    MessageBox.Show($"파라미터 생성 중 오류가 발생했습니다: {task.Exception.Message}",
                                    "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                                var result = task.Result;

                                //   MessageBox.Show("Result : " + result);

                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    parameter.DefaultActual = result;
                                    //Parameters.Add(parameter);
                                });
                            });
                        }


                        if (parameter.Func != "N")
                        {
                            string[] funcSplit = parameter.Func.Split('_');

                            //    MessageBox.Show(funcSplit[0] + "  |  " + funcSplit[1]);


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


                        Parameters.Add(parameter);

                    }
                });

                Logger.Info($"파라미터 생성 완료: {start}부터 {end}까지 {parameters.Count}개 생성됨");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "파라미터 생성 중 오류 발생");
                MessageBox.Show($"파라미터 생성 중 오류가 발생했습니다: {ex.Message}",
                "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async Task<List<ParameterModel>> GetReadModbusData(int start, int numberOfPoints) =>
                    await _modbusConnect.ReadModbusData(start, numberOfPoints);


        private async void ExecuteWrite(ParameterModel parameter)
        {

            if (parameter == null)
            {
                Logger.Warn("WriteCommand: 파라미터가 null입니다.");
                return;
            }

            try
            {
                Logger.Info($"저장 버튼 클릭 - 인덱스: {parameter.Index:D3}, 주소: {parameter.Address}");
                AddLog("버튼 클릭", $"인덱스: {parameter.Index:D3}, 주소: {parameter.Address}");

                if (string.IsNullOrWhiteSpace(parameter.NewValue))
                {
                    MessageBox.Show("설정값을 입력해주세요.", "입력 오류",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(parameter.NewValue, out int newValue))
                {
                    MessageBox.Show("설정값은 숫자여야 합니다.", "입력 오류",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                $"다음 값을 설정하시겠습니까?\n\n인덱스: {parameter.Index:D3}\n주소: {parameter.Address}\n설명: {parameter.Description}\n현재값: {parameter.DefaultActual:F3}\n설정값: {newValue}",
                "설정 확인",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);



                if (result == MessageBoxResult.Yes)
                {

                    #region 표시 범위랑 값들인데, 나중에 다른 프로젝트 할때  추가 해야함. 일단 /10 만 있음.
                    string[] normalRangeSplit = parameter.NormalRange.Split('~');

                    int lowRange = int.Parse(normalRangeSplit[0]) / 10;
                    int highRange = int.Parse(normalRangeSplit[1]) / 10;
                    if (lowRange > newValue || highRange < newValue)
                    {

                        MessageBox.Show("설정값이 정상 범위를 벗어났습니다.", "입력 오류",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;

                    }

                    var allParameters = GetAllParameters();

                    foreach (var item in allParameters)
                    {
                        if (item.Endian == "H")
                        {
                            var numOne = item.Index;
                            parameter.NewValue = "";
                        }
                        else if (item.Endian == "L")
                        {
                            var numTwo = item.Index;


                            var refVal = ModbusDataConverter.FromInt32Big((int)item.DefaultActual);


                            await _modbusConnect.WriteRegister(parameter.Index - 1, refVal[0]);
                            await _modbusConnect.WriteRegister(parameter.Index, refVal[1]);
                            parameter.NewValue = "";

                            Logger.Info($"파라미터 쓰기 완료 - 인덱스: {parameter.Index:D3}, 주소: {parameter.Address}, 값: {newValue}");
                            return;
                        }
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


                                newValue /= int.Parse(funcSplit[1]);
                                await _modbusConnect.WriteRegister(parameter.Index, newValue);
                                parameter.DefaultActual = newValue * int.Parse(funcSplit[1]);
                                parameter.NewValue = "";
                                break;
                            case "/":
                                newValue *= int.Parse(funcSplit[1]);
                                await _modbusConnect.WriteRegister(parameter.Index, newValue);
                                parameter.DefaultActual = newValue / int.Parse(funcSplit[1]);
                                parameter.NewValue = "";
                                break;
                            default:
                                break;

                        }

                        #endregion

                    }
                    else
                    {
                        await _modbusConnect.WriteRegister(parameter.Index, newValue);
                        parameter.DefaultActual = newValue;
                        parameter.NewValue = "";
                    }
                    parameter.NewValue = "";
                    Logger.Info($"파라미터 쓰기 완료 - 인덱스: {parameter.Index:D3}, 주소: {parameter.Address}, 값: {newValue}");
                }


            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"파라미터 쓰기 중 오류 발생 - 인덱스: {parameter.Index:D3}, 주소: {parameter.Address}");
                MessageBox.Show($"값 설정 중 오류가 발생했습니다: {ex.Message}",
                "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        public List<ParameterModel> GetAllParameters()
        {
            return Parameters.ToList();
        }

        public ObservableCollection<string> ChartTypes
        {
            get => _chartTypes;
            set => SetProperty(ref _chartTypes, value);
        }

        private ObservableCollection<ParameterModel> _parameters;
        public ObservableCollection<ParameterModel> Parameters
        {
            get => _parameters;
            set => SetProperty(ref _parameters, value);
        }


        public DelegateCommand ResetChartCommand { get; }
        public DelegateCommand ExportDataCommand { get; }
        public DelegateCommand ResetStatisticsCommand { get; }
        #endregion

        #region Constructor
        public Queue<ParameterModel> queuePparmameterModels = new Queue<ParameterModel>();
        private List<ParameterModel> listParameterModels = new List<ParameterModel>();



        public ParameterWindowViewModel(ModbusConnect modbusConnect)
        {
            _modbusConnect = modbusConnect;
            _communicationLog = new ObservableCollection<CommunicationLogItem>();

            Parameters = new ObservableCollection<ParameterModel>();
            ResetChartCommand = new DelegateCommand(ResetChart);
            ExportDataCommand = new DelegateCommand(ExportData);
            ResetStatisticsCommand = new DelegateCommand(ResetStatistics);


            _updateTimer = new Timer(2000);
            _updateTimer.Elapsed += OnUpdateTimerElapsed;
            _updateTimer.AutoReset = false;

            _modbusConnect.ConnectionStatusChanged += OnConnectionStatusChanged;

            AddLog("초기화", "ParameterWindow 초기화 완료");
            InitializeChart();
            InitializeChartTypes();
            LoadModbusData();
        }
        #endregion



        #region Private Methods

        public void RefreshTemplate()
        {
            RefreshTemplateAction?.Invoke();
            RaisePropertyChanged(nameof(IsCardView));
            LoadModbusData();
        }


        #region 타이머 갱신인데 나중에 넣어야 함.
        private async void LoadModbusData()
        {
            try
            {
                if (!_modbusConnect.IsConnected())
                {
                    AddLog("연결 상태", "ModBus 연결되지 않음");
                    return;
                }

                AddLog("데이터 요청", "ModBus 데이터 읽기 시작");

                if (!Parameters.Any())
                {

                    var initialData = await GetReadModbusData(0, 1);
                    AddLog("초기 데이터", $"레지스터 수: {initialData.Count}");

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        foreach (var parameter in initialData)
                        {
                            InitializeParameter(parameter);
                        }
                    });
                }
                else
                {

                    //int startAddress, endAddress, count;
                    //ModbusCount(out startAddress, out endAddress, out count);

                    //var updatedData = await GetReadModbusData(startAddress, count);

                    ////MessageBox.Show(string.Format("주소 범위: {0} - {1}, 개수: {2}", startAddress, endAddress, count));

                    //AddLog("데이터 갱신", $"주소 범위: {startAddress}-{endAddress}, 개수: {count}");


                    //await Application.Current.Dispatcher.InvokeAsync(() =>
                    //{
                    //    foreach (var parameter in updatedData)
                    //    {
                    //        UpdateExistingParameter(parameter);

                    //    }
                    //    RaisePropertyChanged(nameof(Parameters));
                    //    RaisePropertyChanged(nameof(AvailableParameters));
                    //});
                }
            }
            catch (Exception ex)
            {
                AddLog("오류", $"ModBus 데이터 로드 실패: {ex.Message}");
                Logger.Error(ex, "Modbus 데이터 로드 실패");
            }
        }


        //사용 안할것 같은 코드
        private void UpdateExistingParameter(ParameterModel updatedParameter)
        {
            var existingParameter = Parameters.FirstOrDefault(p => p.Address == updatedParameter.Address);
            if (existingParameter != null)
            {
                if (Math.Abs(existingParameter.DefaultActual - updatedParameter.DefaultActual) > 0.001)
                {
                    var oldValue = existingParameter.DefaultActual;

                    existingParameter.IsValueChanged = true;

                    AddLog("값 변경", $"주소: {existingParameter.Address}, " +
                    $"이전 값: {oldValue:F2}, " +
                    $"새 값: {updatedParameter.DefaultActual:F2}");

                    Task.Delay(3000).ContinueWith(_ =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            existingParameter.IsValueChanged = false;
                        });
                    });
                }
            }
        }

        #endregion






        private void AddLog(string eventName, string details)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var logItem = new CommunicationLogItem(eventName, details);
                _communicationLog.Insert(0, logItem);

                while (_communicationLog.Count > 500)  //500개 부터 삭제 

                {
                    _communicationLog.RemoveAt(_communicationLog.Count - 1);
                }


                Logger.Info($"{eventName}: {details}");
                RaisePropertyChanged(nameof(CommunicationLog));

            });
        }

        private void InitializeParameter(ParameterModel parameter)
        {
            //접속 했을때 가장 초기값. 이것 역시 수정해야하고.
            //고급 모드가 아니라면 이것 역시 엑셀에 수정 가능한 리스트 만큼 가져오게 할것
            //settingview에서 값은 참조 할 것.


            // Settings에서 파라미터 정보 가져오기
            var indexList = Properties.Settings.Default.Index;
            var descriptionList = Properties.Settings.Default.Description;
            var unitList = Properties.Settings.Default.Unit;
            var defaultValueList = Properties.Settings.Default.DefaultValue;
            var normalRangeList = Properties.Settings.Default.NormalRange;
            var symbolsList = Properties.Settings.Default.Symbols;


            var newParameter = new ParameterModel
            {
                Address = parameter.Address,

                NormalRange = "-",
                DefaultActual = parameter.DefaultActual,
                Description = "description",
                Label = $"Register {parameter.Address}",

                ModbusUnit = "Raw"

            };


            if (indexList != null)
            {
                var settingsIndex = indexList.Cast<string>()
.Select((indexStr, i) => new { Index = int.Parse(indexStr), Position = i })
.FirstOrDefault(x => x.Index == parameter.Address);

                if (settingsIndex != null)
                {
                    newParameter.Description = descriptionList[settingsIndex.Position];
                    newParameter.Label = descriptionList[settingsIndex.Position];
                    newParameter.ModbusUnit = unitList[settingsIndex.Position];
                    newParameter.DefaultValue = defaultValueList[settingsIndex.Position];
                }
            }

            Parameters.Add(newParameter);
        }



        #region 차트 관련 메서드

        private async void OnUpdateTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (SelectedParameter == null) return;

                // 데이터 버퍼에서 최근 값 가져오기 시도
                var historicalData = _modbusConnect.dataBuffer.GetHistoricalData(SelectedParameter.Address, TimeSpan.FromSeconds(5));

                double value;
                double time = (DateTime.Now - DateTime.Today).TotalSeconds;
                bool valueUpdated = false;

                if (historicalData.Length > 0)
                {
                    var latestData = historicalData.OrderByDescending(dp => dp.Timestamp).First();
                    value = latestData.Value;
                    valueUpdated = true;
                }
                else
                {
                    if (_modbusConnect.IsConnected())
                    {
                        var data = await GetReadModbusData(SelectedParameter.Address, 1);
                        if (data.Any())
                        {
                            value = data[0].DefaultActual;
                            valueUpdated = true;
                            Logger.Debug($"버퍼에 데이터가 없어 직접 통신: {SelectedParameter.Address}");
                        }
                        else
                        {
                            // 연결은 되어 있지만 데이터를 읽지 못함
                            return;
                        }
                    }
                    else
                    {
                        // 연결이 안 되어 있으면 타이머 계속 유지
                        return;
                    }
                }

                if (valueUpdated)
                {
                    lock (_lockObject)
                    {
                        _timeData.Enqueue(time);
                        _valueData.Enqueue(value);

                        if (_timeData.Count > _maxDataPoints)
                        {
                            _timeData.Dequeue();
                            _valueData.Dequeue();
                        }

                        Application.Current.Dispatcher.Invoke(() =>
                        {


                            //차트 업데이트 갱신 부분

                            SelectedParameter.DefaultActual = value;
                            CurrentValue = value;
                            UpdatePlot();



                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "타이머 데이터 업데이트 중 오류 발생");
            }
            finally
            {
                if (IsRealTimeUpdate)
                {
                    _updateTimer.Start();
                }
            }
        }



        #endregion



        private void OnConnectionStatusChanged(bool isConnected)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (isConnected)
                {
                    _dataUpdateTimer?.Start();
                    LoadModbusData();
                }
                else
                {
                    _dataUpdateTimer?.Stop();
                    IsRealTimeUpdate = false;
                }
            });
        }

        private void StartDataCollection()
        {
            _updateTimer.Start();
        }

        private void StopDataCollection()
        {
            _updateTimer.Stop();
        }
        #endregion

        #region Public Methods
        public void InitializeWithPlot(WpfPlot plot)
        {
            _wpfPlot = plot;
            InitializeChart();
        }


        public void UpdateCommunicationStatus()
        {
            bool isConnected = _modbusConnect.IsConnected();
            OnConnectionStatusChanged(isConnected);
        }

        public void Cleanup()
        {
            StopDataCollection();
            _dataUpdateTimer?.Stop();
            _dataUpdateTimer?.Dispose();
            if (_modbusConnect != null)
            {
                _modbusConnect.ConnectionStatusChanged -= OnConnectionStatusChanged;
            }
        }
        #endregion

        #region Helper Methods


        private ushort[] ConvertToUShortArray(double[] input)
        {
            return input.Select(x => (ushort)x).ToArray();
        }


        private void InitializeChart()
        {
            if (_wpfPlot == null) return;

            var plt = _wpfPlot.Plot;
            plt.Clear();
            plt.Font.Set("맑은 고딕");

            plt.Title("Real Time ", 16);
            plt.XLabel("시간 (초)");
            plt.YLabel("값");

            double[] initialData = { 0 };
            double[] initialTimes = { 0 };
            _dataPlot = plt.Add.ScatterLine(initialTimes, initialData);

            plt.Axes.SetLimits(left: -10, right: 0, bottom: -10, top: 10);
            _wpfPlot.Refresh();
        }

        private void InitializeChartTypes()
        {
            ChartTypes = new ObservableCollection<string> { "Line", "Bar", "Scatter" };
            SelectedChartType = ChartTypes.First();
        }



        //차트 삭제 함수
        private void ResetChart()
        {
            if (_wpfPlot == null) return;

            lock (_lockObject)
            {
                _timeData.Clear();
                _valueData.Clear();

                var plt = _wpfPlot.Plot;
                plt.Clear();
                double[] initialData = { 0 };
                double[] initialTimes = { 0 };
                _dataPlot = plt.Add.ScatterLine(initialTimes, initialData);

                plt.Axes.SetLimits(left: -10, right: 0, bottom: -10, top: 10);

                _wpfPlot.Refresh();
            }
        }




        private void ExportData()
        {
            // 데이터 내보내기 로직
        }

        private void ResetStatistics()
        {
            // 통계 초기화 로직
        }
        #endregion
    }

    public class CommunicationLogItem
    {
        public DateTime Timestamp { get; }
        public string Event { get; }
        public string Details { get; }

        public CommunicationLogItem(string eventName, string details)
        {
            Timestamp = DateTime.Now;
            Event = eventName;
            Details = details;
        }
    }
}
