using System.Windows.Media;
using Prism.Mvvm;
using System.Threading.Tasks;
using System.Windows;

namespace Mvvm.ViewModels
{
    public class ParameterModel : BindableBase
    {
        private int _address;
        private string _label;
        private string _description;
        private double _defaultActual;
        private string _defaultValue;
        private string _modbusUnit;
        private bool _isValueChanged;
        private bool _isMonitoring;
        private string _statusMessage;
        private string _statusIcon;
        private SolidColorBrush _statusColor;

        public int Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        public string Label
        {
            get => _label;
            set => SetProperty(ref _label, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public double DefaultActual
        {
            get => _defaultActual;
            set
            {
                if (SetProperty(ref _defaultActual, value))
                {
                    IsValueChanged = true;
                    ResetValueChangedFlag();
                }
            }
        }

        public string DefaultValue
        {
            get => _defaultValue;
            set => SetProperty(ref _defaultValue, value);
        }

        public string ModbusUnit
        {
            get => _modbusUnit;
            set => SetProperty(ref _modbusUnit, value);
        }

        public bool IsValueChanged
        {
            get => _isValueChanged;
            set => SetProperty(ref _isValueChanged, value);
        }

        public bool IsMonitoring
        {
            get => _isMonitoring;
            set => SetProperty(ref _isMonitoring, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public string StatusIcon
        {
            get => _statusIcon;
            set => SetProperty(ref _statusIcon, value);
        }

        public SolidColorBrush StatusColor
        {
            get => _statusColor;
            set => SetProperty(ref _statusColor, value);
        }

        public ParameterModel()
        {
            StatusIcon = "CheckCircle";
            StatusColor = new SolidColorBrush(Colors.Green);
            StatusMessage = "준비";
        }

        private async void ResetValueChangedFlag()
        {
            await Task.Delay(1000);
            Application.Current.Dispatcher.Invoke(() =>
            {
                IsValueChanged = false;
            });
        }

        public void UpdateStatus(bool isSuccess, string message = null)
        {
            StatusIcon = isSuccess ? "CheckCircle" : "Alert";
            StatusColor = new SolidColorBrush(isSuccess ? Colors.Green : Colors.Red);
            StatusMessage = message ?? (isSuccess ? "정상" : "오류");
        }
    }
}
