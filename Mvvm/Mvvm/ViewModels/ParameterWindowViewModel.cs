using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using Mvvm.Model;
using Prism.Commands;
using Prism.Mvvm;

namespace Mvvm.ViewModels
{
    public class ParameterWindowViewModel : BindableBase
    {
        private readonly ModbusConnect _modbusConnect;
        private readonly SettingPageViewModel _settingPageViewModel;
        private int _startAddress;
        private int _endAddress;
        private bool _isCardView;
        private bool _isListView;
        private ObservableCollection<ParameterModel> _parameters;
        private Timer _updateTimer;

        public ParameterWindowViewModel(ModbusConnect modbusConnect, SettingPageViewModel settingPageViewModel)
        {
            _modbusConnect = modbusConnect;
            _settingPageViewModel = settingPageViewModel;
            GenerateParametersCommand = new DelegateCommand(GenerateParameters);
            Parameters = new ObservableCollection<ParameterModel>();
            StartAddress = 1;
            EndAddress = 20;

            // 타이머 설정
            _updateTimer = new Timer(100);
            _updateTimer.Elapsed += OnUpdateTimerElapsed;
            _updateTimer.Start();
        }

        public int StartAddress
        {
            get => _startAddress;
            set => SetProperty(ref _startAddress, value);
        }

        public int EndAddress
        {
            get => _endAddress;
            set => SetProperty(ref _endAddress, value);
        }

        public bool IsCardView
        {
            get => _isCardView;
            set => SetProperty(ref _isCardView, value);
        }

        public bool IsListView
        {
            get => _isListView;
            set => SetProperty(ref _isListView, value);
        }

        public ObservableCollection<ParameterModel> Parameters
        {
            get => _parameters;
            set => SetProperty(ref _parameters, value);
        }

        public ICommand GenerateParametersCommand { get; }

        public Action RefreshTemplateAction { get; set; }

        private async void GenerateParameters()
        {
            Parameters.Clear();
            var modbusData = await _modbusConnect.ReadModbusData(StartAddress, EndAddress - StartAddress + 1);
            for (int i = StartAddress; i <= EndAddress; i++)
            {
                var data = modbusData.FirstOrDefault(d => d.Address == i);
                Parameters.Add(new ParameterModel
                {
                    Address = i,
                    Label = $"레지스터 {i}",
                    Description = $"레지스터 {i} 설명",
                    DefaultActual = data?.DefaultActual ?? 0,
                    DefaultValue = data?.DefaultValue ?? "0",
                    ModbusUnit = data?.ModbusUnit ?? "unit"
                });
            }

            RefreshTemplateAction?.Invoke();
        }

        private async void OnUpdateTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // 데이터 갱신 로직 추가
            var updatedParameters = await _modbusConnect.ReadModbusData(StartAddress, EndAddress - StartAddress + 1);
            if (updatedParameters != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Parameters.Clear();
                    foreach (var param in updatedParameters)
                    {
                        Parameters.Add(param);
                    }
                });
            }
        }
    }
}
