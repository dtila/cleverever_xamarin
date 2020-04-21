using CleverEver.Analytics;
using CleverEver.Phone.Pages.Home;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace CleverEver.Pages.Navigation
{
    public partial class RootPage : NavigationPage
    {
        private IAnalyticsService _analytics;

        public RootPage()
        {
            InitializeComponent();

            Pushed += RootPage_Pushed;

            PushAsync(new HomePage()).ConfigureAwait(false);
        }

        private void RootPage_Pushed(object sender, NavigationEventArgs e)
        {
            var identifiable = e.Page as IIdentifiable;
            if (identifiable != null)
            {
                if (_analytics == null)
                    _analytics = CleverEver.Composition.DependencyContainer.Resolve<IAnalyticsService>();

                _analytics.SetCurrentScreen(identifiable.Id);
            }
        }
    }
}
