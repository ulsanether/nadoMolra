using System.Windows;
using System.Windows.Controls;
using Mvvm.ViewModels;

namespace Mvvm.Views
{
    public class ParameterTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ListTemplate { get; set; }
        public DataTemplate CardTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var viewModel = Application.Current.MainWindow.DataContext as ParameterWindowViewModel;
            return viewModel?.IsCardView == true ? CardTemplate : ListTemplate;
        }

        public void RefreshTemplate()
        {
            // 템플릿 갱신을 트리거하기 위해 UI 강제 갱신
            Application.Current.Dispatcher.Invoke(() =>
            {
                var mainWindow = Application.Current.MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.DataContext = null;
                    mainWindow.DataContext = new ParameterWindowViewModel();
                }
            });
        }
    }
}
