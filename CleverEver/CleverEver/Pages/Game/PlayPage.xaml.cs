using CleverEver.Analytics;
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
    public partial class PlayPage : ContentPage, IIdentifiable
    {
        private PlayViewModel ViewModel
        {
            get { return BindingContext as PlayViewModel; }
        }

        string IIdentifiable.Id
        {
            get { return "Play"; }
        }

        public PlayPage(PlayViewModel vm)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            BindingContextChanged += PlayPage_BindingContextChanged;
            BindingContext = vm;

            vm.PropertyChanged += Vm_PropertyChanged;
        }

        private async void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PlayViewModel.CongratulateText))
            {
                await congratulationMessage.ScaleTo(1.5, 300, easing: Easing.Linear);
                await congratulationMessage.ScaleTo(0.0, 500, easing: Easing.Linear);
            }
        }

        private void PlayPage_BindingContextChanged(object sender, EventArgs e)
        {
            var vm = BindingContext as PlayViewModel;
            if (vm == null)
                throw new InvalidOperationException("BindingContext needs to be a PlayViewModel instance");

            questionVM.Initialize(vm.Game);
        }

        protected override bool OnBackButtonPressed()
        {
            ViewModel.OnBackButtonPressed().ConfigureAwait(false);
            return true;
        }
    }
}
