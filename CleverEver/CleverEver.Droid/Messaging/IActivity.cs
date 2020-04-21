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
using Android.Support.V4.App;

namespace CleverEver.Droid.Messaging
{
    interface IActivity
    {
        FragmentActivity Cast { get; }

        event EventHandler Paused;
        event EventHandler Resumed;
        event EventHandler Started;
        event EventHandler Stopped;


        void AddActivityHandler(IActivityHandler handler);
        void RemoveActivityHandler(IActivityHandler handler);
    }
}