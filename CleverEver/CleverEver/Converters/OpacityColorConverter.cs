using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CleverEver.Phone.Converters
{
    public class OpacityColorConverter : BindableObject, IMarkupExtension<Color>
    {
        private static Dictionary<int, Color> computedColors = new Dictionary<int, Color>();

        public static BindableProperty OpacityProperty = BindableProperty.Create("Opacity", typeof(double), typeof(OpacityColorConverter), 1.0, propertyChanging: OpacityPropertyChangingDelegate);
        public static BindableProperty ColorProperty = BindableProperty.Create("Color", typeof(Color), typeof(OpacityColorConverter), Color.Black);

        public static double GetOpacity(BindableObject view)
        {
            return (double)view.GetValue(OpacityProperty);
        }

        public static void SetOpacity(BindableObject view, double value)
        {
            view.SetValue(OpacityProperty, value);
        }

        public static Color GetColor(BindableObject view)
        {
            return (Color)view.GetValue(ColorProperty);
        }

        public static void SetColor(BindableObject view, Color value)
        {
            view.SetValue(ColorProperty, value);
        }

        public Color Color
        {
            get { return GetColor(this); }
            set { SetColor(this, value); }
        }

        public double Opacity
        {
            get { return GetOpacity(this); }
            set { SetOpacity(this, value); }
        }

        Color IMarkupExtension<Color>.ProvideValue(IServiceProvider serviceProvider)
        {
            return GetComputedColumn();
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return GetComputedColumn();
        }

        private Color GetComputedColumn()
        {
            var baseColor = Color;
            var opacity = Opacity;
            var hash = baseColor.GetHashCode() ^ opacity.GetHashCode();

            Color computedColor;
            if (computedColors.TryGetValue(hash, out computedColor))
                return computedColor;

            computedColor = Color.FromRgba(baseColor.R, baseColor.G, baseColor.B, opacity);
            computedColors.Add(hash, computedColor);
            return computedColor;
        }

        private static void OpacityPropertyChangingDelegate(BindableObject bindable, object oldValue, object newValue)
        {
            var newOpacity = (double)newValue;
            if (newOpacity < 0 || newOpacity > 1)
                throw new InvalidOperationException($"The opacity needs to be between 0 and 1");
        }
    }
}
