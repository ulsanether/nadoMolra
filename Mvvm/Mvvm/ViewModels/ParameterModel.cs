using System.Threading.Tasks;
using System.Windows.Media;
using Prism.Mvvm;

namespace Mvvm.Model
{
    public class ParameterModel : BindableBase
    {
        private int _address;

        private string _description;
        private double _defaultActual;
        private string _defaultValue;
        private bool _isValueChanged;
        private bool _isMonitoring;
        private string _statusMessage;
        private string _statusIcon;
        private SolidColorBrush _statusColor;
        private int _index;
        private string _unit;
        private string _newValue;
        private string _endian;


        public string Endian
        {
            get => _endian;
            set => SetProperty(ref _endian, value);
        }


        public int Address { get; set; }

        public string Description { get; set; }

        public double DefaultActual
        {
            get => _defaultActual;
            set => SetProperty(ref _defaultActual, value);
        }

        public string DefaultValue { get; set; }


        private string _normalRange;

        public string NormalRange { get => _normalRange; set => SetProperty(ref _normalRange, value); }


        public string Func { get; set;}

        private string _note;

        public string Note { get => _note; set => SetProperty(ref _note, value); }

        private string _symbols;

        public string Symbols { get => _symbols; set => SetProperty(ref _symbols, value); }


        public string ModbusUnit { get; set; }
        public bool IsValueChanged
        {
            get => _isValueChanged;
            set => SetProperty(ref _isValueChanged, value);
        }
        public bool IsMonitoring { get; set; }
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
        public int Index
        {
            get => _index;
            set => SetProperty(ref _index, value);
        }
        public string Unit
        {
            get => _unit;
            set => SetProperty(ref _unit, value);
        }
        public string NewValue
        {
            get => _newValue;
            set
            {
                SetProperty(ref _newValue, value);


            }
        }
        public void NotifyPropertyChanged(string propertyName)
        {
            RaisePropertyChanged(propertyName);
        }

        public ParameterModel()
        {
        }
        private async void ResetValueChangedFlag()
        {
            await Task.Delay(1000);
            IsValueChanged = false;
        }

        public void UpdateStatus(bool isSuccess, string message = null)
        {
            StatusMessage = message;
            StatusIcon = isSuccess ? "Check" : "Close";
            StatusColor = isSuccess ? Brushes.Green : Brushes.Red;
        }
    }
}
