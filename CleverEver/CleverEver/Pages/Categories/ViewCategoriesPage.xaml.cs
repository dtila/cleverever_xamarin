using CleverEver.Composition;
using CleverEver.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace CleverEver.Pages.Game
{
    public partial class ViewCategoriesPage : ContentPage
    {
        public ViewCategoriesViewModel ViewModel
        {
            get { return BindingContext as ViewCategoriesViewModel; }
        }

        public ViewCategoriesPage()
            : this (DependencyContainer.Resolve<ViewCategoriesViewModel>())
        {
        }

        public ViewCategoriesPage(ViewCategoriesViewModel vm)
        {
            InitializeComponent();
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

            ViewModel.OnDissapearing();
        }

        private async void OnItemTapped(object sender, EventArgs e)
        {
            var listView = sender as ListView;
            await ViewModel.Select((ViewCategoriesViewModel.CategoryViewModelItem)listView.SelectedItem);

            listView.SelectedItem = null;
        }
    }
}
