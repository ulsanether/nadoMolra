using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using DevExpress.Mvvm;
using Mvvm.Model.ComPort;
using Serilog.Events;

namespace Mvvm.ViewModels
{
    public class SettingPageViewModel : BindableBase, INotifyPropertyChanged
    {
        private string _selectedConnection;
        private string _selectedBaud;
        private string _selectedDataBit;
        private string _selectedParity;
        private string _selectedStopBit;

        private readonly SerialPortConfig serialDataConfig = new SerialPortConfig();

        public ObservableCollection<string> ConnectionOptions { get; set; }
        public ObservableCollection<string> BaudOptions { get; set; }
        public ObservableCollection<string> DataBitOptions { get; set; }
        public ObservableCollection<string> ParityOptions { get; set; }
        public ObservableCollection<string> StopBitOptions { get; set; }




        public string SelectedConnection
        {
            get => _selectedConnection;
            set
            {
                _selectedConnection = value;
                OnPropertyChanged();
                UpdateSerialPortConfig();
            }
        }
        public string SelectedBaud
        {
            get => _selectedBaud;
            set
            {
                _selectedBaud = value;
                OnPropertyChanged();
                UpdateSerialPortConfig();
            }
        }

        public string SelectedDataBit
        {
            get => _selectedDataBit;
            set
            {
                _selectedDataBit = value;
                OnPropertyChanged();
                UpdateSerialPortConfig();
            }
        }

        public string SelectedParity
        {
            get => _selectedParity;
            set
            {
                _selectedParity = value;
                OnPropertyChanged();
                UpdateSerialPortConfig();
            }
        }

        public string SelectedStopBit
        {
            get => _selectedStopBit;
            set
            {
                _selectedStopBit = value;
                OnPropertyChanged();
                UpdateSerialPortConfig();
            }
        }




        public SettingPageViewModel(MainWindowViewModel mainWindowViewModel)
        {
            ConnectionOptions = new ObservableCollection<string> { "SerialPort", "TCP/IP", "Option3" };
            BaudOptions = new ObservableCollection<string> { "9600", "19200", "38400", "57600", "115200", "128000" };
            DataBitOptions = new ObservableCollection<string> { "7", "8" };
            ParityOptions = new ObservableCollection<string> { "None", "Odd", "Even" };
            StopBitOptions = new ObservableCollection<string> { "1", "1.5", "2" };

            SelectedConnection = "SerialPort";
            SelectedBaud = "9600";
            SelectedDataBit = "8";
            SelectedParity = "None";
            SelectedStopBit = "1";
        }


      private void UpdateSerialPortConfig() {
         if(SelectedConnection == "SerialPort") {
            serialDataConfig.BaudRate = int.Parse(SelectedBaud ?? "9600");
            serialDataConfig.DataBits = int.Parse(SelectedDataBit ?? "8");
            serialDataConfig.Parity = (Parity)System.Enum.Parse(typeof(Parity),SelectedParity ?? "None");
            serialDataConfig.StopBits = (StopBits)System.Enum.Parse(typeof(StopBits),SelectedStopBit ?? "One");
            }
         }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
