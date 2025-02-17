using Prism.Commands;
using Prism.Mvvm;

using System;
using System.Windows;

namespace Mvvm.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "에플리케이션";

        public DelegateCommand ShowMessageCommand { get; set; }  

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }


      public int plus(int i,int z) {

         return i+z;
      
      }

        public MainWindowViewModel()
        {
         ShowMessageCommand = new DelegateCommand(ShowMessage);
        }

      private void ShowMessage() {
         MessageBox.Show("버튼!","알림",MessageBoxButton.OK,MessageBoxImage.Information);
         }
      }
}
