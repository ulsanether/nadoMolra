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
using Mvvm.Model;
using System.Diagnostics;

namespace Mvvm.ViewModels
{
    public class ModbusDataViewPageViewModel : BindableBase
    {
        #region Fields
        private readonly ModbusConnect _modbusConnect;
        private readonly ParameterWindowViewModel _parameterWindowViewModel;
        private readonly Timer _updateTimer;
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
        private ObservableCollection<string> _chartTypes;
        private ObservableCollection<string> _communicationLog;
        private ObservableCollection<ParameterModel> _availableParameters;
        #endregion

        #region Properties
        public object LockObject => _lockObject;
        public Queue<double> TimeData => _timeData;
        public Queue<double> ValueData => _valueData;
        public int MaxDataPoints => _maxDataPoints;

        public ObservableCollection<ParameterModel> AvailableParameters
        {
            get => _availableParameters;
            set => SetProperty(ref _availableParameters, value);
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

        public ObservableCollection<string> ChartTypes
        {
            get => _chartTypes;
            set => SetProperty(ref _chartTypes, value);
        }

        public ObservableCollection<string> CommunicationLog
        {
            get => _communicationLog;
            set => SetProperty(ref _communicationLog, value);
        }
        #endregion

        #region Commands
        public DelegateCommand ResetChartCommand { get; }
        public DelegateCommand ExportDataCommand { get; }
        public DelegateCommand ResetStatisticsCommand { get; }
        #endregion

        #region Constructor
        public ModbusDataViewPageViewModel(ModbusConnect modbusConnect, ParameterWindowViewModel parameterWindowViewModel, WpfPlot plot)
        {
            _modbusConnect = modbusConnect;
            _parameterWindowViewModel = parameterWindowViewModel;
            _wpfPlot = plot;

            AvailableParameters = new ObservableCollection<ParameterModel>();
            ResetChartCommand = new DelegateCommand(ResetChart);
            ExportDataCommand = new DelegateCommand(ExportData);
            ResetStatisticsCommand = new DelegateCommand(ResetStatistics);

            // 차트 업데이트용 타이머
            _updateTimer = new Timer(500);
            _updateTimer.Elapsed += OnUpdateTimerElapsed;
            _updateTimer.AutoReset = true;

            // 데이터 업데이트용 타이머
            _dataUpdateTimer = new Timer(1000); // 1초마다 업데이트
            _dataUpdateTimer.Elapsed += OnDataUpdateTimerElapsed;
            _dataUpdateTimer.AutoReset = true;
            _dataUpdateTimer.Start();

            _modbusConnect.ConnectionStatusChanged += OnConnectionStatusChanged;

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
                var modbusData = await _modbusConnect.ReadModbusData(1, 10);
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var parameter in modbusData)
                    {
                        var existingParameter = AvailableParameters.FirstOrDefault(p => p.Address == parameter.Address);
                        if (existingParameter != null)
                        {
                            if (existingParameter.DefaultActual != parameter.DefaultActual)
                            {
                                existingParameter.DefaultActual = parameter.DefaultActual;
                                existingParameter.IsValueChanged = true;
                            }
                        }
                        else
                        {
                            var indexList = Properties.Settings.Default.Index;
                            var descriptionList = Properties.Settings.Default.Description;
                            var unitList = Properties.Settings.Default.Unit;
                            var defaultValueList = Properties.Settings.Default.DefaultValue;

                            var newParameter = new ParameterModel
                            {
                                Address = parameter.Address,
                                DefaultActual = parameter.DefaultActual,
                                Description = $"Register {parameter.Address}",
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

                            AvailableParameters.Add(newParameter);
                        }
                    }

                    RaisePropertyChanged(nameof(AvailableParameters));
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Modbus 데이터 로드 실패: {ex.Message}");
            }
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
                _dataPlot = plt.Add.ScatterLine(times, values);

                double timeSpan = 10;
                double latestTime = times.Last();
                plt.Axes.SetLimits(
                    left: Math.Max(latestTime - timeSpan, times[0]),
                    right: latestTime,
                    bottom: values.Min() - 1,
                    top: values.Max() + 1
                );

                // 텍스트 추가
                for (int i = 0; i < times.Length; i++)
                {
                    plt.Add.Text(values[i].ToString(), times[i], values[i]);
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
}
