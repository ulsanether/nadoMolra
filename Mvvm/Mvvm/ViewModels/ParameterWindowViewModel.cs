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
using System.Threading;
using NLog;

using Timer = System.Timers.Timer;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Specialized;
using ImTools;
using FluentIcons.Common;
using System.Net.Sockets;
using DevExpress.DocumentServices.ServiceModel.DataContracts;
using Accord;

using Mvvm.Model;
using Mvvm.Converters;


namespace Mvvm.ViewModels
{
    public class ParameterWindowViewModel : BindableBase
    {

        private Timer _chartUpdateTimer;
        private DelegateCommand _addButtonClickCommand;
        public ICommand AddButtonClickCommand =>
        _addButtonClickCommand ??= new DelegateCommand(OnAddButtonClick);


        #region  임시로 놔두는곳

        private void OnAddButtonClick()
        {
            if (SelectedParameter != null)
            {
                var parameterAddr = SelectedParameter.Index;

                Debug.WriteLine(parameterAddr);
                AddValueToChart( parameterAddr);
            }
            else
            {

                MessageBox.Show("파라미터를 먼저 선택해주세요.", "알림");
            }
        }


        private Dictionary<int, bool> addressDictionary = new Dictionary<int, bool>();

        private void AddValueToChart(int addr)
        {
            var parameter = Parameters?.FirstOrDefault(p => p.Address == addr);
            int start = Properties.Settings.Default.StartAddress;
            int end = Properties.Settings.Default.EndAddress;
            if (parameter != null)
            {
                var addressRange = Enumerable.Range(start, end - start + 1);
                if (addressDictionary == null || !addressDictionary.Any())
                {
                    addressDictionary = addressRange.ToDictionary(address => address, address => false);
                }

                if (addressDictionary.ContainsKey(parameter.Address))
                {
                    addressDictionary[parameter.Address] = true;
                }

                UpdateChart();
            }
        }

        private readonly Dictionary<int, Queue<double>> _timeDataDict = new();
        private readonly Dictionary<int, Queue<double>> _valueDataDict = new();

        private async Task UpdateChart()
        {
            if (_wpfPlot == null) return;

            var trueAddresses = addressDictionary.Where(entry => entry.Value).Select(entry => entry.Key).ToList();
            var parametersToPlot = Parameters.Where(p => trueAddresses.Contains(p.Address)).ToList();

            if (_wpfPlot == null) return;

            var plt = _wpfPlot.Plot;
            plt.Clear();

            foreach (var parameter in parametersToPlot)
            {
                if (!_timeDataDict.ContainsKey(parameter.Address))
                {
                    _timeDataDict[parameter.Address] = new Queue<double>();
                    _valueDataDict[parameter.Address] = new Queue<double>();
                }

                var timeData = _timeDataDict[parameter.Address];
                var valueData = _valueDataDict[parameter.Address];

                timeData.Enqueue(DateTime.Now.ToOADate());
                valueData.Enqueue(parameter.DefaultActual);

                if (timeData.Count > MaxDataPoints)
                {
                    timeData.Dequeue();
                    valueData.Dequeue();
                }

                double[] times = timeData.ToArray();
                double[] values = valueData.ToArray();

                var scatterPlot = plt.Add.Scatter(times, values);
                scatterPlot.Label = $"{parameter.Description}";
            }

            if (AutoScale)
            {
                plt.Axes.AutoScale();
            }

            plt.ShowLegend();
            _wpfPlot.Refresh();
        }

        #endregion



