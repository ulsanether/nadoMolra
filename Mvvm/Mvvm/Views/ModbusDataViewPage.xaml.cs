using System.Windows.Controls;
using System.Windows.Threading;
using Mvvm.ViewModels;
using Mvvm.Model;
using System;
using System.Windows;
using System.Collections.Generic;

namespace Mvvm.Views
{
    public partial class ModbusDataViewPage : UserControl
    {
        private readonly ParameterWindowViewModel _viewModel;


        public ModbusDataViewPage(ParameterWindowViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _viewModel.InitializeWithPlot(WpfPlot);
            DataContext = _viewModel;


            // 차트 초기 설정
            WpfPlot.Plot.YLabel("값");
            WpfPlot.Plot.XLabel("시간 (초)");
            WpfPlot.Refresh();
        }



        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.StartDataReading();

        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _viewModel.StopDataReading();

        }

    }
}
