using System.Windows.Controls;
using Accord.Statistics.Testing;
using Mvvm.ViewModels;


namespace Mvvm.Views
{
    /// <summary>
    /// HomePage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HomePage : UserControl
    {


        private readonly MainWindowViewModel _mainWindowViewModel;

        public HomePage(MainWindowViewModel viewModel  )


        {
            InitializeComponent();

            _mainWindowViewModel = viewModel;

            DataContext = _mainWindowViewModel;
        }



    }
}
