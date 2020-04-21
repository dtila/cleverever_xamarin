using CleverEver.Phone.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace CleverEver.Pages.Navigation
{
    public partial class DetailPage : ContentPage
    {
        public DetailPage(string title, View inner)
        {
            this.Icon = new FileImageSource();
            InitializeComponent();

            this.Content = inner;
            Title = title;

            var vm = inner.BindingContext as BaseViewModel;
            if (vm != null)
            {
                SetBinding(IsBusyProperty, new Binding("IsBusy", source: vm));
            }
        }
    }
}
