using System.Windows;
using Mvvm.Views;
using Prism.Ioc;

namespace Mvvm
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App 
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {

         containerRegistry.RegisterSingleton<MainWindow>(); //싱글턴으로만 처리 

        }
    }
}
