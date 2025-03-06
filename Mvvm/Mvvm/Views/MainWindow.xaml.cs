using Mvvm.ViewModels;
using Prism.Regions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Mvvm.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _MainWindowViewModel;

        public MainWindow(IRegionManager regionManager, IRegionManager bottomRegionManager)
        {
            InitializeComponent();
            _MainWindowViewModel = new MainWindowViewModel(regionManager);
            var settingPage = new SettingPage(_MainWindowViewModel);
            bottomRegionManager.RegisterViewWithRegion("BottmContentRegion", "MainBottomBar");
            Loaded += MainWindow_Loaded;
        }

    
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _MainWindowViewModel.LoadAvailablePorts(PortComboBox);
            var viewModel = DataContext as MainWindowViewModel;
            viewModel.HomePageLoad();

        }
    }
}
