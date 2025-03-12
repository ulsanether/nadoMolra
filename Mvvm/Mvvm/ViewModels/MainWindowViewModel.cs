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
using Microsoft.Win32;
using System.Collections.ObjectModel;
using Mvvm.Views;
using MaterialDesignThemes.Wpf;
using ScottPlot.WPF;

namespace Mvvm.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private readonly SettingPageViewModel _settingPageViewModel;
        private readonly ModbusDataViewPageViewModel _modbusDataViewPageViewModel;
        private string _title = "애플리케이션";
        private readonly Timer _timer;
        private ModbusConnect _modbusConnect;

        private ComboBox _portComBox;
        private string _selectPort;
        private SnackbarMessageQueue _bottomMessageQueue;

        private ObservableCollection<string> _shortStringList = new();
        public ObservableCollection<string> ShortStringList
        {
            get => _shortStringList;
            set => SetProperty(ref _shortStringList, value);
        }

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
        public DelegateCommand NavigateToModbusDataViewPageCommand { get; }
        public DelegateCommand PortConnectButton { get; }
        public SnackbarMessageQueue BottomMessageQueue
        {
            get => _bottomMessageQueue;
            set => SetProperty(ref _bottomMessageQueue, value);
        }
        public DelegateCommand NavigateToHomePageCommand { get; }
        public DelegateCommand LoadAvailablePortsCommand { get; }
        public DelegateCommand ShowMessageCommand { get; }

        public event Action<string, int> PortConnected;

        private readonly MainBottomBarViewModel _mainBottomBarViewModel;

        public MainWindowViewModel(IRegionManager regionManager, MainBottomBarViewModel mainBottomBarViewModel, ModbusDataViewPageViewModel modbusDataViewPageViewModel)
        {
            _regionManager = regionManager;
            _mainBottomBarViewModel = mainBottomBarViewModel;
            _modbusDataViewPageViewModel = modbusDataViewPageViewModel;
            _modbusConnect = new ModbusConnect();

            ShowMessageCommand = new DelegateCommand(ShowMessage);

            NavigateToParameterWindowCommand = new DelegateCommand(NavigateToParameterWindow);
            NavigateToSettingWindowCommand = new DelegateCommand(NavigateToSettingWindow);
            NavigateToModbusDataViewPageCommand = new DelegateCommand(NavigateToModbusDataViewPage);
            NavigateToHomePageCommand = new DelegateCommand(HomePageLoad);
            PortConnectButton = new DelegateCommand(ConnectPorts);

            BottomMessageQueue = new SnackbarMessageQueue();

            _timer = new Timer(1000);
            _timer.Elapsed += (sender, e) => LoadAvailablePorts(_portComBox);
            _timer.Start();
            BottomMessageQueue.Enqueue("애플리케이션 시작", "OK", () => { });
        }

        private void ShowMessage()
        {
            MessageBox.Show("버튼 클릭!", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void ConnectPorts()
        {
            await _modbusConnect.ConnectToPort(SelectPort);

            if (_modbusConnect.IsConnected())
            {
                BottomMessageQueue.Enqueue("포트 연결 성공", "OK", () => { });
              //  PortConnected?.Invoke(_modbusConnect.portName, _modbusConnect.serialPortConfig.BaudRate);

                _modbusDataViewPageViewModel.InitializeWithPlot(new WpfPlot());
                _modbusDataViewPageViewModel.IsRealTimeUpdate = true;
                NavigateToModbusDataViewPage();

               // _mainBottomBarViewModel.OnPortConnected("11111", 19200);

            }
            else
            {
                BottomMessageQueue.Enqueue("포트 연결 실패", "OK", () => { });
            }
        }

        public void HomePageLoad()
        {
            _regionManager.RequestNavigate("ContentRegion", "HomePage");
        }
        private void NavigateToModbusDataViewPage() => _regionManager.RequestNavigate("ContentRegion", "ModbusDataViewPage");
        private void NavigateToParameterWindow() => _regionManager.RequestNavigate("ContentRegion", "ParameterWindow");
        private void NavigateToSettingWindow() => _regionManager.RequestNavigate("ContentRegion", "SettingPage");

        public void LoadAvailablePorts(ComboBox portComBox)
        {
            if (portComBox == null) return;

            var ports = SerialPort.GetPortNames();
            portComBox.Dispatcher.Invoke(() =>
            {
                portComBox.ItemsSource = ports;
                _portComBox = portComBox;
            });

            PortConnected?.Invoke(_modbusConnect.portName, _modbusConnect.serialPortConfig.BaudRate);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
