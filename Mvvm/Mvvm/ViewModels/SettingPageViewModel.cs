using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Windows;
using Prism.Commands;
using Prism.Mvvm;
using Mvvm.Model.ComPort;
using Mvvm.Model.IniFileRead;

namespace Mvvm.ViewModels
{
    public class SettingPageViewModel : BindableBase
    {
        #region Fields
        private string _selectedConnection;
        private string _selectedBaud;
        private string _selectedDataBit;
        private string _selectedParity;
        private string _selectedStopBit;
        private ushort _startAddress;
        private ushort _endAddress;

        private readonly SerialPortConfig _serialPortConfig;
        private readonly ExcelSettingsManager _settingsManager;
        private readonly MainWindowViewModel _mainWindowViewModel;

        public DelegateCommand SaveExcelCommand { get; private set; }

        #endregion

        #region Properties
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
        #endregion

        #region Commands
        public DelegateCommand OpenExcelCommand { get; }
        #endregion

        #region Events
        public event Action<List<ParameterModel>> ExcelDataLoaded;
        #endregion
       
        #region Constructor

        public SettingPageViewModel()
        {
           

            _serialPortConfig = new SerialPortConfig();
            _settingsManager = new ExcelSettingsManager();

            Parameters = new ObservableCollection<ParameterModel>();
            InitializeCollections();

            SetDefaultValues();

            OpenExcelCommand = new DelegateCommand(OpenExcelFile);
            SaveExcelCommand = new DelegateCommand(ExecuteSaveExcel);

            try
            {
                LoadExcelData();
            }
            catch (Exception ex)
            {
                ShowError($"기본 엑셀 데이터 로드 실패: {ex.Message}");
            }
        }
        #endregion

        #region Private Methods
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
           
        }

        private void SetDefaultValues()
        {
            SelectedConnection = "SerialPort";
            SelectedBaud = "115200";
            SelectedDataBit = "8";
            SelectedParity = "None";
            SelectedStopBit = "1";
            StartAddress = 0x0000;
            EndAddress = 0x0064;
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
                    LoadParametersFromModbusData(modbusData);
                    ShowSuccess("엑셀 데이터를 성공적으로 로드했습니다.");
                }
                catch (Exception ex)
                {
                    ShowError($"엑셀 데이터 로드 중 오류가 발생했습니다.\n{ex.Message}");
                }
            }
        }

        private void LoadExcelData()
        {
            string defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "ModbusParameters.xlsx");

            if (File.Exists(defaultPath))
            {
                var modbusData = ExcelSettingsManager.LoadModbusParameters(defaultPath);
                LoadParametersFromModbusData(modbusData);
            }
        }

        private void ExecuteSaveExcel()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                Title = "엑셀 파일 저장",
                DefaultExt = ".xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    _settingsManager.SaveExcelDataToSettings(saveFileDialog.FileName);
                    ShowSuccess("엑셀 데이터를 성공적으로 저장했습니다.");
                }
                catch (Exception ex)
                {
                    ShowError($"엑셀 데이터 저장 중 오류가 발생했습니다.\n{ex.Message}");
                }
            }
        }

        private void LoadParametersFromModbusData(Dictionary<int, (string Description, string Unit, double DefaultValue, string Note)> modbusData)
        {
            Parameters.Clear();
            var parameterList = new List<ParameterModel>();

            foreach (var kvp in modbusData)
            {
                var parameter = new ParameterModel
                {
                    Address = kvp.Key,
                    Label = kvp.Value.Description,
                    Description = kvp.Value.Description,
                    DefaultValue = kvp.Value.DefaultValue.ToString(),
                    DefaultActual = kvp.Value.Note.Contains("나누기 10")
                        ? kvp.Value.DefaultValue * 10
                        : kvp.Value.DefaultValue,
                    ModbusUnit = kvp.Value.Unit
                };

                Parameters.Add(parameter);
                parameterList.Add(parameter);
            }

            ExcelDataLoaded?.Invoke(parameterList);
        }

        private void UpdateSerialPortConfig()
        {
            if (SelectedConnection == "SerialPort")
            {
                _serialPortConfig.BaudRate = int.Parse(SelectedBaud ?? "115200");
                _serialPortConfig.DataBits = int.Parse(SelectedDataBit ?? "8");
                _serialPortConfig.Parity = (Parity)Enum.Parse(typeof(Parity), SelectedParity ?? "None");
                _serialPortConfig.StopBits = (StopBits)Enum.Parse(typeof(StopBits), SelectedStopBit ?? "One");
                _serialPortConfig.startAddress = StartAddress;
                _serialPortConfig.numberOfPoints = (ushort)(EndAddress - StartAddress + 1);
            }
        }
        #endregion

        #region Helper Methods
        private void ShowSuccess(string message)
        {
            MessageBox.Show(message, "성공", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion
    }
}
