using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Timers;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using Mvvm.Model;
using System.Windows.Media;

namespace Mvvm.ViewModels
{
    public class ParameterWindowViewModel : BindableBase
    {
        private readonly ModbusConnect _modbusConnect;
        private readonly Timer _updateTimer;
        private bool _isCardView;
        private int _columns = 1;
        private int _parameterCount;
        private int _startAddress;
        private int _endAddress;

        public Action RefreshTemplateAction { get; set; }

        #region Properties
        public bool IsCardView
        {
            get => _isCardView;
            set
            {
                SetProperty(ref _isCardView, value);
                UpdateTemplate();
            }
        }

        public int Columns
        {
            get => _columns;
            set => SetProperty(ref _columns, value);
        }

        public int ParameterCount
        {
            get => _parameterCount;
            set
            {
                SetProperty(ref _parameterCount, value);
                UpdateParameters();
            }
        }

        public int StartAddress
        {
            get => _startAddress;
            set
            {
                SetProperty(ref _startAddress, value);
                UpdateAddressCount();
            }
        }

        public int EndAddress
        {
            get => _endAddress;
            set
            {
                SetProperty(ref _endAddress, value);
                UpdateAddressCount();
            }
        }

        public ObservableCollection<ParameterModel> Parameters { get; } = new();
        #endregion

        #region Commands
        public DelegateCommand ApplyCommand { get; }
        public DelegateCommand UpdateTemplateCommand { get; }
        public DelegateCommand<ParameterModel> WriteCommand { get; }
        #endregion

        public ParameterWindowViewModel(ModbusConnect modbusConnect, SettingPageViewModel settingPageViewModel)
        {
            _modbusConnect = modbusConnect;

            ApplyCommand = new DelegateCommand(UpdateParameters);
            UpdateTemplateCommand = new DelegateCommand(UpdateTemplate);
            WriteCommand = new DelegateCommand<ParameterModel>(ExecuteWrite);

            _updateTimer = new Timer(100);
            _updateTimer.Elapsed += OnTimedEvent;
            _updateTimer.AutoReset = false;

            _modbusConnect.ConnectionStatusChanged += OnConnectionStatusChanged;
            settingPageViewModel.ExcelDataLoaded += OnExcelDataLoaded;
        }

        #region Private Methods
        private void UpdateTemplate()
        {
            Columns = IsCardView ? 3 : 1;
            RefreshTemplateAction?.Invoke();
        }

        private async void UpdateParameters()
        {
            try
            {
                var modbusData = await _modbusConnect.ReadModbusData(StartAddress, EndAddress - StartAddress + 1);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Parameters.Clear();
                    foreach (var parameter in modbusData)
                    {
                        Parameters.Add(parameter);
                    }
                });
            }
            catch (Exception ex)
            {
                ShowError($"파라미터 업데이트 실패: {ex.Message}");
            }
        }

        private void UpdateAddressCount()
        {
            if (EndAddress >= StartAddress)
            {
                var address = EndAddress - StartAddress;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Parameters.Clear();
                    for (int i = 0; i < address; i++)
                    {
                        Parameters.Add(new ParameterModel
                        {
                            Address = StartAddress + i,
                            Label = $"Register {StartAddress + i}",
                            DefaultValue = "0",
                            DefaultActual = 0,
                            ModbusUnit = "Raw",
                            Description = $"Modbus Register at address {StartAddress + i}"
                        });
                    }
                });
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

        private void ShowError(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
                MessageBox.Show(message, "오류", MessageBoxButton.OK, MessageBoxImage.Error));
        }
        #endregion
    }
}
