using System.Windows.Controls;
using System.Windows.Threading;
using Mvvm.ViewModels;
using Mvvm.Model;
using System;
using System.Windows;
using System.Collections.Generic;

namespace Mvvm.Views
{
    public partial class ModbusDataViewPage : UserControl
    {
        private readonly ParameterWindowViewModel _viewModel;
        private readonly DispatcherTimer _statusUpdateTimer;

        public ModbusDataViewPage()
        {
            InitializeComponent();
            var modbusConnect = new ModbusConnect();
            _viewModel = new ParameterWindowViewModel(modbusConnect);
            _viewModel.InitializeWithPlot(WpfPlot);
            DataContext = _viewModel;

            // 타이머 초기화 및 설정
            _statusUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _statusUpdateTimer.Tick += StatusUpdateTimer_Tick;
            _statusUpdateTimer.Start();
        }

        private void StatusUpdateTimer_Tick(object sender, EventArgs e)
        {
            // 통신 상태 갱신
            _viewModel.UpdateCommunicationStatus();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _viewModel.Cleanup();
            _statusUpdateTimer.Stop();
        }

        private void OnAddButtonClick(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(ValueInputTextBox.Text, out double newValue))
            {
                lock (_viewModel.LockObject)
                {
                    var time = (DateTime.Now - DateTime.Today).TotalSeconds;
                    _viewModel.TimeData.Enqueue(time);
                    _viewModel.ValueData.Enqueue(newValue);

                    if (_viewModel.TimeData.Count > _viewModel.MaxDataPoints)
                    {
                        _viewModel.TimeData.Dequeue();
                        _viewModel.ValueData.Dequeue();
                    }

                    _viewModel.CurrentValue = newValue;
                    _viewModel.UpdatePlot();
                }
            }
            else
            {
                MessageBox.Show("유효한 숫자를 입력하세요.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
