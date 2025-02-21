using System.Windows;
using System.Windows.Controls;
using Mvvm.ViewModels;

namespace Mvvm.Views
{
    public partial class ParameterWindow : UserControl
    {
        public ParameterWindow()
        {
            InitializeComponent();
            var viewModel = new ParameterWindowViewModel();
            DataContext = viewModel;
            viewModel.RefreshTemplateAction = RefreshTemplate;
        }

        private void RefreshTemplate()
        {
            var itemsControl = this.FindName("ParameterItemsControl") as ItemsControl;
            if (itemsControl != null)
            {
                var itemsSource = itemsControl.ItemsSource;
                itemsControl.ItemsSource = null;
                itemsControl.ItemsSource = itemsSource;
            }
        }
    }
}
