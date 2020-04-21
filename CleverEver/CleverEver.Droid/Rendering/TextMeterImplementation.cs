using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CleverEver.Rendering;
using Android.Graphics;
using Android.Util;
using CleverEver.Droid.Rendering;

namespace CleverEver.Droid.Rendering
{
    class TextMeterImplementation : ITextMeter
    {
        public Xamarin.Forms.Size MeasureTextSize(string text, double width, double fontSize, string fontName = null)
        {
            var textView = new TextView(global::Android.App.Application.Context);
            Typeface typeFace = null;
            try
            {
                if (fontName != null)
                {
                    typeFace = Typeface.Create(fontName, TypefaceStyle.Normal);
                }

                textView.Typeface = typeFace ?? Typeface.Default;
                textView.SetText(text, TextView.BufferType.Normal);
                textView.SetTextSize(ComplexUnitType.Px, (float)fontSize);

                int widthMeasureSpec = Android.Views.View.MeasureSpec.MakeMeasureSpec(
                    (int)width, MeasureSpecMode.AtMost);
                int heightMeasureSpec = Android.Views.View.MeasureSpec.MakeMeasureSpec(
                    0, MeasureSpecMode.Unspecified);

                textView.Measure(widthMeasureSpec, heightMeasureSpec);

                return new Xamarin.Forms.Size(textView.MeasuredWidth, textView.MeasuredHeight);
            }
            finally
            {
                textView.Dispose();
                if (typeFace != null)
                    typeFace.Dispose();
            }
        }
    }
}