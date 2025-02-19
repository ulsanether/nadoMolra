using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System.Windows;
using Mvvm.Model;
using System.IO.Ports;
using System.Windows.Controls;
using System;
namespace Mvvm.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private string _title = "애플리케이션";

        PortConnect portConnector = new PortConnect();

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public DelegateCommand NavigateToParameterWindowCommand { get; }
        public DelegateCommand NavigateToSettingWindowCommand { get; }

        public DelegateCommand ShowMessageCommand { get; }

        public MainWindowViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            // 기본 페이지 로드


            NavigateToParameterWindowCommand = new DelegateCommand(NavigateToParameterWindow);
            ShowMessageCommand = new DelegateCommand(ShowMessage);

            NavigateToSettingWindowCommand = new DelegateCommand(NavigateToSettingWindow);
        }

        private void NavigateToSettingWindow() => _regionManager.RequestNavigate("ContentRegion", "SettingPage");

        public void LoadAvailablePorts(ComboBox portComBox)
        {
            var ports = SerialPort.GetPortNames();
            portComBox.ItemsSource = ports;
        }

        private void HomePageLoad()
        {
            _regionManager.RequestNavigate("ContentRegion", "HomePage");

           // MessageBox.Show("애플리케이션 시작", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void NavigateToParameterWindow()
        {
            _regionManager.RequestNavigate("ContentRegion", "ParameterWindow");
        }

        private void ShowMessage()
        {
            MessageBox.Show("버튼 클릭!", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
