using Prism.Mvvm;

namespace Mvvm.ViewModels;

public class ParameterModel : BindableBase
{


    private double _defaultActual;
    private string _defaultValue;


    public string Label { get; set; }

    public string ButtonContent { get; set; }

    public double DefaultActual
    {
        get { return _defaultActual; }
        set { SetProperty(ref _defaultActual, value); }
    }

    public string DefaultValue{ get {
      return _defaultValue;

        } set {

       SetProperty(ref _defaultValue, value);

        } }

    public string ModbusUnit{ get; set; }

}
