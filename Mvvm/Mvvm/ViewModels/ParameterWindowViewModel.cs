using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Timers;
using System.Collections.Generic;
using Mvvm.Model;
using System.ComponentModel;
using Mvvm.Model.IniFileRead;

using System.Threading;
using System.Threading.Tasks;

namespace Mvvm.ViewModels
{
    public class ParameterWindowViewModel : BindableBase
    {
        #region Fields
        private readonly ModbusConnect _modbusConnect;
        private readonly System.Timers.Timer _updateTimer;
        private readonly Dictionary<int, (string Description, string Unit, double DefaultValue, string Note)> _modbusData;
        private bool _isCardView;
        private int _columns = 1;
        private int _startAddress;
        private int _endAddress;

        private readonly SettingPageViewModel _settingPageViewModel;
        private CancellationTokenSource _cancellationTokenSource; // 추가된 필드
        #endregion

        #region Properties
        public Action RefreshTemplateAction { get; set; }

        public bool IsCardView
        {
            get => _isCardView;
            set
            {
                SetProperty(ref _isCardView, value);
                UpdateTemplate();
            }
        }

        public bool IsListView
        {
            get => !_isCardView;
            set
            {
                IsCardView = !value;
            }
        }

        public int Columns
        {
            get => _columns;
            set => SetProperty(ref _columns, value);
        }

        public const int MAX_ADDRESS = 65535; // ushort의 최대값

        public int StartAddress
        {
            get => _startAddress;
            set
            {
                if (value < 0)
                {
                    ShowError("시작 주소는 0보다 작을 수 없습니다.");
                    return;
                }
                if (value > MAX_ADDRESS)
                {
                    ShowError($"시작 주소는 {MAX_ADDRESS}보다 클 수 없습니다.");
                    return;
                }
                if (value > EndAddress)
                {
                    ShowError("시작 주소는 끝 주소보다 클 수 없습니다.");
                    return;
                }

                SetProperty(ref _startAddress, value);
                UpdateAddressCount();
            }
        }

        public int EndAddress
        {
            get => _endAddress;
            set
            {
                if (value < 0)
                {
                    ShowError("끝 주소는 0보다 작을 수 없습니다.");
                    return;
                }
                if (value > MAX_ADDRESS)
                {
                    ShowError($"끝 주소는 {MAX_ADDRESS}보다 클 수 없습니다.");
                    return;
                }
                if (StartAddress > value)
                {
                    ShowError("끝 주소는 시작 주소보다 작을 수 없습니다.");
                    return;
                }

                if (value - StartAddress > 125)  // Modbus 프로토콜 제한
                {
                    ShowError("한 번에 읽을 수 있는 최대 레지스터 수는 125개입니다.");
                    return;
                }

                SetProperty(ref _endAddress, value);
                UpdateAddressCount();
            }
        }

