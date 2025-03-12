using System.Windows.Controls;
using Mvvm.Model;
using Mvvm.ViewModels;
using Prism.Regions;
using Mvvm.Model;
namespace Mvvm.Views
{
    public partial class MainBottomBar : UserControl
    {


        public MainBottomBar()
        {
            InitializeComponent();
            DataContext = new MainBottomBarViewModel(new ModbusConnect());

        }
    }
}
