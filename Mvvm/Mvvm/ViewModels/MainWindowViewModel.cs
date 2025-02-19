using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System.Windows;

namespace Mvvm.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private string _title = "애플리케이션";

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public DelegateCommand NavigateToParameterWindowCommand { get; }
        public DelegateCommand ShowMessageCommand { get; }

        public MainWindowViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            // 기본 페이지 로드

            HomePageLoad();
            NavigateToParameterWindowCommand = new DelegateCommand(NavigateToParameterWindow);
            ShowMessageCommand = new DelegateCommand(ShowMessage);
       

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
