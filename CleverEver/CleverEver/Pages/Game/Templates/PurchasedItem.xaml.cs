using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace CleverEver.Pages.Game.Templates
{
    public partial class PurchasedItem
    {
        public PurchasedItem()
        {
            InitializeComponent();

            BindingContextChanged += PurchasedItem_BindingContextChanged;
        }

        private void PurchasedItem_BindingContextChanged(object sender, EventArgs e)
        {
            var binding = (sender as Element).BindingContext;
        }
    }
}
