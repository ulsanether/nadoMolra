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

namespace Mvvm.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;

        private string _title = "애플리케이션";
        private readonly Timer _timer;
        private ModbusConnect _modbusConnect;


        public MainBottomBarViewModel MainBottomBarViewModel { get; }


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


        #region 필요 없음 관계 된거 나중에 다 삭제
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

        //단위 테스트 확인용으로 놔둔것 
        public int plus(int n, int z)
        {
            return n + z;
        }


        #endregion





        public MainWindowViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            _modbusConnect = new ModbusConnect();


            ShowMessageCommand = new DelegateCommand(ShowMessage);


            NavigateToParameterWindowCommand = new DelegateCommand(NavigateToParameterWindow);
            NavigateToSettingWindowCommand = new DelegateCommand(NavigateToSettingWindow);
            NavigateToModbusDataViewPageCommand = new DelegateCommand(NavigateToModbusDataViewPage);
            NavigateToHomePageCommand = new DelegateCommand(HomePageLoad);
            PortConnectButton = new DelegateCommand(ConnectPorts);
    

            BottomMessageQueue = new SnackbarMessageQueue();
            MainBottomBarViewModel = new MainBottomBarViewModel(_modbusConnect);

            _timer = new Timer(1000);
            _timer.Elapsed += (sender, e) => LoadAvailablePorts(PortComBox);
            _timer.Start();
            BottomMessageQueue.Enqueue("애플리케이션 시작", "OK", () => { });

        }


        private async void ConnectPorts()
        {
            await _modbusConnect.ConnectToPort(SelectPort);

            if (_modbusConnect.IsConnected())
            {
                BottomMessageQueue.Enqueue("포트 연결 성공", "OK", () => { });
       
            }
            else
            {
                BottomMessageQueue.Enqueue("포트 연결 실패", "OK", () => { });
            }
        }


        #region 버튼 페이지 로드 부분 
        public void HomePageLoad()
        {
            _regionManager.RequestNavigate("ContentRegion", "HomePage");
        }
        private void NavigateToModbusDataViewPage() => _regionManager.RequestNavigate("ContentRegion", "ModbusDataViewPage");
        private void NavigateToParameterWindow() => _regionManager.RequestNavigate("ContentRegion", "ParameterWindow");
        private void NavigateToSettingWindow() => _regionManager.RequestNavigate("ContentRegion", "SettingPage");





        #endregion




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


        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
