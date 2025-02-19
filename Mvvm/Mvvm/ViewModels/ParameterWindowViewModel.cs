using Mvvm.Views;
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
            get => _isCardView;
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

        // ApplyCommand: 데이터 갱신
        public DelegateCommand ApplyCommand { get; }

        // UpdateTemplateCommand: 템플릿 변경 버튼 동작
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
            // 템플릿 변경 메시지 (테스트용)
            MessageBox.Show($"템플릿 변경됨: {(IsCardView ? "카드" : "리스트")}", "Template Update");

            // 템플릿 업데이트 로직
            var selector = Application.Current.Resources["ParameterTemplateSelector"] as ParameterTemplateSelector;
            if (selector != null)
            {
                selector.RefreshTemplate();
            }
        }
    }
}
