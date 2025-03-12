using Mvvm.Model;
using Mvvm.ViewModels;
using Prism.Events;
using Prism.Regions;
using System.Windows;

namespace Mvvm.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _MainWindowViewModel;

        public MainWindow(IRegionManager regionManager, IRegionManager bottomRegionManager, MainBottomBarViewModel mainBottomBarViewModel, ModbusDataViewPageViewModel modbusDataViewPageViewModel, ModbusConnect modbusConnect)
        {
            InitializeComponent();

            var settingPageViewModel = new SettingPageViewModel();
            var parameterWindowViewModel = new ParameterWindowViewModel(modbusConnect, settingPageViewModel);

            _MainWindowViewModel = new MainWindowViewModel(regionManager, mainBottomBarViewModel, modbusDataViewPageViewModel, modbusConnect, parameterWindowViewModel);
            mainBottomBarViewModel.SubscribeToPortConnectedEvent(_MainWindowViewModel);

            var settingPage = new SettingPage();
            bottomRegionManager.RegisterViewWithRegion("BottmContentRegion", "MainBottomBar");
            Loaded += MainWindow_Loaded;
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _MainWindowViewModel.LoadAvailablePorts(PortComboBox);
            var viewModel = DataContext as MainWindowViewModel;
            viewModel.HomePageLoad();
        }
    }
}