        #region Fields
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ModbusConnect _modbusConnect;

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
           // MessageBox.Show(parameter.NormalRange);
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
                if (_writeCommand == null )
                {
                    _writeCommand = new DelegateCommand<ParameterModel>(
                        param =>
                        {
                            if (param != null)
                                ExecuteWrite(param);
                        },
                        param => param is ParameterModel parameter && CanExecuteWrite(parameter)
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
                int start = Properties.Settings.Default.StartAddress;
                int end = Properties.Settings.Default.EndAddress;
                int numberOfPoints = end - start + 1;

                var parame = await GetReadModbusData(start, numberOfPoints);
                foreach (var param in parame)
                {
                    var parameter = Parameters.FirstOrDefault(p => p.Address == param.Address);
                    if (parameter != null)
                    {
                        parameter.DefaultActual = param.DefaultActual;
                    }
                }
                double[] Temp = { 0, 0 };

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var parameter in Parameters)
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

                var result = MessageBox.Show( $"다음 값을 설정하시겠습니까?\n\n인덱스: {parameter.Index:D3}\n주소: {parameter.Address}\n설명: {parameter.Description}\n현재값: {parameter.DefaultActual:F3}\n설정값: {newValue}",
                "설정 확인",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);



                if (result == MessageBoxResult.Yes)
                {
                    #region 표시 범위랑 값들인데, 나중에 다른 프로젝트 할때  추가 해야함. 일단 /10 만 있음.

                    CheckParameterRange(parameter, newValue);

                    var allParameters = GetAllParameters();

                    if(parameter.Endian == "H")
                    {
                        MessageBox.Show("이 부분은 수정할 수 없습니다. ");
                        parameter.NewValue = "";
                        return;
                    }

                    if (parameter.Endian == "L")
                    {

                        var allParameter = GetAllParameters();

                        foreach (var item in allParameters)
                        {

                           if (item.Index == parameter.Index - 1)
    {
        MessageBox.Show("h");
        var numOne = item.Index;
        parameter.NewValue = "";
    }

                            if (item.Index == parameter.Index) {


                                var numTwo = item.Index;
                                MessageBox.Show("l");


                                var refVal = ModbusDataConverter.FromInt32Big((int)item.DefaultActual);


                                await _modbusConnect.WriteRegister(parameter.Index - 1, refVal[0]);
                                await _modbusConnect.WriteRegister(parameter.Index, refVal[1]);
                                parameter.NewValue = "";

                                Logger.Info($"파라미터 쓰기 완료 - 인덱스: {parameter.Index:D3}, 주소: {parameter.Address}, 값: {newValue}");
                                return;

                            }


                        }


                    }

                    if (parameter.Func != "N" )
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

        private static void CheckParameterRange(ParameterModel parameter, int newValue)
        {
            if (parameter.NormalRange.Contains('~'))
            {

                string[] normalRangeSplit = parameter.NormalRange.Split('~');


                int lowRange = int.Parse(normalRangeSplit[0]) / 10;
                int highRange = int.Parse(normalRangeSplit[1]) / 10;
                if (lowRange > newValue || highRange < newValue)
                {

                    MessageBox.Show("설정값이 정상 범위를 벗어났습니다.", "입력 오류",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;

                }
            }
        }

        #endregion

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



            _isParameterUpdateEnabled = false;

            _modbusConnect.ConnectionStatusChanged += OnConnectionStatusChanged;

            AddLog("초기화", "ParameterWindow 초기화 완료");
            InitializeChart();
            InitializeChartTypes();
            LoadModbusData();


            // 차트 갱신 타이머 설정
            _chartUpdateTimer = new Timer(2000);
            _chartUpdateTimer.Elapsed += async (sender, e) => await UpdateChart();
            _chartUpdateTimer.Start();


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
                    try
                    {
                        var initialData = await GetReadModbusData(0, 23);
                        AddLog("초기 데이터", $"레지스터 수: {initialData.Count}");

                        if (initialData.Any())
                        {
                            await Application.Current.Dispatcher.InvokeAsync(() => {
                               InitializeParameterInBackground();

                            });
                        }
                        else
                        {
                            AddLog("경고", "초기 데이터를 가져오지 못했습니다.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "백그라운드 Modbus 초기 데이터 읽기 실패");
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog("오류", $"ModBus 데이터 로드 실패: {ex.Message}");
                Logger.Error(ex, "Modbus 데이터 로드 실패");
            }
        }

        private async void InitializeParameterInBackground()
        {
            try
            {
                int start = Properties.Settings.Default.StartAddress;
                int end = Properties.Settings.Default.EndAddress;
                int numberOfPoints = end - start + 1;

                Parameters.Clear();

                var parametersTask = Task.Run(async () => {


                    return await GetReadModbusData(start, numberOfPoints);
                });

                var parameters = await parametersTask;

                if (parameters == null || !parameters.Any())
                {
                    AddLog("경고", "Modbus에서 파라미터 데이터를 가져오지 못했습니다.");
                    return;
                }

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var parameter in parameters)
                    {

                        Parameters.Add(parameter);
                    }
                });

                Logger.Info($"파라미터 생성 완료: {start}부터 {end}까지 {parameters.Count}개 생성됨");
                AddLog("초기화", $"파라미터 {parameters.Count}개 생성 완료");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "파라미터 생성 중 오류 발생");
                await Application.Current.Dispatcher.InvokeAsync(() => {
                    MessageBox.Show($"파라미터 생성 중 오류가 발생했습니다: {ex.Message}",
                        "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                });
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


                RaisePropertyChanged(nameof(CommunicationLog));

            });
        }






        //ModbusDataviewPage 데이터 업데이트 항목
        private void OnConnectionStatusChanged(bool isConnected)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (isConnected)
                {
                    _dataUpdateTimer?.Start();
                    LoadModbusData();

                    if (!_isParameterUpdateEnabled)
                    {
                        _isParameterUpdateEnabled = true;
                        StartParameterUpdateTimer();
                        AddLog("연결 상태", "네트워크 연결됨, 자동 파라미터 업데이트 시작");
                    }
                }
                else
                {
                    _dataUpdateTimer?.Stop();
                    IsRealTimeUpdate = false;

                    if (_isParameterUpdateEnabled)
                    {
                        _isParameterUpdateEnabled = false;
                        StopParameterUpdateTimer();
                        AddLog("연결 상태", "네트워크 연결 끊김, 자동 파라미터 업데이트 중지");
                    }
                }
            });
        }

