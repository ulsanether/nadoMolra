using DevExpress.Xpf.Core.Native;
using ImTools;
using Mvvm.Model;
using Mvvm.Model.ComPort;
using Mvvm.Views;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Mvvm.ViewModels
{
    public class ParameterWindowViewModel : BindableBase
    {
        private bool _isCardView;
        public bool IsCardView
        {
            get => _isCardView;
            set
            {
                SetProperty(ref _isCardView, value);
                UpdateTemplate();
            }
        }

        private int _columns = 1;
        public int Columns
        {
            get => _columns;
            set => SetProperty(ref _columns, value);
        }

        private int _parameterCount;
        public int ParameterCount
        {
            get => _parameterCount;
            set
            {
                SetProperty(ref _parameterCount, value);
                UpdateParameters();
            }
        }

        private int _startAddress;
        public int StartAddress
        {
            get => _startAddress;
            set
            {
                SetProperty(ref _startAddress, value);
                UpdateAddressCount();
            }
        }

        private int _endAddress;
        public int EndAddress
        {
            get => _endAddress;
            set
            {
                SetProperty(ref _endAddress, value);
                UpdateAddressCount();
            }
        }

        public ObservableCollection<ParameterModel> Parameters { get; set; } = new();

        public DelegateCommand ApplyCommand { get; }
        public DelegateCommand UpdateTemplateCommand { get; }
        public DelegateCommand LoadExcelCommand { get; }

        public Action RefreshTemplateAction { get; set; }

        private readonly ModbusConnect _modbusConnect;
        private readonly ExcelSettingsManager _settingsManager = new ExcelSettingsManager();

        public ParameterWindowViewModel(SettingPageViewModel settingPageViewModel)
        {
            _modbusConnect = new ModbusConnect();
            ApplyCommand = new DelegateCommand(UpdateParameters);
            UpdateTemplateCommand = new DelegateCommand(UpdateTemplate);
            LoadExcelCommand = settingPageViewModel.OpenExcelCommand;

            settingPageViewModel.ExcelDataLoaded += OnExcelDataLoaded;
        }

        private void OnExcelDataLoaded(List<ParameterModel> parameterList)
        {
            Parameters.Clear();
            foreach (var parameter in parameterList)
            {
                Parameters.Add(parameter);
            }
        }

        private void UpdateParameters()
        {
            Parameters.Clear();
            var modbusData = _modbusConnect.ReadModbusData(StartAddress, EndAddress - StartAddress + 1);
            foreach (var parameter in modbusData)
            {
                Parameters.Add(parameter);
            }
            UpdateAddressCount();
        }

        private void UpdateAddressCount()
        {
            var address = EndAddress - StartAddress;

            for (int i = 0; i < address; i++)
            {
                double v = i + 1;
                Parameters.Add(new ParameterModel
                {
                    Label = $"TestString {i + 1}",  //여기 부분 엑셀 name으로 변경
                    DefaultActual = v
                });
            }
        }

        private void UpdateTemplate()
        {
            Columns = IsCardView ? 3 : 1;
            RefreshTemplateAction?.Invoke();
        }
    }
}

