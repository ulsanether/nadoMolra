using System.Windows;
using System.Windows.Controls;
using Mvvm.ViewModels;

namespace Mvvm.Views
{
public partial class ParameterWindow : UserControl
{
private readonly SettingPageViewModel _settingPageViewModel;

public ParameterWindow(SettingPageViewModel settingPageViewModel)
{
InitializeComponent();
_settingPageViewModel = settingPageViewModel;
var viewModel = new ParameterWindowViewModel(_settingPageViewModel);
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
