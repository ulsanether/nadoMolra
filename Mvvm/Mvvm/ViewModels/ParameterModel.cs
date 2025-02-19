using Prism.Mvvm;

namespace Mvvm.ViewModels;

public class ParameterModel  :BindableBase
{


    private double _defaultActual;

    public string Label{ get; set; }

    public string ButtonContent { get; set; }   

    public double DefaultActual
    {
        get { return _defaultActual; }
        set { SetProperty(ref _defaultActual, value);   }
    }




}
