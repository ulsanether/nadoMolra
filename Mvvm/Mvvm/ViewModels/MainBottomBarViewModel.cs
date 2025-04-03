using Prism.Mvvm;
using Mvvm.Model;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Prism.Commands;
using System;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Drawing;
using ImTools;

namespace Mvvm.ViewModels
{
    public class MainBottomBarViewModel : BindableBase
    {
        #region 필드
        private readonly ModbusConnect _modbusConnect;
        private int CountAdvanced = 0;

        private int _baudRate;
        private string _portName;
        private string _portState;
        private bool _isConnected;
        private bool _alertButton;

        #endregion



        #region 프로퍼티
        public ICommand AdvancedModeCommand => new DelegateCommand(OnAdvancedModeCommand);
        public event EventHandler AdvancedModeChanged;

        public bool AlertButton{

            get=>_alertButton;
            set => SetProperty(ref _alertButton, value);

        }


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

        public string PortState
        {
            get => _portState;
            set => SetProperty(ref _portState, value);
        }

        public string PortName
        {
            get => _portName;
            set => SetProperty(ref _portName, value);
        }


        public int BaudRate
        {
            get => _baudRate;
            set => SetProperty(ref _baudRate, value);
        }

#endregion


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

        private async Task InitializeValues()
        {
            IsConnected = _modbusConnect.IsConnected();
            UpdatePortState();
            await UpdatePortConfiguration();
        }


        private void OnAdvancedModeCommand()
        {
            CountAdvanced++;
            if (CountAdvanced > 5) {
               AdvancedModeChanged?.Invoke(this, EventArgs.Empty);
                CountAdvanced = 0;
            }
        }





        private async  Task UpdatePortConfiguration()
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


    public class BooleanToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Brushes.Green : Brushes.Red;
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


}
