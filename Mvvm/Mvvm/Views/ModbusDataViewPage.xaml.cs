using System.Windows.Controls;
using Mvvm.Model;
using Mvvm.ViewModels;

namespace Mvvm.Views
{
    public partial class ModbusDataViewPage : UserControl
    {
        private readonly ModbusDataViewPageViewModel _viewModel;

        public ModbusDataViewPage()
        {
            InitializeComponent();
            var modbusConnect = new ModbusConnect(); 
            _viewModel = new ModbusDataViewPageViewModel(modbusConnect);
            DataContext = _viewModel;

        }

        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _viewModel.Cleanup();
        }
    }
}
