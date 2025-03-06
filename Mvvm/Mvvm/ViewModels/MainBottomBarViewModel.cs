using System;
using System.Windows;
using System.Windows.Threading;
using Mvvm.Model;
using Prism.Mvvm;

namespace Mvvm.ViewModels
{
    public class MainBottomBarViewModel : BindableBase, IDisposable
    {
        private string _portName;
        private string _baudRate;
        private string _portState;
        private bool _isConnected;
        private readonly ModbusConnect _modbusConnect;

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                if (SetProperty(ref _isConnected, value))
                {
                    System.Diagnostics.Debug.WriteLine($"IsConnected 변경됨: {value}");
                }
            }
        }

        public string PortState
        {
            get => _portState;
            set
            {
                if (SetProperty(ref _portState, value))
                {
                    System.Diagnostics.Debug.WriteLine($"PortState 변경됨: {value}");
                }
            }
        }

        public string PortName
        {
            get => _portName;
            set
            {
                System.Diagnostics.Debug.WriteLine($"PortName 설정 시도: {value}");
                if (SetProperty(ref _portName, value))
                {
                    System.Diagnostics.Debug.WriteLine($"PortName 설정 완료: {_portName}");
                }
            }
        }

        public string BaudRate
        {
            get => _baudRate;
            set
            {
                if (SetProperty(ref _baudRate, value))
                {
                    System.Diagnostics.Debug.WriteLine($"BaudRate 변경됨: {value}");
                }
            }
        }

        public MainBottomBarViewModel(ModbusConnect modbusConnect)
        {
            _modbusConnect = modbusConnect ?? throw new ArgumentNullException(nameof(modbusConnect));

            InitializeValues();
            _modbusConnect.ConnectionStatusChanged += OnConnectionStatusChanged;
        }

        private void InitializeValues()
        {
            if (!Dispatcher.CurrentDispatcher.CheckAccess())
            {
                Dispatcher.CurrentDispatcher.Invoke(InitializeValues);
                return;
            }

            IsConnected = _modbusConnect.IsConnected();
            PortState = IsConnected ? "Opened" : "Closed";
            PortName = _modbusConnect.portName ?? "No Port";
            BaudRate = _modbusConnect.serialPortConfig?.BaudRate.ToString() ?? "No BaudRate";
        }

        private void OnConnectionStatusChanged(bool isConnected)
        {
            if (!Dispatcher.CurrentDispatcher.CheckAccess())
            {
                Dispatcher.CurrentDispatcher.Invoke(() => OnConnectionStatusChanged(isConnected));
                return;
            }

            UpdateConnectionStatus(isConnected);
        }

        private void UpdateConnectionStatus(bool isConnected)
        {
            IsConnected = isConnected;
            PortState = isConnected ? "Opened" : "Closed";

            if (isConnected)
            {
                UpdatePortInformation();
            }
            else
            {
                PortName = "No Port";
                BaudRate = "No BaudRate";
            }
        }

        private void UpdatePortInformation()
        {
            if (_modbusConnect != null)
            {
                PortName = string.IsNullOrEmpty(_modbusConnect.portName)
                    ? "No Port"
                    : _modbusConnect.portName;

                BaudRate = _modbusConnect.serialPortConfig?.BaudRate.ToString()
                    ?? "No BaudRate";
            }
        }

        public void Dispose()
        {
            if (_modbusConnect != null)
            {
                _modbusConnect.ConnectionStatusChanged -= OnConnectionStatusChanged;
            }
        }
    }
}
