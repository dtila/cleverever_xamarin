using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Gms.Common.Apis;
using Android.Gms.Plus;
using Android.Gms.Common;
using CleverEver.Authentication;
using CleverEver.Droid;

namespace CleverEver.Phone.Droid.Authentication
{
    class GoogleOAuthProviderFactory : IOAuthProviderFactory
    {
        private MainActivity activity;

        public GoogleOAuthProviderFactory(MainActivity activity)
        {
            this.activity = activity;
        }

        public bool IsAvailable
        {
            get
            {
                return GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(activity) == 0;
            }
        }

        public GoogleOAuthProvider Create()
        {
            return new GoogleOAuthProvider(activity);
        }

        IOAuthProvider IOAuthProviderFactory.Create()
        {
            return Create();
        }
    }
}