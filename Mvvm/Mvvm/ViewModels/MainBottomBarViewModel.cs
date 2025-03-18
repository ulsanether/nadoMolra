using Prism.Mvvm;
using Mvvm.Model;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Prism.Commands;
using System;

namespace Mvvm.ViewModels
{
    public class MainBottomBarViewModel : BindableBase
    {
        #region 필드
        private readonly ModbusConnect _modbusConnect;

        private int CountAdvanced = 0;


        #endregion



        public ICommand AdvancedModeCommand => new DelegateCommand(OnAdvancedModeCommand);



  

        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                if (SetProperty(ref _isConnected, value))
                {
                    UpdatePortState();
                }
            }
        }

        private string _portState;
        public string PortState
        {
            get => _portState;
            set => SetProperty(ref _portState, value);
        }

        private string _portName;
        public string PortName
        {
            get => _portName;
            set => SetProperty(ref _portName, value);
        }

        private int _baudRate;
        public int BaudRate
        {
            get => _baudRate;
            set => SetProperty(ref _baudRate, value);
        }

        public MainBottomBarViewModel(ModbusConnect modbusConnect)
        {
            _modbusConnect = modbusConnect;

            _modbusConnect.ConnectionStatusChanged += OnConnectionStatusChanged;

            InitializeValues();
        }

        public void SubscribeToPortConnectedEvent(MainWindowViewModel mainWindowViewModel)
        {
            mainWindowViewModel.PortConnected += OnPortConnected;
        }

        private void InitializeValues()
        {
            IsConnected = _modbusConnect.IsConnected();
            UpdatePortState();
            UpdatePortConfiguration();
        }


        private void OnAdvancedModeCommand()
        {

            CountAdvanced++;


            MessageBox.Show("Advanced Mode " + CountAdvanced.ToString());


        }

        private void UpdatePortConfiguration()
        {
            if (_modbusConnect.serialPortConfig != null)
            {
               BaudRate = _modbusConnect.serialPortConfig.BaudRate;
                PortName = _modbusConnect.portName ?? "없음";
            }
        }

        private void UpdatePortState()
        {
            PortState = IsConnected ? "연결됨" : "연결 안됨";
        }

        private void OnConnectionStatusChanged(bool isConnected)
        {
            IsConnected = isConnected;
            UpdatePortConfiguration();
        }

        private void OnPortConnected(string portName, int baudRate)
        {
              PortName = portName;
             BaudRate = baudRate;
        
        }
    }
}
