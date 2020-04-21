using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using CleverEver.Rendering;
using System.Drawing;
using CleverEver.iOS.Rendering;

namespace CleverEver.iOS.Rendering
{
    class TextMeterImplementation : ITextMeter
    {
        public Xamarin.Forms.Size MeasureTextSize(string text, double width, double fontSize, string fontName = null)
        {
            var nsText = new NSString(text);
            var boundSize = new SizeF((float)width, float.MaxValue);
            var options = NSStringDrawingOptions.UsesFontLeading | NSStringDrawingOptions.UsesLineFragmentOrigin;

            if (fontName == null)
            {
                fontName = "HelveticaNeue";
            }

            var attributes = new UIStringAttributes
            {
                Font = UIFont.FromName(fontName, (float)fontSize)
            };

            var sizeF = nsText.GetBoundingRect(boundSize, options, attributes, null).Size;

            return new Xamarin.Forms.Size((double)sizeF.Width, (double)sizeF.Height);
            //return (double)sizeF.Height + 5;
        }
    }
}