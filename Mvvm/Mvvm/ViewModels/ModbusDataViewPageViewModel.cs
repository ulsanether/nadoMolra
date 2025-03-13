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
using System.Drawing.Drawing2D;

namespace Mvvm.ViewModels
{
    public class ModbusDataViewPageViewModel : BindableBase
    {
        private readonly ModbusConnect _modbusConnect;
        private readonly Timer _updateTimer;
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

        public ObservableCollection<ParameterModel> AvailableParameters { get; }
        public DelegateCommand ResetChartCommand { get; }
        public DelegateCommand ExportDataCommand { get; }
        public DelegateCommand ResetStatisticsCommand { get; }

        public bool IsRealTimeUpdate
        {
            get => _isRealTimeUpdate;
            set
            {
                if (SetProperty(ref _isRealTimeUpdate, value))
                {
                    if (value)
                        StartDataCollection();
                    else
                        StopDataCollection();
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

        public ModbusDataViewPageViewModel(ModbusConnect modbusConnect, WpfPlot plot)
        {
            _modbusConnect = modbusConnect;
            _wpfPlot = plot;
            
            AvailableParameters = new ObservableCollection<ParameterModel>();
            ResetChartCommand = new DelegateCommand(ResetChart);
            ExportDataCommand = new DelegateCommand(ExportData);
            ResetStatisticsCommand = new DelegateCommand(ResetStatistics);

            _updateTimer = new Timer(50);
            _updateTimer.Elapsed += OnUpdateTimerElapsed;
            _updateTimer.AutoReset = false;

            _modbusConnect.ConnectionStatusChanged += OnConnectionStatusChanged;

            InitializeChart();
            InitializeChartTypes();
        }

        public void InitializeWithPlot(WpfPlot plot)
        {
            _wpfPlot = plot;
            InitializeChart();
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

        private void UpdatePlot()
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
            }

            _wpfPlot.Refresh();
        }

        private async void OnUpdateTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (SelectedParameter == null) return;

                var data = await _modbusConnect.ReadModbusData(
                    SelectedParameter.Address, 1);

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

        private void StartDataCollection()
        {
            _updateTimer.Start();
        }

        private void StopDataCollection()
        {
            _updateTimer.Stop();
        }

        private void OnConnectionStatusChanged(bool isConnected)
        {
            if (isConnected)
            {


                LoadModbusData();
            }
            else
            {
                IsRealTimeUpdate = false;
            }
        }

        private async void LoadModbusData()
        {
            try
            {
                var modbusData = await _modbusConnect.ReadModbusData(1, 10);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AvailableParameters.Clear();
                    foreach (var parameter in modbusData)
                    {
                        AvailableParameters.Add(parameter);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Modbus 데이터 로드 실패: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            // 데이터 내보내기 로직 추가
        }

        private void ResetStatistics()
        {
            // 통계 리셋 로직 추가
        }

        public void UpdateAvailableParameters(IEnumerable<ParameterModel> parameters)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                AvailableParameters.Clear();
                foreach (var param in parameters)
                {
                    AvailableParameters.Add(param);
                }

                if (AvailableParameters.Any())
                {
                    SelectedParameter = AvailableParameters.First();
                }
            });
        }


        public void UpdateCommunicationStatus()
        {
         
            bool isConnected = _modbusConnect.IsConnected();

            OnConnectionStatusChanged(isConnected);
        }

        public void Cleanup()
        {
            StopDataCollection();
            if (_modbusConnect != null)
            {
                _modbusConnect.ConnectionStatusChanged -= OnConnectionStatusChanged;
            }
        }
    }
}


