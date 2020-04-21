using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace CleverEver.Pages.Game
{
    public partial class Result : ContentPage
    {
        private ResultViewModel ViewModel
        {
            get { return BindingContext as ResultViewModel; }
        }

        public Result()
        {
            InitializeComponent();

            pointsLabel.Scale = 0;
            bonusLabel.Scale = 0;

            NavigationPage.SetHasNavigationBar(this, false);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ViewModel.OnAppearing();

            Animate();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            ViewModel.OnDisappearing();
        }

        public async void Animate()
        {
            await Animate(pointsLabel);
            await Animate(bonusLabel);
        }

        private async Task Animate(View view)
        {
            await view.ScaleTo(1.2, 300, Easing.BounceIn);
            await view.ScaleTo(1, 100, Easing.Linear);
        }
    }
}
