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
using System.Collections.Generic;
using System.IO;
using Mvvm.Model.IniFileRead;
using MaterialDesignThemes.Wpf.Transitions;
using System.Threading.Tasks;
using System.Drawing;

namespace Mvvm.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private readonly ParameterWindowViewModel _parameterWindowViewModel;
        private string _title = "애플리케이션";
        private readonly Timer _timer;
        private readonly ModbusConnect _modbusConnect;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private ComboBox _portComBox;
        private string _selectPort;
        private SnackbarMessageQueue _bottomMessageQueue;



        #region 연결 아이콘 컬러 바꾸려고 만들어 놓은것들

        private string _icon = "Connector";
        private string _iconColor = "White";

        public string Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        public string IconColor
        {
            get => _iconColor;
            set => SetProperty(ref _iconColor, value);
        }

        #endregion


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

        private byte _slaveId;
        public byte SlaveId
        {
            get => _slaveId;
            set => SetProperty(ref _slaveId, value);
        }



        private int _delayBetweenPolls;
        public int DelayBetweenPolls
        {
            get => _delayBetweenPolls;
            set => SetProperty(ref _delayBetweenPolls, value);
        }

        private int _responseTimeout;
        public int ResponseTimeout
        {
            get => _responseTimeout;
            set => SetProperty(ref _responseTimeout, value);
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

        // Fields from SettingPageViewModel
        private string _selectedConnection;
        private string _selectedBaud;
        private string _selectedDataBit;
        private string _selectedParity;
        private string _selectedStopBit;
        private ushort _endAddress;
        private ushort _startAddress;





        private readonly SerialPortConfig _serialPortConfig;
        private readonly ExcelSettingsManager _settingsManager;

        public DelegateCommand SaveExcelCommand { get; private set; }

        // Properties from SettingPageViewModel
        public ObservableCollection<string> ConnectionOptions { get; set; }
        public ObservableCollection<string> BaudOptions { get; set; }
        public ObservableCollection<string> DataBitOptions { get; set; }
        public ObservableCollection<string> ParityOptions { get; set; }
        public ObservableCollection<string> StopBitOptions { get; set; }
        public ObservableCollection<ParameterModel> Parameters { get; set; }

        private string _baudRate;
        private string _portName;

        public string BaudRate
        {
            get => _baudRate;
            set => SetProperty(ref _baudRate, value);
        }

        public string PortName
        {
            get => _portName;
            set => SetProperty(ref _portName, value);
        }

        public string SelectedConnection
        {
            get => _selectedConnection;
            set
            {
                SetProperty(ref _selectedConnection, value);
                UpdateSerialPortConfig();
            }
        }

        public string SelectedBaud
        {
            get => _selectedBaud;
            set
            {
                SetProperty(ref _selectedBaud, value);
                UpdateSerialPortConfig();
            }
        }

        public string SelectedDataBit
        {
            get => _selectedDataBit;
            set
            {
                SetProperty(ref _selectedDataBit, value);
                UpdateSerialPortConfig();
            }
        }

        public string SelectedParity
        {
            get => _selectedParity;
            set
            {
                SetProperty(ref _selectedParity, value);
                UpdateSerialPortConfig();
            }
        }

        public string SelectedStopBit
        {
            get => _selectedStopBit;
            set
            {
                SetProperty(ref _selectedStopBit, value);
                UpdateSerialPortConfig();
            }
        }

        public ushort StartAddress
        {
            get => _startAddress;
            set
            {
                SetProperty(ref _startAddress, value);
                UpdateSerialPortConfig();
            }
        }

        public ushort EndAddress
        {
            get => _endAddress;
            set
            {
                SetProperty(ref _endAddress, value);
                UpdateSerialPortConfig();
            }
        }

        public DelegateCommand OpenExcelCommand { get; }
        public DelegateCommand<ParameterModel> SomeCommand { get; private set; }
        public event Action<List<ParameterModel>> ExcelDataLoaded;

        public MainWindowViewModel(IRegionManager regionManager, MainBottomBarViewModel mainBottomBarViewModel, ModbusConnect modbusConnect, ParameterWindowViewModel parameterWindowViewModel)
        {
            _regionManager = regionManager;
            _mainBottomBarViewModel = mainBottomBarViewModel;
            _modbusConnect = modbusConnect;
            _parameterWindowViewModel = parameterWindowViewModel;

            ShowMessageCommand = new DelegateCommand(ShowMessage);
            SomeCommand = new DelegateCommand<ParameterModel>(ExecuteMethod, CanExecuteMethod);
            NavigateToParameterWindowCommand = new DelegateCommand(NavigateToParameterWindow);
            NavigateToSettingWindowCommand = new DelegateCommand(NavigateToSettingWindow);
            NavigateToModbusDataViewPageCommand = new DelegateCommand(NavigateToModbusDataViewPage);
            NavigateToHomePageCommand = new DelegateCommand(HomePageLoad);
            PortConnectButton = new DelegateCommand(ConnectPorts, () => true);
            BottomMessageQueue = new SnackbarMessageQueue();

            _mainBottomBarViewModel.AdvancedModeChanged += OpenRightBar;


            _timer = new Timer(1000);
            _timer.Elapsed += (sender, e) => LoadAvailablePorts(_portComBox);
            _timer.Start();
            BottomMessageQueue.Enqueue("애플리케이션 시작", "OK", () => { });

            InitializeParameterWindowViewModel();

            // Initialize fields from SettingPageViewModel
            _serialPortConfig = new SerialPortConfig();
            _settingsManager = new ExcelSettingsManager();

            Parameters = new ObservableCollection<ParameterModel>();
            InitializeCollections();
            IconColor = "White";
            SetDefaultValues();

            OpenExcelCommand = new DelegateCommand(OpenExcelFile);
            SaveExcelCommand = new DelegateCommand(SaveSetting);
        }

        private void OpenRightBar(object sender, EventArgs e) {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
            MessageBox.Show("Advanced Mode Changed", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                DrawerHost.OpenDrawerCommand.Execute(Dock.Right, mainWindow.DrawerHost);
            }
        }

        private void InitializeParameterWindowViewModel()
        {
            // 필요한 초기화 로직
        }

        private void ShowMessage()
        {
            MessageBox.Show("버튼 클릭!", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool _isConnected = false;

        private async void ConnectPorts()
        {

            _isConnected = !_isConnected;

            if (_isConnected)
            {

                await _modbusConnect.ConnectToPort(SelectPort);

                if (_modbusConnect.IsConnected())
                {
                    BottomMessageQueue.Enqueue("포트 연결 성공", "OK", () => { });
                    _parameterWindowViewModel.InitializeWithPlot(new WpfPlot());
                    _parameterWindowViewModel.IsRealTimeUpdate = true;
                    Icon = "Checkmark";
                    IconColor = "#FFB46FFE";
                    _portComBox.IsEnabled = false;
                    _portComBox.Foreground = System.Windows.Media.Brushes.Gray;
                }
                else
                {
                    BottomMessageQueue.Enqueue("포트 연결 실패", "OK", () => { });
                    Icon = "Error";
                    IconColor = "Red";
                }
            }
            else {
                await _modbusConnect.DisconnectIfConnected();
                if (!_modbusConnect.IsConnected())
                {
                    BottomMessageQueue.Enqueue("포트 연결 해제", "OK", () => { });
                    Icon = "Connector";
                    IconColor = "White";
                    //  _portComBox.IsEnabled = true;
                }



            }
        }




        public void HomePageLoad()
        {
            _regionManager.RequestNavigate("ContentRegion", "HomePage");
        }

        private void NavigateToModbusDataViewPage()
        {
            _regionManager.RequestNavigate("ContentRegion", "ModbusDataViewPage");
        }

        private void NavigateToParameterWindow()
        {
            _regionManager.RequestNavigate("ContentRegion", "ParameterWindow");
        }

        private void NavigateToSettingWindow()
        {
            _regionManager.RequestNavigate("ContentRegion", "SettingPage");
        }







        public async Task LoadAvailablePorts(ComboBox portComBox)
        {
            if (portComBox == null) return;

            var ports = SerialPort.GetPortNames();

            try
            {
                portComBox.Dispatcher.Invoke(() =>
                {
                    portComBox.ItemsSource = ports;
                    _portComBox = portComBox;
                });
            }
            catch (Exception e)
            {
            MessageBox.Show("에러내용 :" + e.Message);

                throw;
             }

            if (int.TryParse(Properties.Settings.Default.BaudRate, out int baudRate))
            {
                PortConnected?.Invoke(_modbusConnect.portName, baudRate);
            }
            else
            {
                ShowError("에러.");
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void InitializeCollections()
        {
            ConnectionOptions = new ObservableCollection<string> { "SerialPort", "TCP/IP", "Option3" };
            BaudOptions = new ObservableCollection<string> { "9600", "19200", "38400", "57600", "115200", "128000" };
            DataBitOptions = new ObservableCollection<string> { "7", "8" };
            ParityOptions = new ObservableCollection<string> { "None", "Odd", "Even" };
            StopBitOptions = new ObservableCollection<string> { "1", "1.5", "2" };
        }

        private void ShowError(string message)
        {
            Logger.Error(message);
            MessageBox.Show(message, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void SetDefaultValues()
        {
            SelectedConnection = Properties.Settings.Default.SelectedConnection;
            SelectedBaud = Properties.Settings.Default.BaudRate;
            SelectedDataBit = Properties.Settings.Default.DataBits;
            SelectedParity = Properties.Settings.Default.Parity;
            SelectedStopBit = Properties.Settings.Default.StopBits;
            StartAddress = Properties.Settings.Default.StartAddress;
            EndAddress = Properties.Settings.Default.EndAddress;
            SlaveId = Properties.Settings.Default.SlaveId;

        }

        private void OpenExcelFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                Title = "엑셀 파일 선택",
                InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data")
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var modbusData = ExcelSettingsManager.LoadModbusParameters(openFileDialog.FileName);
                    ExcelSettingsManager.SaveModbusParametersToSettings(modbusData);
                    LogModbusParameters(modbusData);
                    ShowSuccess("엑셀 데이터를 성공적으로 로드했습니다.");
                }
                catch (Exception ex)
                {
                    ShowError($"엑셀 데이터 로드 중 오류가 발생했습니다.\n{ex.Message}");
                }
            }
        }

        private void LogModbusParameters(Dictionary<int, (string Size, string Description, string Unit, double DefaultValue, string NormalRange, string Func, string Note, string Endian, string Symbols, string Style)> modbusData)
        {
            foreach (var kvp in modbusData)
            {
                Logger.Info($"Index: {kvp.Key}, Size: {kvp.Value.Size}, Description: {kvp.Value.Description}, Unit: {kvp.Value.Unit}, DefaultValue: {kvp.Value.DefaultValue}, Note: {kvp.Value.Note}, 엔디안 {kvp.Value.Endian}");
            }
        }



        private void ExecuteMethod(ParameterModel parameter)
        {
            // 실행 로직
        }

        private bool CanExecuteMethod(ParameterModel parameter)
        {
            return parameter != null;
        }

        private void SaveSetting()
        {
            Properties.Settings.Default.SelectedConnection = SelectedConnection;
            Properties.Settings.Default.BaudRate = SelectedBaud;
            Properties.Settings.Default.DataBits = SelectedDataBit;
            Properties.Settings.Default.Parity = SelectedParity;
            Properties.Settings.Default.StopBits = SelectedStopBit;
            Properties.Settings.Default.StartAddress = StartAddress;
            Properties.Settings.Default.EndAddress = EndAddress;
            Properties.Settings.Default.SlaveId = SlaveId;

            Properties.Settings.Default.DelayBetweenPolls = DelayBetweenPolls;
            Properties.Settings.Default.ResponseTimeout = ResponseTimeout;
            Properties.Settings.Default.Save();

            _modbusConnect.LoadDefaultConfig();


            MessageBox.Show("설정을 성공적으로 저장했습니다.");


        }

        private void UpdateSerialPortConfig()
        {
            if (SelectedConnection == "SerialPort")
            {
                _serialPortConfig.startAddress = StartAddress;
                _serialPortConfig.numberOfPoints = (ushort)(EndAddress - StartAddress + 1);
            }
        }

        private void ShowSuccess(string message)
        {
            MessageBox.Show(message, "성공", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
