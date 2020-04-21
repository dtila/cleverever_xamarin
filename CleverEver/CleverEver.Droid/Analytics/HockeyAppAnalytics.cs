using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CleverEver.Logging;
using HockeyApp.Android;

[assembly: MetaData("net.hockeyapp.android.appIdentifier", Value = CleverEver.Droid.Analytics.HockeyAppAnalytics.ApiKey)]

namespace CleverEver.Droid.Analytics
{
    class HockeyAppAnalytics : IExceptionLogger
    {
        public const string ApiKey = "5bf83d8495f542e59958fb4c72010a32";

        static HockeyAppAnalytics()
        {
            CrashManager.Register(Application.Context, ApiKey);
        }

        public void LogError(Exception ex, string message = null, [CallerMemberName] string method = null)
        {
            var args = new Dictionary<string, string>
            {
                { "Message", message },
                { "Method", method ?? "null" },
                { "StackTrace", ex.StackTrace }
            };

            HockeyApp.Android.Metrics.MetricsManager.TrackEvent("Error", args);
        }
    }
}