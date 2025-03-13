using System.Threading.Tasks;
using System.Windows.Media;
using Prism.Mvvm;

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

        public int Address { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public double DefaultActual { get; set; }
        public string DefaultValue { get; set; }
        public string ModbusUnit { get; set; }
        public bool IsValueChanged { get; set; }
        public bool IsMonitoring { get; set; }
        public string StatusMessage { get; set; }
        public string StatusIcon { get; set; }
        public SolidColorBrush StatusColor { get; set; }

        public ParameterModel()
        {
            // 기본 생성자
        }

        private async void ResetValueChangedFlag()
        {
            await Task.Delay(1000);
            IsValueChanged = false;
            RaisePropertyChanged(nameof(IsValueChanged));
        }

        public void UpdateStatus(bool isSuccess, string message = null)
        {
            StatusMessage = message;
            StatusColor = isSuccess ? Brushes.Green : Brushes.Red;
            RaisePropertyChanged(nameof(StatusMessage));
            RaisePropertyChanged(nameof(StatusColor));
        }
    }
}
