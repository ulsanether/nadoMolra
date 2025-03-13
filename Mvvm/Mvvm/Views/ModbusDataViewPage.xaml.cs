// ModbusDataViewPage.xaml.cs
using System.Windows.Controls;
using System.Windows.Threading;
using Mvvm.ViewModels;
using Mvvm.Model;
using System;
using System.Windows;

namespace Mvvm.Views
{
    public partial class ModbusDataViewPage : UserControl
    {
        private readonly ModbusDataViewPageViewModel _viewModel;
        private readonly DispatcherTimer _statusUpdateTimer;

        public ModbusDataViewPage()
        {
            InitializeComponent();
            var modbusConnect = new ModbusConnect();
            _viewModel = new ModbusDataViewPageViewModel(modbusConnect, RealTimeChart);
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

        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _viewModel.Cleanup();
            _statusUpdateTimer.Stop();
        }
    }
}
