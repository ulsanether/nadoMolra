using System.Windows.Controls;
using Mvvm.ViewModels;
using Mvvm.Model;

namespace Mvvm.Views
{
    public partial class ParameterWindow : UserControl
    {
        private readonly ParameterWindowViewModel _viewModel;

        public ParameterWindow(ModbusConnect modbusConnect, SettingPageViewModel settingPageViewModel)
        {
            InitializeComponent();
            _viewModel = new ParameterWindowViewModel(modbusConnect, settingPageViewModel);
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
    }
}
