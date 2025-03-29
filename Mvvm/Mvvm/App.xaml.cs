// App.xaml.cs

using System.Windows;
using Mvvm.Model;
using Mvvm.ViewModels;
using Mvvm.Views;
using Prism.Events;
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
            var mainBottomBarViewModel = Container.Resolve<MainBottomBarViewModel>();
            var modbusConnect = Container.Resolve<ModbusConnect>();
            var homePageViewModel = Container.Resolve<HomePageViewModel>();
            var mainWindow = new MainWindow( Container.Resolve<IRegionManager>(),Container.Resolve<IRegionManager>(),
                mainBottomBarViewModel,
                modbusConnect);

            _regionManager = Container.Resolve<IRegionManager>();

            return mainWindow;
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<MainWindowViewModel>();
            containerRegistry.RegisterSingleton<ModbusConnect>();
            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();

            containerRegistry.RegisterSingleton<ParameterWindowViewModel>();
            containerRegistry.RegisterSingleton<HomePageViewModel>();
            containerRegistry.RegisterSingleton<MainBottomBarViewModel>();
            containerRegistry.RegisterForNavigation<HomePage>();
            containerRegistry.RegisterForNavigation<SettingPage>();
            containerRegistry.RegisterForNavigation<ParameterWindow>();
            containerRegistry.RegisterForNavigation<ModbusDataViewPage>();
            containerRegistry.RegisterForNavigation<MainBottomBar>();
        }


        protected override void InitializeModules()
        {
            base.InitializeModules();
        }
    }
}
