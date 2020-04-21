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
using Android.Graphics.Drawables;
using Android.Graphics;
using Xamarin.Forms.Platform.Android;
using XButton = Xamarin.Forms.Button;
using System.ComponentModel;

namespace CleverEver.Droid.Renderers
{
    public class ButtonRenderer : Xamarin.Forms.Platform.Android.AppCompat.ButtonRenderer
    {
        private GradientDrawable _normal;
        private GradientDrawable _pressed;

        protected override void OnElementChanged(ElementChangedEventArgs<XButton> e)
        {
            base.OnElementChanged(e);
            if (Control == null)
                return;

            this.SetAlignment();
            float num = Math.Max(1f, this.Resources.DisplayMetrics.Density);
            var newElement = e.NewElement;

            MeasureSpecMode mode = View.MeasureSpec.GetMode(newElement.BorderRadius);
            float cornerRadius = (float)newElement.BorderRadius * num;
            double num2 = newElement.BorderWidth * (double)num;

            this._normal = new GradientDrawable();
            if (newElement.BackgroundColor.R == -1.0 && newElement.BackgroundColor.G == -1.0 && newElement.BackgroundColor.B == -1.0)
            {
                this._normal.SetColor(Color.ParseColor("#ff2c2e2f"));
            }
            else
            {
                this._normal.SetColor(ColorExtensions.ToAndroid(newElement.BackgroundColor));
            }
            this._normal.SetStroke((int)num2, ColorExtensions.ToAndroid(newElement.BorderColor));
            this._normal.SetCornerRadius(cornerRadius);

            Control.SetMaxLines(2);
            var fontSize = Element.FontSize;
            var text = Element.Text;
            this._pressed = new GradientDrawable();
            Color color = base.Context.ObtainStyledAttributes(new int[] { Android.Resource.Color.Transparent }).GetColor(0, Color.Gray);
            this._pressed.SetColor(color);
            this._pressed.SetStroke((int)num2, ColorExtensions.ToAndroid(newElement.BorderColor));
            this._pressed.SetCornerRadius(cornerRadius / 2);

            StateListDrawable stateListDrawable = new StateListDrawable();
            stateListDrawable.AddState(new int[] { Android.Resource.Attribute.StatePressed }, this._pressed);
            stateListDrawable.AddState(new int[] { }, this._normal);
            ViewExtensions.SetBackground(base.Control, stateListDrawable);
        }

        private void SetAlignment()
        {
            XButton button = base.Element as XButton;
            if (button == null || base.Control == null)
                return;

            base.Control.Gravity = GravityFlags.Center;
            Control.SetMaxLines(2);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            XButton button = (XButton)sender;
            if (this._normal != null && this._pressed != null)
            {
                if (e.PropertyName == "BorderRadius")
                {
                    this._normal.SetCornerRadius((float)button.BorderRadius);
                    this._pressed.SetCornerRadius((float)button.BorderRadius);
                }

                if (e.PropertyName == "BorderWidth" || e.PropertyName == "BorderColor")
                {
                    this._normal.SetStroke((int)button.BorderWidth, ColorExtensions.ToAndroid(button.BorderColor));
                    this._pressed.SetStroke((int)button.BorderWidth, ColorExtensions.ToAndroid(button.BorderColor));
                }
            }
        }
    }
}