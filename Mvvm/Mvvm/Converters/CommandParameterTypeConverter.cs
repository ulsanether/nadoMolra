using System;
using System.Globalization;
using System.Windows.Data;
using System.Diagnostics;

namespace Mvvm.Converters
{
    public class CommandParameterTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                Debug.WriteLine($"CommandParameter 타입: {value.GetType()}");
            }
            else
            {
                Debug.WriteLine("CommandParameter가 null입니다.");
            }
            return value; // 변환된 값을 반환합니다.
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
