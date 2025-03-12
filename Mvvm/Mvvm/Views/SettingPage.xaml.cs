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
public partial class SettingPage : UserControl
{
    public SettingPage()
    {
        InitializeComponent();
        var regionManager = (IRegionManager)Application.Current.Resources["RegionManager"];
        var mainBottomBarViewModel = (MainBottomBarViewModel)Application.Current.Resources["MainBottomBarViewModel"];
        var modbusDataViewPageViewModel = (ModbusDataViewPageViewModel)Application.Current.Resources["ModbusDataViewPageViewModel"];
        var modbusConnect = (ModbusConnect)Application.Current.Resources["ModbusConnect"];
        var parameterWindowViewModel = (ParameterWindowViewModel)Application.Current.Resources["ParameterWindowViewModel"];

        DataContext = new MainWindowViewModel(regionManager, mainBottomBarViewModel, modbusDataViewPageViewModel, modbusConnect, parameterWindowViewModel);
    }
}
