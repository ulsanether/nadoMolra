// SettingPage.xaml.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Mvvm.ViewModels;
using Prism.Regions;
using Mvvm.Model;

namespace Mvvm.Views;
/// <summary>
/// SettingPage.xaml에 대한 상호 작용 논리
/// </summary>
///
public partial class SettingPage : UserControl
{
    private readonly MainWindowViewModel _viewModel;

    public SettingPage()
    {
        InitializeComponent();
        var modbusConnect = new ModbusConnect();
        var mainBottomBarViewModel = new MainBottomBarViewModel(modbusConnect);
        var parameterWindowViewModel = new ParameterWindowViewModel(modbusConnect);
        var regionManager = new RegionManager();

        _viewModel = new MainWindowViewModel(
            regionManager,
            mainBottomBarViewModel,
            modbusConnect,
            parameterWindowViewModel);

        DataContext = _viewModel;
    }
}
