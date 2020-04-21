using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OAuthentication;

namespace CleverEver.Phone.Droid.Authentication
{
    class FacebookOAuthProviderFactory : IOAuthProviderFactory
    {
        private MainActivity activity;

        public FacebookOAuthProviderFactory(MainActivity activity)
        {
            this.activity = activity;
        }

        public FacebookOAuthProvider Create()
        {
            return new FacebookOAuthProvider(activity);
        }

        IOAuthProvider IOAuthProviderFactory.Create()
        {
            return Create();
        }

        public bool IsAvailable
        {
            get
            {
                return true;
            }
        }
    }
}