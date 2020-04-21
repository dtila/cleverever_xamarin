using CleverEver.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace CleverEver.Pages.Game
{
    public partial class ViewCategoryPage : ContentPage
    {
        private int _isSelecting;
        public ViewCategoryViewModel ViewModel
        {
            get { return BindingContext as ViewCategoryViewModel; }
        }

        public ViewCategoryPage(ViewCategoryViewModel vm)
        {
            InitializeComponent();
            _isSelecting = 0;

            Title = vm.Title;
            BindingContext = vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ViewModel.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            ViewModel.OnDisappearing();
        }

        private async void OnItemTapped(object sender, EventArgs e)
        {
            if (Interlocked.CompareExchange(ref _isSelecting, 1, 0) == 1)
                return;

            var listView = sender as ListView;
            await ViewModel.Select(listView.SelectedItem);
            listView.SelectedItem = null;

            _isSelecting = 0;
        }
    }
}
