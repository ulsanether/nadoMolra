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

        public ObservableCollection<ParameterModel> AvailableParameters { get; }
        public DelegateCommand ResetChartCommand { get; }

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

        public ModbusDataViewPageViewModel(ModbusConnect modbusConnect)
        {
            _modbusConnect = modbusConnect;
            AvailableParameters = new ObservableCollection<ParameterModel>();
            ResetChartCommand = new DelegateCommand(ResetChart);

            _updateTimer = new Timer(50);
            _updateTimer.Elapsed += OnUpdateTimerElapsed;
            _updateTimer.AutoReset = false;

            _modbusConnect.ConnectionStatusChanged += OnConnectionStatusChanged;
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
            plt.Title("실시간 모드버스 데이터");
            plt.XLabel("시간 (초)");
            plt.YLabel("값");

            // 초기 데이터 설정
            double[] initialData = { 0 };
            double[] initialTimes = { 0 };

            // 라인 플롯 추가
            _dataPlot = plt.Add.ScatterLine(initialTimes, initialData);

            // 축 범위 설정
            plt.Axes.SetLimits(left: -10, right: 0, bottom: -10, top: 10);
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

                double timeSpan = 10; // 10초 구간만 표시
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
            if (!isConnected)
            {
                IsRealTimeUpdate = false;
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