        private void StartDataCollection()
        {
            _chartUpdateTimer.Start();
        }

        private void StopDataCollection()
        {
            _chartUpdateTimer.Stop();
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
            StopParameterUpdateTimer(); // 파라미터 갱신 타이머 중지
            _parameterUpdateTimer?.Dispose(); // 리소스 해제
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





        private DelegateCommand _toggleParameterUpdateCommand;
        public ICommand ToggleParameterUpdateCommand =>
            _toggleParameterUpdateCommand ??= new DelegateCommand(
                () => IsParameterUpdateEnabled = !IsParameterUpdateEnabled,
                () => _modbusConnect.IsConnected()
            );


        private Timer _parameterUpdateTimer;
        private bool _isParameterUpdateEnabled;

        public bool IsParameterUpdateEnabled
        {
            get => _isParameterUpdateEnabled;
            set
            {
                if (value && !_modbusConnect.IsConnected())
                {
                    MessageBox.Show("네트워크 연결이 없어 자동 업데이트를 시작할 수 없습니다.",
                        "연결 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SetProperty(ref _isParameterUpdateEnabled, value))
                {
                    if (value)
                        StartParameterUpdateTimer();
                    else
                        StopParameterUpdateTimer();
                }
            }
        }

        private async void DataUpdateTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (!_modbusConnect.IsConnected())
                {
                    _isParameterUpdateEnabled = false;
                    StopParameterUpdateTimer();
                    AddLog("타이머", "네트워크 연결이 끊겨 파라미터 업데이트를 중단합니다");
                    return;
                }

                _parameterUpdateTimer.Stop();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {



                    ExecuteGenerateParameters();

                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "자동 파라미터 갱신 중 오류 발생");
                AddLog("오류", $"자동 파라미터 갱신 중 오류: {ex.Message}");
            }
            finally
            {
                // 타이머가 활성화 상태이고 네트워크가 연결된 경우에만 다시 시작
                if (_isParameterUpdateEnabled && _modbusConnect.IsConnected())
                {
                    _parameterUpdateTimer.Start();
                }
            }
        }

        // 파라미터 갱신 타이머 시작
        private void StartParameterUpdateTimer()
        {
            try
            {
                // 연결이 되어 있는 경우에만 타이머 시작
                if (!_modbusConnect.IsConnected())
                {
                    AddLog("타이머", "네트워크가 연결되지 않아 파라미터 업데이트 타이머를 시작할 수 없습니다");
                    _isParameterUpdateEnabled = false;
                    return;
                }

                if (_parameterUpdateTimer == null)
                {
                    _parameterUpdateTimer = new Timer(1000); // 1초 간격
                    _parameterUpdateTimer.Elapsed += DataUpdateTimerElapsed;
                    _parameterUpdateTimer.AutoReset = false; // 단일 타이머 이벤트 후 중지
                }

                _parameterUpdateTimer.Start();
                Logger.Info("파라미터 자동 갱신 타이머 시작: 1초 간격");
                AddLog("타이머", "파라미터 자동 갱신이 시작되었습니다 (1초 간격)");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "파라미터 갱신 타이머 시작 중 오류");
                AddLog("오류", $"타이머 시작 실패: {ex.Message}");
                _isParameterUpdateEnabled = false;
            }
        }

        // 파라미터 갱신 타이머 중지
        private void StopParameterUpdateTimer()
        {
            if (_parameterUpdateTimer != null)
            {
                _parameterUpdateTimer.Stop();
                Logger.Info("파라미터 자동 갱신 타이머 중지");
                AddLog("타이머", "파라미터 자동 갱신이 중지되었습니다");
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
