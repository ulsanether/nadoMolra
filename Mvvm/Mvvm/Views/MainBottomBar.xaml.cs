using System.Windows.Controls;
using Mvvm.ViewModels;
using Prism.Regions;

namespace Mvvm.Views
{
    public partial class MainBottomBar : UserControl
    {
        public MainBottomBar(MainBottomBarViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
