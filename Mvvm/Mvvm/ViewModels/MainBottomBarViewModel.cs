using Prism.Mvvm;
using Mvvm.Model;
using System.Collections.ObjectModel;
using System.Windows;

namespace Mvvm.ViewModels
{
    public class MainBottomBarViewModel : BindableBase
    {
        private readonly ModbusConnect _modbusConnect;
<<<<<<< HEAD
        private bool _isConnected;
=======

>>>>>>> parent of 48f1f02 (bar 업데이트 수정중)
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
<<<<<<< HEAD
            set => SetProperty(ref _portState, value);
=======
            set
            {
                if (SetProperty(ref _portState, value))
                {
                    System.Diagnostics.Debug.WriteLine($"PortState 변경됨: {value}");
                }
            }
>>>>>>> parent of 48f1f02 (bar 업데이트 수정중)
        }

        private string _portName;
        public string PortName
        {
            get => _portName;
<<<<<<< HEAD
            set => SetProperty(ref _portName, value);
=======
            set
            {
                System.Diagnostics.Debug.WriteLine($"PortName 설정 시도: {value}");
                if (SetProperty(ref _portName, value))
                {
                    System.Diagnostics.Debug.WriteLine($"PortName 설정 완료: {_portName}");
                }
            }
>>>>>>> parent of 48f1f02 (bar 업데이트 수정중)
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
            IsConnected = true;
        }
    }
}
