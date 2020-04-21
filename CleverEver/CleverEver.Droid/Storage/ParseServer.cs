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
using System.Threading.Tasks;

namespace CleverEver.Droid.Storage
{
    class ParseServer
    {
        private const string ParseApplicationId = "oYSKYCGDMBk0F4wbBB4ZlCo2EhlW4jgWNGFBI7N3";
        private const string ParseKey = "1DNFnue6T25yY65XqKkKVjyLT9ANC8F8vLEFj6BG";

        public static Task Setup()
        {
            // Initialize the Parse client with your Application ID and .NET Key found on
            // your Parse dashboard
            Parse.ParseClient.Initialize(new Parse.ParseClient.Configuration
            {
                ApplicationId = ParseApplicationId,
                WindowsKey = ParseKey,
                Server = "https://parseapi.back4app.com/"
            });


            /*var acl = new Parse.ParseACL();
            acl.PublicReadAccess = false;
            acl.PublicWriteAccess = false;

            var current = Parse.ParseInstallation.CurrentInstallation;
            current.ACL = acl;
            current["LastUsed"] = DateTime.Now;
             current.SaveAsync();*/
            return Task.CompletedTask;
        }
    }
}