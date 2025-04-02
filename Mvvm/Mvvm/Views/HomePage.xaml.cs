using System.Threading.Tasks;
using System.Threading;
using System.Windows.Controls;
using Accord.Statistics.Testing;
using Mvvm.Model;
using Mvvm.ViewModels;
using System.Windows;


namespace Mvvm.Views
{
    /// <summary>
    /// HomePage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HomePage : UserControl
    {

        private readonly HomePageViewModel _viewModel;

        public HomePage(ModbusConnect modbusConnect)
        {
            InitializeComponent();
            _viewModel = new HomePageViewModel(modbusConnect);
            DataContext = _viewModel;
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
