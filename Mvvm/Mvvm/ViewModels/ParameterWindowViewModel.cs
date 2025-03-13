// ParameterWindowViewModel.cs

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
using System.Windows.Controls;
using NLog;

namespace Mvvm.ViewModels
{
    public class ParameterWindowViewModel : BindableBase
    {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields
        private readonly ModbusConnect _modbusConnect;
        private readonly System.Timers.Timer _updateTimer;
        private readonly Dictionary<int, (string Description, string Unit, double DefaultValue, string Note)> _modbusData;
        private bool _isCardView;
        private int _columns = 1;
        private int _startAddress;
        private int _endAddress;

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
               // UpdateAddressCount();
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
               // UpdateAddressCount();
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

        public ObservableCollection<ParameterModel> ModbusDataViewParameters { get; } = new();
        #endregion

        #region Commands
        public DelegateCommand GenerateParametersCommand { get; }
        public DelegateCommand UpdateTemplateCommand { get; }
        public DelegateCommand<ParameterModel> WriteCommand { get; }
        #endregion

        #region Constructor
        public ParameterWindowViewModel(ModbusConnect modbusConnect)
        {
            _modbusConnect = modbusConnect;
            _modbusData = new Dictionary<int, (string, string, double, string)>();

            GenerateParametersCommand = new DelegateCommand(UpdateParameters);
            UpdateTemplateCommand = new DelegateCommand(UpdateTemplate);
            WriteCommand = new DelegateCommand<ParameterModel>(ExecuteWrite);

            _updateTimer = new System.Timers.Timer(100);
            _updateTimer.Elapsed += OnTimedEvent;
            _updateTimer.AutoReset = false;

            _modbusConnect.ConnectionStatusChanged += OnConnectionStatusChanged;

            try
            {
                string excelPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "ModbusParameters.xlsx");
              //  _modbusData = ExcelSettingsManager.LoadModbusParameters(excelPath);
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

                var modbusData = await _modbusConnect.ReadModbusData(StartAddress, EndAddress - StartAddress + 1);
                if (Application.Current?.Dispatcher != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Parameters.Clear();
                        ModbusDataViewParameters.Clear();

                        var indexList = Properties.Settings.Default.Index;
                        var descriptionList = Properties.Settings.Default.Description;
                        var unitList = Properties.Settings.Default.Unit;
                        var defaultValueList = Properties.Settings.Default.DefaultValue;

                        if (indexList != null && descriptionList != null && unitList != null && defaultValueList != null)
                        {
                            for (int i = 0; i < indexList.Count; i++)
                            {
                                int index = int.Parse(indexList[i]);
                                if (index < StartAddress || index > EndAddress) continue;

                                var defaultValue = double.Parse(defaultValueList[i]);

                                var parameter = new ParameterModel
                                {
                                    Address = index,
                                    Description = descriptionList[i],
                                    ModbusUnit = unitList[i],
                                    DefaultValue = defaultValue.ToString(),
                                    Index = index
                                };
                                Parameters.Add(parameter);

                                // NLog로 출력
                                Logger.Info($"Loaded Parameter - Description: {parameter.Description}, ModbusUnit: {parameter.ModbusUnit}, DefaultValue: {parameter.DefaultValue}");
                            }
                        }

                        foreach (var parameter in modbusData)
                        {
                            if (_modbusData != null && _modbusData.TryGetValue(parameter.Address, out var data))
                            {
                                parameter.Description = data.Description;
                                parameter.ModbusUnit = data.Unit;
                                parameter.DefaultValue = data.DefaultValue.ToString();
                                parameter.DefaultActual = double.Parse(parameter.DefaultValue);
                            }

                            // 중복 추가 방지 및 값 덮어쓰기
                            var existingParameter = Parameters.FirstOrDefault(p => p.Address == parameter.Address);
                            if (existingParameter != null)
                            {
                                existingParameter.Description = parameter.Description;
                                existingParameter.ModbusUnit = parameter.ModbusUnit;
                                existingParameter.DefaultValue = parameter.DefaultValue;
                                existingParameter.DefaultActual = parameter.DefaultActual;
                            }
                            else
                            {
                                Parameters.Add(parameter);
                            }
                            ModbusDataViewParameters.Add(parameter);
                        }
                    });
                }
                else
                {
                    ShowError("Dispatcher is not available.");
                }
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
                if (int.TryParse(parameter.NewValue, out int value))
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
        
                }
            }
            finally
            {
                _updateTimer.Start();
            }
        }


#if DEBUG 

        #endif




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
                    await Task.Delay(Properties.Settings.Default.ReadTimeout);  
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
