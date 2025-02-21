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
            var viewModel = Application.Current.MainWindow.DataContext as ParameterWindowViewModel;
            if (viewModel != null)
            {
                var itemsControl = Application.Current.MainWindow.FindName("ParameterItemsControl") as ItemsControl;
                if (itemsControl != null)
                {
                    var itemsSource = itemsControl.ItemsSource;
                    itemsControl.ItemsSource = null;
                    itemsControl.ItemsSource = itemsSource;
                }
            }
        }
    }
}
