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
namespace Mvvm.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private string _title = "애플리케이션";

        PortConnect portConnector = new PortConnect();

        private SerialPortConfig _serialPortConfig;

        public SerialPortConfig SerialPortConfig
        {
            get => _serialPortConfig;

            set {
                _serialPortConfig = value;
                OnPropertyChanged();

            }
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }



        public event PropertyChangedEventHandler PropertyChanged;

        public DelegateCommand NavigateToParameterWindowCommand { get; }
        public DelegateCommand NavigateToSettingWindowCommand { get; }
        public DelegateCommand PortConnectButton { get; }

        #region 필요 없음 관계 된거 다 삭제 
        public DelegateCommand ShowMessageCommand { get; }

        private void ShowMessage()
        {
            MessageBox.Show("버튼 클릭!", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        #endregion


        public DelegateCommand LoadAvailablePortsCommand { get; }
        public MainWindowViewModel()
        {


        }
        public MainWindowViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            SerialPortConfig = new SerialPortConfig();
            // 기본 페이지 로드

            NavigateToParameterWindowCommand = new DelegateCommand(NavigateToParameterWindow);
            ShowMessageCommand = new DelegateCommand(ShowMessage);
            NavigateToSettingWindowCommand = new DelegateCommand(NavigateToSettingWindow);

            PortConnectButton = new DelegateCommand(ConnectPorts);

        }

        private void ConnectPorts() {

            portConnector = new PortConnect();
            portConnector.ConnectToPort("COM3");


        }


        private void NavigateToSettingWindow() => _regionManager.RequestNavigate("ContentRegion", "SettingPage");


        public void LoadAvailablePorts(ComboBox portComBox)
        {
            var ports = SerialPort.GetPortNames();
            portComBox.ItemsSource = ports;

        }

        private void HomePageLoad()
        {
            _regionManager.RequestNavigate("ContentRegion", "HomePage");

           // MessageBox.Show("애플리케이션 시작", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
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
