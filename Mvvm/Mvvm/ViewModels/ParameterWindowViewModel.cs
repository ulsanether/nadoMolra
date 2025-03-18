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

namespace Mvvm.ViewModels
{
    public class ParameterWindowViewModel : BindableBase
    {
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
        private DelegateCommand _generateParametersCommand;
        private DelegateCommand<ParameterModel> _writeCommand;
        #endregion


        #region Properties



        public ICommand ModbosWritCommand => new DelegateCommand(OnModbusWrite);

   

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
                    ResetChart();
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


        public DelegateCommand GenerateParametersCommand =>
            _generateParametersCommand ?? (_generateParametersCommand = new DelegateCommand(ExecuteGenerateParameters));


        public DelegateCommand<ParameterModel> WriteCommand =>
            _writeCommand ?? (_writeCommand = new DelegateCommand<ParameterModel>(ExecuteWrite));

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


        private async void OnModbusWrite(){
            MessageBox.Show("Modbus Write 쓸곳");
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

                Parameters.Clear();

                var parameters = await _modbusConnect.ReadModbusData(start, numberOfPoints);


              

    

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var parameter in parameters)
                    {   //parameters.Count

                        //갱신 해야할 데터 순서 
                        //1. index
                        //2. description
                        //3. unit
                        //4. default value

                        
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





        //모드버스 쓰기 커맨드 
        private async void ExecuteWrite(ParameterModel parameter)
        {
            if (parameter == null)
            {
                Logger.Warn("WriteCommand: 파라미터가 null입니다.");
                return;
            }

            try
            {
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
                    $"다음 값을 설정하시겠습니까?\n\n주소: {parameter.Address}\n설명: {parameter.Description}\n현재값: {parameter.DefaultActual}\n설정값: {newValue}",
                    "설정 확인",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    await _modbusConnect.WriteRegister(parameter, newValue);
                    parameter.DefaultActual = newValue;
                    parameter.NewValue = string.Empty;  // 입력값 초기화

                    Logger.Info($"파라미터 쓰기 완료 - 주소: {parameter.Address}, 값: {newValue}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"파라미터 쓰기 중 오류 발생 - 주소: {parameter.Address}");
                MessageBox.Show($"값 설정 중 오류가 발생했습니다: {ex.Message}",
                    "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
        public Action RefreshTemplateAction { get; set; }

        public void RefreshTemplate()
        {
            RefreshTemplateAction?.Invoke();
            RaisePropertyChanged(nameof(IsCardView));
            LoadModbusData();
        }

        public ParameterWindowViewModel(ModbusConnect modbusConnect)
        {
            _modbusConnect = modbusConnect;
            _communicationLog = new ObservableCollection<CommunicationLogItem>();

            Parameters = new ObservableCollection<ParameterModel>();
            ResetChartCommand = new DelegateCommand(ResetChart);
            ExportDataCommand = new DelegateCommand(ExportData);
            ResetStatisticsCommand = new DelegateCommand(ResetStatistics);

            // 데이터 업데이트용 타이머 간격을 2초로 변경
            _dataUpdateTimer = new System.Timers.Timer(2000); // 2초마다 업데이트
            _dataUpdateTimer.Elapsed += OnDataUpdateTimerElapsed;
            _dataUpdateTimer.AutoReset = true;
            _dataUpdateTimer.Start();

            _updateTimer = new Timer(2000); // 2초로 변경
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
                    var initialData = await _modbusConnect.ReadModbusData(0, 1);
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
                    int startAddress, endAddress, count;
                    ModbusCount(out startAddress, out endAddress, out count);

                    var updatedData = await _modbusConnect.ReadModbusData(startAddress, count);
                    AddLog("데이터 갱신", $"주소 범위: {startAddress}-{endAddress}, 개수: {count}");

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        foreach (var parameter in updatedData)
                        {
                            UpdateExistingParameter(parameter);
                        }
                        RaisePropertyChanged(nameof(Parameters));
                        RaisePropertyChanged(nameof(AvailableParameters));
                    });
                }
            }
            catch (Exception ex)
            {
                AddLog("오류", $"ModBus 데이터 로드 실패: {ex.Message}");
                Logger.Error(ex, "Modbus 데이터 로드 실패");
            }
        }

        private void ModbusCount(out int startAddress, out int endAddress, out int count)
        {
            startAddress = Parameters.Min(p => p.Address);
            endAddress = Parameters.Max(p => p.Address);
            count = endAddress - startAddress + 1;
        }

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

        private void AddLog(string eventName, string details)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var logItem = new CommunicationLogItem(eventName, details);
                _communicationLog.Insert(0, logItem);

                // 최대 로그 수 제한
                while (_communicationLog.Count > 1000)
                {
                    _communicationLog.RemoveAt(_communicationLog.Count - 1);
                }

                // NLog를 통한 로깅
                Logger.Info($"{eventName}: {details}");

                // UI 업데이트를 위한 속성 변경 알림
                RaisePropertyChanged(nameof(CommunicationLog));
            });
        }

        private void InitializeParameter(ParameterModel parameter)
        {
            var newParameter = new ParameterModel
            {
                Address = parameter.Address,
                DefaultActual = parameter.DefaultActual,
                Description = "Description",
                Label = $"Register {parameter.Address}",
                ModbusUnit = "Raw"
            };

            // Settings에서 파라미터 정보 가져오기
            var indexList = Properties.Settings.Default.Index;
            var descriptionList = Properties.Settings.Default.Description;
            var unitList = Properties.Settings.Default.Unit;
            var defaultValueList = Properties.Settings.Default.DefaultValue;

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

        private void OnDataUpdateTimerElapsed(object sender, ElapsedEventArgs e)
        {
            LoadModbusData();
        }

        private async void OnUpdateTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (SelectedParameter == null) return;

                var data = await _modbusConnect.ReadModbusData(SelectedParameter.Address, 1);
                if (data.Any())
                {
                    var value = data[0].DefaultActual;
                    var time = (DateTime.Now - DateTime.Today).TotalSeconds;

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
                            // SelectedParameter의 DefaultActual 값을 업데이트
                            SelectedParameter.DefaultActual = value;
                            CurrentValue = value;
                            UpdatePlot();
                        });
                    }
                }
            }
            finally
            {
                if (IsRealTimeUpdate)
                {
                    _updateTimer.Start();
                }
            }
        }

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

        public void UpdatePlot()
        {
            if (_wpfPlot == null) return;

            var times = _timeData.ToArray();
            var values = _valueData.ToArray();

            if (times.Length > 0)
            {
                var plt = _wpfPlot.Plot;
                plt.Clear();

                if (SelectedParameter != null)
                {
                    plt.Title($"{SelectedParameter.Label} 실시간 데이터");
                    plt.YLabel($"값 ({SelectedParameter.ModbusUnit})");
                }

                _dataPlot = plt.Add.ScatterLine(times, values);

                if (AutoScale)
                {
                    // 자동 스케일링 적용
                    plt.Axes.AutoScale(); // AxisAuto() 대신 AutoScale() 사용
                }
                else
                {
                    double timeSpan = 10;
                    double latestTime = times.Last();
                    plt.Axes.SetLimits(
                        left: Math.Max(latestTime - timeSpan, times[0]),
                        right: latestTime,
                        bottom: values.Min() - 1,
                        top: values.Max() + 1
                    );
                }

                // 마지막 값 텍스트 추가 (size 파라미터 제거)
                if (values.Length > 0)
                {
                    plt.Add.Text(
                        text: values.Last().ToString("F2"),
                        x: times.Last(),
                        y: values.Last()
                    );
                }
            }

            _wpfPlot.Refresh();
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
