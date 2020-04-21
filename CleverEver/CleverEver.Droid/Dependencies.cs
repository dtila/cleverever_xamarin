using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DryIoc;
using CleverEver.Droid;
using Plugin.Toasts;
using CleverEver.Composition;
using Android.Util;
using CleverEver.Game.Model;

namespace CleverEver.Phone.Droid.Infrastructure
{
    class Dependencies
    {
        public static void Setup(MainActivity activity)
        {
            Log.Error("Dependencies", "0");
            var container = new DryIoc.Container(Rules.Default.WithoutThrowOnRegisteringDisposableTransient());
            Log.Error("Dependencies", "1");
            container.RegisterDelegate<CleverEver.Droid.Messaging.IActivity>(r => activity, Reuse.Singleton);


            // Cancel
            //container.Register<Purchasing.IPurchasingService, CleverEver.Droid.Purchasing.GooglePurchasingService>();
            //container.Register<Multiplayer.IMultiplayerServerHost, Multiplayer.App42.App42MultiplayerServerHost>();
            container.Register<Game.IGameServer, CleverEver.Droid.Connectivity.Google.GoogleServer>(Reuse.Singleton);


            // Initialize facebook
            /*Xamarin.Facebook.FacebookSdk.SdkInitialize(activity);
            container.RegisterDelegate<OAuthentication.IOAuthProviderFactory>(r => new Authentication.FacebookOAuthProviderFactory(activity), Reuse.Singleton, serviceKey: Authentication.FacebookOAuthProvider.ProviderName);
            container.Register(made: Made.Of(r => DryIoc.ServiceInfo.Of<OAuthentication.IOAuthProviderFactory>(serviceKey: Authentication.FacebookOAuthProvider.ProviderName), f => f.Create()),
                serviceKey: Authentication.FacebookOAuthProvider.ProviderName);

            // Initialize Google
            container.RegisterDelegate<OAuthentication.IOAuthProviderFactory>(r => new Authentication.GoogleOAuthProviderFactory(activity), Reuse.Singleton, serviceKey: Authentication.GoogleOAuthProvider.ProviderName);
            container.Register(made: Made.Of(r => DryIoc.ServiceInfo.Of<OAuthentication.IOAuthProviderFactory>(serviceKey: Authentication.GoogleOAuthProvider.ProviderName), f => f.Create()),
                serviceKey: Authentication.GoogleOAuthProvider.ProviderName);*/
            
            // Device
            //container.Register<IApplicationInfo, AndroidApplicationInfo>();
            //container.Register<IDevice, AndroidDeviceInfo>();
            container.RegisterDelegate<Acr.Settings.ISettings>(r => new Acr.Settings.SettingsImpl(), reuse: Reuse.Singleton);
            container.RegisterDelegate<Acr.UserDialogs.IUserDialogs>(r => Acr.UserDialogs.UserDialogs.Instance);
            container.Register<Rendering.ITextMeter, CleverEver.Droid.Rendering.TextMeterImplementation>();
            Log.Error("Dependencies", "2");
            // Toast Notification
            container.Register<IToastNotificator, Plugin.Toasts.ToastNotification>();
            ToastNotification.Init(activity);
            Log.Error("Dependencies", "3");

            /*container.Register<Copas.ApplicationServices.Storage.ICrashFileStorage, Storage.CrashFileStorage>();
            container.Register<Copas.ApplicationServices.Notifications.IPushNotificationServer, Notifications.GoogleCloudNotificationServer>();
            container.RegisterDelegate<ApplicationServices.Connectivity.INetworkConnectivity>(r => new Connectivity.NetworkConnectivityAdapter(activity), Reuse.Singleton);
            
            container.Register<ApplicationServices.Connectivity.Web.IWebRequestFactory, Connectivity.Web.RestSharpHttpFactory>();*/

            // Authentication
            container.RegisterMany(
                new[] { typeof(CleverEver.Authentication.IAuthenticationService), typeof(CleverEver.Droid.Authentication.ParseAuthentication) },
                typeof(CleverEver.Droid.Authentication.ParseAuthentication));

            // Storage
            container.RegisterMany(
               new[] { typeof(Game.Repositories.IQuestionRepository), typeof(Game.Repositories.IContestRepository) },
               typeof(CleverEver.Droid.Storage.ParseRepository));
            container.Register<Storage.ILocalStorage<IEnumerable<Category>>, CleverEver.Droid.Storage.FileQuestionsRepository>();
            Log.Error("Dependencies", "4");

            // Analytics
            container.Register<Analytics.IAnalyticsService, CleverEver.Droid.Analytics.GoogleAnalyticsService>(Reuse.Singleton);
            container.Register<Logging.IExceptionLogger, CleverEver.Droid.Analytics.FirebaseCrashReportingService>(Reuse.Singleton);
            Log.Error("Dependencies", "5");


            /*container.Register<Copas.ApplicationServices.Storage.ILocalStorageRepository<ReferenceData>, Storage.ApplicationCacheRepository>();
            container.Register<Copas.ApplicationServices.Storage.ILocalStorageRepository<IEnumerable<AdvertDTO>>, Storage.AdvertsLocalStorageRepository>();
            container.Register<Copas.ApplicationServices.Storage.ILocalStorageRepository<IEnumerable<PlaceDetailsDTO>>, Storage.PlacesLocalStorageRepository>();
    
            container.Register<Copas.Phone.Navigation.IPlatformNavigationService, Navigation.PlatformAndroidNavigation>();
            container.Register<Copas.Phone.Navigation.IFullScreenVideoDispatcher, Navigation.FullScreenVideoDispatcher>(Reuse.Singleton);
            container.Register<ApplicationServices.Notifications.IShowNotification, Notifications.ShowNotificationImplementation>();*/

            DependencyContainer.Container = container;
        }
    }
}