using System.Windows;
using Mvvm.Model;
using Mvvm.ViewModels;
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
            return mainWindow;
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 싱글톤으로 ModbusConnect 등록
            containerRegistry.RegisterSingleton<ModbusConnect>();

            // ViewModels 등록
            containerRegistry.RegisterSingleton<SettingPageViewModel>();
            containerRegistry.Register<ParameterWindowViewModel>();
            containerRegistry.Register<ModbusDataViewPageViewModel>();
            containerRegistry.Register<HomePageViewModel>();

            // Views 등록
            containerRegistry.RegisterForNavigation<HomePage>();
            containerRegistry.RegisterForNavigation<SettingPage>();
            containerRegistry.RegisterForNavigation<ParameterWindow>();
            containerRegistry.RegisterForNavigation<ModbusDataViewPage>();
        }
    }
}

