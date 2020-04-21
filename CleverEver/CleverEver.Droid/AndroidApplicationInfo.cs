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
using Xamarin.Forms;
using CleverEver.Platform;

namespace CleverEver.Droid
{
    class AndroidApplicationInfo : IApplicationInfo
    {
        public int AppVersion
        {
            get 
            {
                var context = Forms.Context.ApplicationContext;
                return context.PackageManager.GetPackageInfo(context.PackageName, Android.Content.PM.PackageInfoFlags.Activities).VersionCode;
            }
        }

        public string AppIdentifier
        {
            get 
            {
                var context = Forms.Context.ApplicationContext;
                return context.PackageManager.GetPackageInfo(context.PackageName, Android.Content.PM.PackageInfoFlags.Activities).VersionName;
            }
        }
    }

    class AndroidDeviceInfo : IDevice
    {
        public string Platform
        {
            get { return "android"; }
        }

        public string PlatformVersion
        {
            get { return Build.VERSION.Sdk; }
        }

        public string Display
        {
            get { return Build.Display; }
        }

        public string Manufacturer
        {
            get { return Build.Manufacturer;; }
        }

        public string Model
        {
            get { return Build.Model; }
        }

        public void DebugWrite(string content)
        {
            System.Diagnostics.Debug.WriteLine(content);
        }

        public void Dial(string phoneNumber)
        {
            var uri = Android.Net.Uri.Parse("tel:" + phoneNumber);
            var intent = new Intent(Intent.ActionDial, uri);
            intent.SetFlags(ActivityFlags.NewTask);
            Forms.Context.StartActivity(intent);
        }
    }

}