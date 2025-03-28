using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Mvvm.Model
{
    public class CustomItem
    {
    public string Content{ get; set; }
    public string Width { get; set; }
    public double Height { get; set; }
    public Thickness Margin { get; set; }
    public Brush Background { get; set; }



    }
}
