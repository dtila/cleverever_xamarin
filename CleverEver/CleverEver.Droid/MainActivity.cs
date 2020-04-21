using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using CleverEver.Phone;
using CleverEver.Droid.Messaging;

namespace CleverEver.Droid
{
    [Activity(Label = "CleverEver", Icon = "@drawable/icon", 
        Theme = "@style/AppTheme", MainLauncher = false, 
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, IActivity
    {
        private static System.Collections.Generic.List<WeakReference<IActivityHandler>> _activityHandlers;

        public MainActivity()
        {
            _activityHandlers = new System.Collections.Generic.List<WeakReference<IActivityHandler>>();
        }

        Android.Support.V4.App.FragmentActivity IActivity.Cast
        {
            get { return this; }
        }

        public event EventHandler Paused;
        public event EventHandler Resumed;
        public event EventHandler Started;
        public event EventHandler Stopped;

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            Acr.UserDialogs.UserDialogs.Init(this);
            global::Xamarin.Forms.Forms.Init(this, bundle);
            Storage.ParseServer.Setup().ConfigureAwait(false);

            FFImageLoading.Forms.Droid.CachedImageRenderer.Init();
            Phone.Droid.Infrastructure.Dependencies.Setup(this);

            LoadApplication(new Phone.App());
        }

        protected override void OnPause()
        {
            base.OnPause();
            //Xamarin.Facebook.AppEvents.AppEventsLogger.DeactivateApp(this);

            Paused?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnResume()
        {
            base.OnResume();
            //Xamarin.Facebook.AppEvents.AppEventsLogger.ActivateApp(this);

            Resumed?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnStart()
        {
            base.OnStart();
            Started?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnStop()
        {
            base.OnStop();
            Stopped?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _activityHandlers.Clear();
            Paused = null;
            Resumed = null;
            Started = null;
            Stopped = null;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Android.Content.Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            foreach (var handlerRef in _activityHandlers)
            {
                IActivityHandler handler;
                if (handlerRef.TryGetTarget(out handler) && handler.OnActivityResult(requestCode, resultCode, data))
                    break;
            }
        }

        internal void AddActivityHandler(IActivityHandler handler)
        {
            ((IActivity)this).AddActivityHandler(handler);
        }

        internal void RemoveActivityHandler(IActivityHandler handler)
        {
            ((IActivity)this).RemoveActivityHandler(handler);
        }


        void IActivity.AddActivityHandler(IActivityHandler handler)
        {
            _activityHandlers.Add(new WeakReference<IActivityHandler>(handler));
        }

        void IActivity.RemoveActivityHandler(IActivityHandler handler)
        {
            var removed = false;
            for (int i = _activityHandlers.Count - 1; i >= 0; i--)
            {
                IActivityHandler pinned;
                if (!_activityHandlers[i].TryGetTarget(out pinned))
                {
                    _activityHandlers.RemoveAt(i);
                    continue;
                }

                if (object.ReferenceEquals(pinned, handler))
                {
                    _activityHandlers.RemoveAt(i);
                    removed = true;
                    break;
                }
            }

            if (!removed)
            {
                // Helpers.ExceptionHandlerHelper.LogException(new InvalidOperationException(string.Format("No handler was defined for {0}", handler)));
            }
        }
    }
}

