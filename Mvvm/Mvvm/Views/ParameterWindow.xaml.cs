// ParameterWindow.xaml.cs

using System.Windows.Controls;
using Mvvm.ViewModels;
using Mvvm.Model;
using System.Windows.Threading;
using System;
using System.Windows;

namespace Mvvm.Views
{
    public partial class ParameterWindow : UserControl
    {
        private readonly ParameterWindowViewModel _viewModel;
        private readonly DispatcherTimer _statusUpdateTimer;
        // ParameterWindow.xaml.cs
        public ParameterWindow(ParameterWindowViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            _viewModel.RefreshTemplateAction = RefreshTemplate;

            //_statusUpdateTimer = new DispatcherTimer
            //{
            //    Interval = TimeSpan.FromMilliseconds(100)
            //};
         //   _statusUpdateTimer.Tick += StatusUpdateTimer_Tick;
        //  _statusUpdateTimer.Start();

        }
        private void StatusUpdateTimer_Tick(object sender, EventArgs e)
        {
            _viewModel.UpdateCommunicationStatus();
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
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.StartDataReading();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _viewModel.StopDataReading();
            _statusUpdateTimer.Stop();
        }

    }
}

