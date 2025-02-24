using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Windows;
using DevExpress.DashboardWeb.Native;
using DevExpress.Mvvm;
using Microsoft.Win32;
using Mvvm.Model;
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
        private ushort _startAddress;
        private ushort _endAddress;

        private readonly SerialPortConfig serialDataConfig = new SerialPortConfig();
        private readonly ExcelSettingsManager _settingsManager = new ExcelSettingsManager();




        public ObservableCollection<string> ConnectionOptions { get; set; }
        public ObservableCollection<string> BaudOptions { get; set; }
        public ObservableCollection<string> DataBitOptions { get; set; }
        public ObservableCollection<string> ParityOptions { get; set; }
        public ObservableCollection<string> StopBitOptions { get; set; }

        public ObservableCollection<ParameterModel> Parameters { get; set; } = new();   


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

        public ushort StartAddress
        {
            get => _startAddress;
            set
            {
                _startAddress = value;
                OnPropertyChanged();
                UpdateSerialPortConfig();
            }
        }

        public ushort EndAddress
        {
            get => _endAddress;
            set
            {
                _endAddress = value;
                OnPropertyChanged();
                UpdateSerialPortConfig();
            }
        }

        public DelegateCommand<object> OpenExcelCommand { get; }


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
            StartAddress = 0x0000;
            EndAddress = 0x0064;


            OpenExcelCommand = new DelegateCommand<object>(OpenExcelFile);
        }

        private void UpdateSerialPortConfig()
        {
            if (SelectedConnection == "SerialPort")
            {
                serialDataConfig.BaudRate = int.Parse(SelectedBaud ?? "9600");
                serialDataConfig.DataBits = int.Parse(SelectedDataBit ?? "8");
                serialDataConfig.Parity = (Parity)System.Enum.Parse(typeof(Parity), SelectedParity ?? "None");
                serialDataConfig.StopBits = (StopBits)System.Enum.Parse(typeof(StopBits), SelectedStopBit ?? "One");
                serialDataConfig.startAddress = StartAddress;
                serialDataConfig.numberOfPoints = (ushort)(EndAddress - StartAddress + 1);
            }
        }



        public ObservableCollection<string> ModbusNameList { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> ModbusUnitList { get; } = new ObservableCollection<string>();


        private void LoadExcelData()
        {


            var (modbusNameList, modbusUnitList) = _settingsManager.LoadDataFromSettings();
            ModbusNameList.Clear();
            foreach (var name in modbusNameList)
            {
                ModbusNameList.Add(name);
            }
            ModbusUnitList.Clear();
            foreach (var unit in modbusUnitList)
            {
                ModbusUnitList.Add(unit);
            }
            _settingsManager.PrintDataToConsole();

        }


        private void OpenExcelFile(object parameter)
        {
            var openFileDialog = new OpenFileDialog
            {


                Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                Title = "엑셀 파일 선택"
            };

            MessageBox.Show("엑셀 파일을 선택해주세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // 엑셀 데이터를 Settings에 저장
                    _settingsManager.SaveExcelDataToSettings(openFileDialog.FileName);
                    MessageBox.Show("엑셀 데이터를 성공적으로 저장했습니다.", "성공", MessageBoxButton.OK, MessageBoxImage.Information);

                    var (modbusNameList, modbusUnitList) = _settingsManager.LoadDataFromSettings();

                    ModbusNameList.Clear();
                    foreach (var name in modbusNameList)
                    {
                        ModbusNameList.Add(name);
                    }

                    ModbusUnitList.Clear();
                    foreach (var unit in modbusUnitList)
                    {
                        ModbusUnitList.Add(unit);
                    }

                    _settingsManager.PrintDataToConsole();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"엑셀 데이터를 저장하는 중 오류가 발생했습니다.\n\n{ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



    }
}
