using CleverEver.Composition;
using CleverEver.Localization;
using CleverEver.Pages.About;
using CleverEver.Pages.Game;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CleverEver.Phone.Pages.Home
{
    public partial class HomePage
    {
        private bool _isVisible;

        private MainPageViewModel ViewModel
        {
            get { return BindingContext as MainPageViewModel; }
        }

        public HomePage()
        {
            InitializeComponent();

            NavigationPage.SetHasNavigationBar(this, false);

            BindingContext = DependencyContainer.Resolve<MainPageViewModel>();
        }

        private async void Animate()
        {
            while (_isVisible)
            {
                await background.ScaleTo(1.1, 11000, easing: Easing.SinIn);
                if (!_isVisible)
                    break;
                await background.ScaleTo(1.2, 10000, easing: Easing.CubicOut);
                if (!_isVisible)
                    break;
                await background.ScaleTo(1.0, 11000, easing: Easing.SinOut);
            }
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
            ViewModel.OnAppearing();

            AnimationExtensions.AbortAnimation(background, "ScaleTo");
            _isVisible = true;
            Animate();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _isVisible = false;
        }

        private void Button_Clicked(object sender, System.EventArgs e)
        {
            var vm = new LeaderboardViewModel();
            Navigation.PushAsync(new LeaderboardView() { BindingContext = vm });
        }

        private void AboutButton_Clicked(object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new HowItWorks());
        }
    }
}

