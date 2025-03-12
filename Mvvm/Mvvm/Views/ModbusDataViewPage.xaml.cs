// ModbusDataViewPage.xaml.cs
using System.Windows.Controls;
using Mvvm.ViewModels;
using Mvvm.Model;

namespace Mvvm.Views
{
    public partial class ModbusDataViewPage : UserControl
    {
        private readonly ModbusDataViewPageViewModel _viewModel;

        public ModbusDataViewPage()
        {
            InitializeComponent();
            var modbusConnect = new ModbusConnect();
            _viewModel = new ModbusDataViewPageViewModel(modbusConnect, RealTimeChart);
            DataContext = _viewModel;
        }
        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _viewModel.Cleanup();
        }
    }
}