        private void UpdateAddressCount()
        {
            try
            {
                if (EndAddress >= StartAddress)
                {
                    var address = EndAddress - StartAddress + 1;

                    if (address > 125)
                    {
                        ShowError("주소 범위가 너무 큽니다. 최대 125개까지 가능합니다.");
                        return;
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Parameters.Clear();
                        for (int i = 0; i < address; i++)
                        {
                            var currentAddress = StartAddress + i;
                            var parameter = new ParameterModel
                            {
                                Address = currentAddress,
                                Label = _modbusData.TryGetValue(currentAddress, out var data)
                                    ? data.Description
                                    : $"Register {currentAddress}",
                                DefaultValue = "0",
                                DefaultActual = 0,
                                ModbusUnit = _modbusData.TryGetValue(currentAddress, out data)
                                    ? data.Unit
                                    : "Raw",
                                Description = _modbusData.TryGetValue(currentAddress, out data)
                                    ? data.Description
                                    : $"Modbus Register at address {currentAddress}"
                            };
                            Parameters.Add(parameter);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                ShowError($"주소 업데이트 중 오류 발생: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        public ObservableCollection<ParameterModel> Parameters { get; } = new();
        #endregion

        #region Commands
        public DelegateCommand GenerateParametersCommand { get; }
        public DelegateCommand UpdateTemplateCommand { get; }
        public DelegateCommand<ParameterModel> WriteCommand { get; }
        #endregion

        #region Constructor
        public ParameterWindowViewModel(ModbusConnect modbusConnect, SettingPageViewModel settingPageViewModel)
        {
            _modbusConnect = modbusConnect;
            _modbusData = new Dictionary<int, (string, string, double, string)>();

            // Commands 초기화
            GenerateParametersCommand = new DelegateCommand(UpdateParameters);
            UpdateTemplateCommand = new DelegateCommand(UpdateTemplate);
            WriteCommand = new DelegateCommand<ParameterModel>(ExecuteWrite);

            // 타이머 초기화
            _updateTimer = new System.Timers.Timer(100);
            _updateTimer.Elapsed += OnTimedEvent;
            _updateTimer.AutoReset = false;

            // 이벤트 구독
            _modbusConnect.ConnectionStatusChanged += OnConnectionStatusChanged;
            settingPageViewModel.ExcelDataLoaded += OnExcelDataLoaded;
            // _settingPageViewModel.PropertyChanged += SettingPageViewModel_PropertyChanged;
            // 엑셀 데이터 로드 시도
            try
            {
                string excelPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "ModbusParameters.xlsx");
                _modbusData = ExcelSettingsManager.LoadModbusParameters(excelPath);
            }
            catch (Exception ex)
            {
                ShowError($"엑셀 데이터 로드 실패: {ex.Message}");
            }

            // 백그라운드 작업 시작
            StartBackgroundTask();
        }
        #endregion

        #region Private Methods

        private void SettingPageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_settingPageViewModel.BaudRate))
            {
                // BaudRate 변경 처리
            }
            else if (e.PropertyName == nameof(_settingPageViewModel.PortName))
            {
                // PortName 변경 처리
            }
        }

        private void UpdateTemplate()
        {
            Columns = IsCardView ? 3 : 1;
            RefreshTemplateAction?.Invoke();
        }

        public async void UpdateParameters()
        {
            try
            {
                if (EndAddress < StartAddress || StartAddress < 0)
                {
                    ShowError("유효하지 않은 주소 범위입니다.");
                    return;
                }

                var modbusData = await _modbusConnect.ReadModbusData(1, 10);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Parameters.Clear();
                    foreach (var parameter in modbusData)
                    {
                        if (_modbusData.TryGetValue(parameter.Address, out var data))
                        {
                            parameter.Description = data.Description;
                            parameter.ModbusUnit = data.Unit;
                            parameter.DefaultValue = data.DefaultValue.ToString();
                        }
                        Parameters.Add(parameter);
                    }
                });
            }
            catch (Exception ex)
            {
                ShowError($"파라미터 업데이트 실패: {ex.Message}");
            }
        }

        private async void ExecuteWrite(ParameterModel parameter)
        {
            try
            {
                if (int.TryParse(parameter.DefaultValue, out int value))
                {
                    await _modbusConnect.WriteRegister(parameter, value);
                    parameter.UpdateStatus(true, "쓰기 성공");
                }
                else
                {
                    parameter.UpdateStatus(false, "잘못된 값 형식");
                }
            }
            catch (Exception ex)
            {
                parameter.UpdateStatus(false, ex.Message);
            }
        }

        private void OnConnectionStatusChanged(bool isConnected)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var parameter in Parameters)
                {
                    parameter.UpdateStatus(isConnected, isConnected ? "연결됨" : "연결 끊김");
                }

                if (isConnected)
                    _updateTimer.Start();
                else
                    _updateTimer.Stop();
            });
        }

        private async void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            try
            {
                _updateTimer.Stop();
                var monitoringParameters = Parameters.Where(p => p.IsMonitoring).ToList();

                if (monitoringParameters.Any())
                {
                    var data = await _modbusConnect.ReadModbusData(StartAddress, EndAddress - StartAddress + 1);
                    UpdateParameterValues(data);
                }
            }
            finally
            {
                _updateTimer.Start();
            }
        }

        private void UpdateParameterValues(List<ParameterModel> newData)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                for (int i = 0; i < newData.Count && i < Parameters.Count; i++)
                {
                    if (Parameters[i].IsMonitoring)
                    {
                        Parameters[i].DefaultActual = newData[i].DefaultActual;
                        Parameters[i].ModbusUnit = newData[i].ModbusUnit;
                    }
                }
            });
        }

        private void OnExcelDataLoaded(List<ParameterModel> parameters)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Parameters.Clear();
                foreach (var param in parameters)
                {
                    Parameters.Add(param);
                }
                UpdateTemplate();
            });
        }

        private void StartBackgroundTask()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    UpdateParameters();
                    await Task.Delay(1000); // 1초마다 업데이트
                }
            }, token);
        }

        public void StopBackgroundTask()
        {
            _cancellationTokenSource?.Cancel();
        }

        #endregion
    }
}

