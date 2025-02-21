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
            return mainWindow;
        }
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<MainWindow>();
            containerRegistry.RegisterForNavigation<HomePage>();
            containerRegistry.RegisterForNavigation<ParameterWindow>();
            containerRegistry.RegisterForNavigation<SettingPage>();

        }
    }
}
