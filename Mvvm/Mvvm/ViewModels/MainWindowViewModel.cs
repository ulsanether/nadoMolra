using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System.Windows;
using Mvvm.Model;
using System.IO.Ports;
using System.Windows.Controls;
using System;
using Mvvm.Model.ComPort;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;

namespace Mvvm.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private string _title = "애플리케이션";
        private readonly Timer _timer;

        private ModbusConnect _modbusConnect;
        
      

        private ComboBox _portComBox;
        private string _selectPort;

        public string SelectPort
        {
            get => _selectPort;
            set => SetProperty(ref _selectPort, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public new event PropertyChangedEventHandler PropertyChanged;

        public DelegateCommand NavigateToParameterWindowCommand { get; }
        public DelegateCommand NavigateToSettingWindowCommand { get; }
        public DelegateCommand PortConnectButton { get; }

        #region 필요 없음 관계 된거 다 삭제 
        public DelegateCommand ShowMessageCommand { get; }

        private void ShowMessage()
        {
            MessageBox.Show("버튼 클릭!", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public ComboBox PortComBox
        {
            get => _portComBox;
            set => SetProperty(ref _portComBox, value);
        }
        #endregion

        public DelegateCommand LoadAvailablePortsCommand { get; }

        public MainWindowViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            _modbusConnect = new ModbusConnect();

            NavigateToParameterWindowCommand = new DelegateCommand(NavigateToParameterWindow);
            ShowMessageCommand = new DelegateCommand(ShowMessage);
            NavigateToSettingWindowCommand = new DelegateCommand(NavigateToSettingWindow);

            PortConnectButton = new DelegateCommand(ConnectPorts);

     
            _timer = new Timer(1000); 
            _timer.Elapsed += (sender, e) => LoadAvailablePorts(PortComBox);
            _timer.Start();
        }

        private async void ConnectPorts()
        {
            await _modbusConnect.ConnectToPort(SelectPort);
        }

        private void NavigateToSettingWindow() => _regionManager.RequestNavigate("ContentRegion", "SettingPage");

        public void LoadAvailablePorts(ComboBox portComBox)
        {
            if (portComBox == null) return;

            var ports = SerialPort.GetPortNames();
            portComBox.Dispatcher.Invoke(() =>
            {
                portComBox.ItemsSource = ports;
                PortComBox = portComBox;
            });
        }

        public int plus(int n, int z)
        {
            return n + z;
        }

        private void HomePageLoad()
        {
            _regionManager.RequestNavigate("ContentRegion", "HomePage");
        }

        private void NavigateToParameterWindow()
        {
            _regionManager.RequestNavigate("ContentRegion", "ParameterWindow");
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
