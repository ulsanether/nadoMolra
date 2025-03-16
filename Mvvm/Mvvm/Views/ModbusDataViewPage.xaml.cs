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
        public ModbusDataViewPage(ParameterWindowViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _viewModel.InitializeWithPlot(WpfPlot);
            _viewModel.RefreshTemplateAction = RefreshTemplate;
            DataContext = _viewModel;

            // 상태 업데이트 타이머 설정
            _statusUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1000) // 100ms로 설정
            };
            _statusUpdateTimer.Tick += StatusUpdateTimer_Tick;
            _statusUpdateTimer.Start();
        }

        private void RefreshTemplate()
        {
            Dispatcher.Invoke(() =>
            {
                // Parameters DataGrid 새로고침
                if (ParametersDataGrid != null)
                {
                    ParametersDataGrid.Items.Refresh();
                }

                // Plot 업데이트
                if (WpfPlot != null)
                {
                    _viewModel.UpdatePlot();
                    WpfPlot.Refresh();
                }

                // CommunicationLog ListView 새로고침
                if (CommunicationLogListView != null)
                {
                    CommunicationLogListView.Items.Refresh();
                }
            });
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
