using Mvvm.Views;
using Org.BouncyCastle.Asn1.Crmf;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Windows;

namespace Mvvm.ViewModels
{
    public class ParameterWindowViewModel : BindableBase
    {
        private bool _isCardView;
        public bool IsCardView
        {
            get =>_isCardView;
            set
            {
                SetProperty(ref _isCardView, value);

                UpdateTemplate();
            }
        }

        private int _columns = 1;
        public int Columns
        {
            get => _columns;
            set => SetProperty(ref _columns, value);
        }

        private int _parameterCount;
        public int ParameterCount
        {
            get => _parameterCount;
            set
            {
                SetProperty(ref _parameterCount, value);
                UpdateParameters();
            }
        }

        public ObservableCollection<ParameterModel> Parameters { get; set; } = new();

        public DelegateCommand ApplyCommand { get; }

        public DelegateCommand UpdateTemplateCommand { get; }

        public ParameterWindowViewModel()
        {
            ApplyCommand = new DelegateCommand(UpdateParameters);
            UpdateTemplateCommand = new DelegateCommand(UpdateTemplate);
        }

        private void UpdateParameters()
        {
            Parameters.Clear();
            for (int i = 0; i < ParameterCount; i++)
            {
                double v = i + 1;
                Parameters.Add(new ParameterModel
                {
                    Label = $"Parameter {i + 1}",
                    DefaultActual = v
                });
            }
        }

        private void UpdateTemplate()
        {

            var selector = Application.Current.Resources["ParameterTemplateSelector"] as ParameterTemplateSelector;
           
            if (selector != null)
            {
                selector.RefreshTemplate();
            }
        }


    }
}
