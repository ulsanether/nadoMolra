using System.Windows.Controls;
using Mvvm.ViewModels;
using Prism.Regions;

namespace Mvvm.Views
{
    public partial class MainBottomBar : UserControl
    {
<<<<<<< HEAD


        public MainBottomBar()
        {
            InitializeComponent();
            DataContext = new MainBottomBarViewModel(new ModbusConnect());

=======
        public MainBottomBar(MainBottomBarViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
>>>>>>> parent of 48f1f02 (bar 업데이트 수정중)
        }
    }
}
