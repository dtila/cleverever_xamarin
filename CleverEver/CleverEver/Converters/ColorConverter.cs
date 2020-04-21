using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CleverEver.Phone.Converters
{
    /// <summary>
    /// A color converter 
    /// </summary>
    public class TransparencyColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Color))
                throw new NotSupportedException("The provided value of the parameter is not a color");

            if (!(parameter is double))
                throw new NotSupportedException("The parameter is not a double");

            var color = (Color)value;
            var multiplier = (double)parameter;

            return color.MultiplyAlpha(multiplier);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("TransparencyColorConverter only supports one way bindings");
        }
    }
}
