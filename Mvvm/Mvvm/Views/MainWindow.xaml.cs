using Mvvm.ViewModels;
using Prism.Regions;
using System.Windows;

namespace Mvvm.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _MainWindowViewModel;

        public MainWindow(IRegionManager regionManager)
        {
            InitializeComponent();
            _MainWindowViewModel = new MainWindowViewModel(regionManager);
            var settingPage = new SettingPage(_MainWindowViewModel);
            // this.Content = settingPage;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _MainWindowViewModel.LoadAvailablePorts(PortComboBox);
        }
    }
}
