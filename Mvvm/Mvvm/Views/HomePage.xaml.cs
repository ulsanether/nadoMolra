using System.Windows.Controls;
using Accord.Statistics.Testing;
using Mvvm.Model;
using Mvvm.ViewModels;


namespace Mvvm.Views
{
    /// <summary>
    /// HomePage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HomePage : UserControl
    {

        private readonly HomePageViewModel _ViewModel;

        public HomePage(ModbusConnect modbusConnect)
        {
            InitializeComponent();
            _ViewModel = new HomePageViewModel(modbusConnect);
            DataContext = _ViewModel;
        }



    }
}
