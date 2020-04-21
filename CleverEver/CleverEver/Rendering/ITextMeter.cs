using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Rendering
{
    //https://gist.github.com/alexrainman/82b00160ab32bef9e69dee6d460f44fa
    public interface ITextMeter
    {
        Xamarin.Forms.Size MeasureTextSize(string text, double width, double fontSize, string fontName = null);
    }
}
