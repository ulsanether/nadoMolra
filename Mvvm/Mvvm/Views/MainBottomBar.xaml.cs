using System.Windows.Controls;
using Mvvm.Model;
using Mvvm.ViewModels;
using Prism.Regions;
using Mvvm.Model;
namespace Mvvm.Views
{
    public partial class MainBottomBar : UserControl
    {


        private readonly MainBottomBarViewModel _viewModel;
        public MainBottomBar(ModbusConnect modbusConnect)
        {
            InitializeComponent();

            _viewModel = new MainBottomBarViewModel(modbusConnect);
            DataContext = _viewModel;
        }
    }
}
