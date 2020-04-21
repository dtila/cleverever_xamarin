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
using CleverEver.Authentication;

namespace CleverEver.Phone.Droid.Authentication
{
    class GoogleAuthenticationHelper
    {
        public const string SENDER_ID = "567969667145";
        public const string EXTRA_MESSAGE = "message";
        public const string PROPERTY_REG_ID = "registration_id";
        private const string PROPERTY_APP_VERSION = "appVersion";

        public static TokenResponse GetToken()
        {
            var appInfo = Composition.DependencyContainer.Resolve<Platform.IApplicationInfo>();

            var registrationId = GetRegistrationId(appInfo);
            if (string.IsNullOrEmpty(registrationId))
            {
                registrationId = Register();
                StoreRegistrationId(appInfo, registrationId);
            }

            return new TokenResponse(registrationId, DateTime.MaxValue);
        }

        public static Task<TokenResponse> GetTokenAsync()
        {
            return Task.Run(new Func<TokenResponse>(GetToken));
        }


        private static string Register()
        {
            var gcm = Android.Gms.Gcm.GoogleCloudMessaging.GetInstance(Android.App.Application.Context);
            return gcm.Register(SENDER_ID);
        }

        private static string GetRegistrationId(Platform.IApplicationInfo appInfo)
        {
            var prefs = GetGCMPreferences();
            string registrationId = prefs.GetString(PROPERTY_REG_ID, "");
            if (string.IsNullOrEmpty(registrationId))
                return null;

            // Check if app was updated; if so, it must clear the registration ID
            // since the existing registration ID is not guaranteed to work with
            // the new app version.
            int registeredVersion = prefs.GetInt(PROPERTY_APP_VERSION, Int32.MaxValue);

            int currentVersion = appInfo.AppVersion;
            if (registeredVersion != currentVersion)
                return "";

            return registrationId;
        }

        private static ISharedPreferences GetGCMPreferences()
        {
            // This sample app persists the registration ID in shared preferences, but
            // how you store the registration ID in your app is up to you.
            return Android.App.Application.Context.GetSharedPreferences("GCM", FileCreationMode.Private);
        }

        private static void StoreRegistrationId(Platform.IApplicationInfo appInfo, string regId)
        {
            var prefs = GetGCMPreferences();

            var editor = prefs.Edit();
            editor.PutString(PROPERTY_REG_ID, regId);
            editor.PutInt(PROPERTY_APP_VERSION, appInfo.AppVersion);
            editor.Commit();
        }
    }
}