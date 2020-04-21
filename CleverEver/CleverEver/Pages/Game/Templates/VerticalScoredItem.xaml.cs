using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace CleverEver.Pages.Game.Templates
{
    public partial class VerticalScoredItem : ContentView
    {
        public static readonly BindableProperty ImageHeightProperty = BindableProperty.Create("ImageHeight", typeof(double), typeof(VerticalScoredItem), 40.0);


        public double ImageHeight
        {
            get { return (double)GetValue(ImageHeightProperty); }
            set { SetValue(ImageHeightProperty, value); }
        }

        public VerticalScoredItem()
        {
            InitializeComponent();
        }
    }
}
