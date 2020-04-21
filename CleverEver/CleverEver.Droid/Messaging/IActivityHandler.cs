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

namespace CleverEver.Droid.Messaging
{
    interface IActivityHandler
    {
        bool OnActivityResult(int requestCode, Result resultCode, Intent data);
    }
}