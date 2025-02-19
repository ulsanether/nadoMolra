using System.Windows;
using Mvvm.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Mvvm
{
    public partial class App
    {
        private IRegionManager _regionManager;

        protected override Window CreateShell()
        {
            var mainWindow = Container.Resolve<MainWindow>();
            _regionManager = Container.Resolve<IRegionManager>();

            // 앱 시작 시 홈 페이지를 자동으로 로드
            _regionManager.RequestNavigate("ContentRegion", "HomePage");

            return mainWindow;
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<MainWindow>();

            // 네비게이션 등록
            containerRegistry.RegisterForNavigation<HomePage>();
            containerRegistry.RegisterForNavigation<ParameterWindow>();
            containerRegistry.RegisterForNavigation<SettingPage>();


        }
    }
}
