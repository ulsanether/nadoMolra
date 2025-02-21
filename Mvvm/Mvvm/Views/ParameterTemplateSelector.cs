using System.Windows;
using System.Windows.Controls;
using Mvvm.ViewModels;

namespace Mvvm.Views
{

    public class ParameterTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? CardTemplate { get; set; }
        public DataTemplate? ListTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {

            var itemsControl = ItemsControl.ItemsControlFromItemContainer(container);
            var viewModel = itemsControl?.DataContext as ParameterWindowViewModel;

            return viewModel?.IsCardView == true ? CardTemplate : ListTemplate;
        }
    }

}
