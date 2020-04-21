using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Graphics.Drawables;

namespace CleverEver.Droid
{
    using System.Threading;

    using Android.App;
    using Android.OS;
    using Droid;
    using Acr.Settings;
    using Android.Support.V7.App;
    using Android.Support.V7.Widget;

    [Activity(
        Label = "CleverEver",
        Theme = "@style/Theme.Splash",
        Icon = "@drawable/icon",
        MainLauncher = true,
        NoHistory = true)
    ]

    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            RequestWindowFeature(WindowFeatures.IndeterminateProgress);

            SetContentView(CleverEver.Droid.Resource.Layout.splash_screen);

            //CleverEver.Phone.Dependencies.Register();

            StartActivity(typeof(MainActivity));
        }
    }
}