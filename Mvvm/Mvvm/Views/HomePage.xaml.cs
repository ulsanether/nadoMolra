using System.Windows.Controls;
using Mvvm.ViewModels;


namespace Mvvm.Views
{
    /// <summary>
    /// HomePage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HomePage : UserControl
    {
        public HomePage()
        {
            InitializeComponent();

            DataContext = new HomePageViewModel();
        }
    }
}
