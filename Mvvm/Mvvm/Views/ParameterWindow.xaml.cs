// ParameterWindow.xaml.cs

using System.Windows.Controls;
using Mvvm.ViewModels;
using Mvvm.Model;

namespace Mvvm.Views
{
    public partial class ParameterWindow : UserControl
    {
        private readonly ParameterWindowViewModel _viewModel;

        public ParameterWindow(ModbusConnect modbusConnect)
        {
            InitializeComponent();
            _viewModel = new ParameterWindowViewModel(modbusConnect);
            DataContext = _viewModel;
            _viewModel.RefreshTemplateAction = RefreshTemplate;
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

        private void NumberValidationTextBox(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var regex = new System.Text.RegularExpressions.Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}

