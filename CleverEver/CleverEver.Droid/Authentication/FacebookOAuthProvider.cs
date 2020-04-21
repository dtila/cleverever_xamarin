using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Copas.Phone.Droid.Serialization;
using Copas.Phone.Droid.Messages;

namespace CleverEver.Phone.Droid.Authentication
{
    class FacebookOAuthProvider : IOAuthProvider
    {
        public const string ProviderName = "Facebook";

        private MainActivity _activity;

        public FacebookOAuthProvider(MainActivity activity)
        {
            _activity = activity;
        }

        public string Name { get { return ProviderName; } }

        public async Task<OAuthConnectedResult> ConnectAsyncTask()
        {
            using (var operation = new ConnectOperation(this))
                return await operation.Task;
        }

        public async Task<OAuthPersonInfo> GetUserInfoAsyncTask()
        {
            Xamarin.Facebook.Profile profile = Profile.CurrentProfile;
            if (profile == null)
            {
                throw new InvalidOperationException("Unable to get the user info for a user which is not logged in");
                //LoginManager.Instance.LogInWithReadPermissions(_activity, new[] { "public_profile", "email" });
            }

            using (var operation = new UserInfoOperation())
                return await operation.Task;
        }


        class ConnectOperation : Java.Lang.Object, IFacebookCallback, IActivityHandler
        {
            private TaskCompletionSource<OAuthConnectedResult> _tcs;
            private FacebookOAuthProvider _owner;
            private ICallbackManager _callbackManager;

            public ConnectOperation(FacebookOAuthProvider owner)
            {
                _tcs = new TaskCompletionSource<OAuthConnectedResult>();
                _owner = owner;

                _callbackManager = CallbackManagerFactory.Create();
                var loginManager = LoginManager.Instance;
                loginManager.RegisterCallback(_callbackManager, this);
                loginManager.LogInWithReadPermissions(owner._activity, new[] { "public_profile", "email" });

                owner._activity.AddActivityHandler(this);
            }

            public Task<OAuthConnectedResult> Task
            {
                get { return _tcs.Task; }
            }

            public void OnCancel()
            {
                _tcs.TrySetCanceled();
            }

            public void OnError(FacebookException error)
            {
                _tcs.TrySetException(error);
            }

            public void OnSuccess(Java.Lang.Object p0)
            {
                var result = p0.JavaCast<LoginResult>();
                _tcs.SetResult(new OAuthConnectedResult(ProviderName, result.AccessToken.UserId, result.AccessToken.Token));
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _callbackManager.Dispose();
                    _owner._activity.RemoveActivityHandler(this);
                }

                base.Dispose(disposing);
            }

            bool IActivityHandler.OnActivityResult(int requestCode, Result resultCode, Intent data)
            {
                return _callbackManager.OnActivityResult(requestCode, (int)resultCode, data);
            }

            void IActivityHandler.OnPause()
            {
            }

            void IActivityHandler.OnResume()
            {
            }
        }

        class UserInfoOperation : Java.Lang.Object, GraphRequest.ICallback
        {
            private TaskCompletionSource<OAuthPersonInfo> _tcs;

            public UserInfoOperation()
            {
                _tcs = new TaskCompletionSource<OAuthPersonInfo>();
                GetGraphRequest().ExecuteAsync();
            }

            public Task<OAuthPersonInfo> Task
            {
                get { return _tcs.Task; }
            }

            public void OnCompleted(GraphResponse graphResponse)
            {
                try
                {
                    Profile profile = Profile.CurrentProfile;

                    string email = null, genre = null;
                    var jsonObject = graphResponse.JSONObject;
                    if (jsonObject != null)
                    {
                        try
                        {
                            email = JsonSerializationHelpers.GetString(jsonObject, "email");
                            genre = JsonSerializationHelpers.GetString(jsonObject, "genre");
                        }
                        catch (Org.Json.JSONException e)
                        {
                            e.PrintStackTrace();
                        }
                    }

                    var personInfo = new PersonInfo(profile.FirstName, profile.LastName, profile.MiddleName, email, genre);
                    _tcs.SetResult(new OAuthPersonInfo(ProviderName, profile.Id, personInfo));
                }
                catch (System.Exception ex)
                {
                    _tcs.SetException(ex);
                }
            }

            private GraphRequest GetGraphRequest()
            {
                Bundle parameters = new Bundle();
                parameters.PutString("fields", "id,link,gender,email");
                return new GraphRequest(AccessToken.CurrentAccessToken, "me", parameters, HttpMethod.Get, this);
            }
        }

        public void Dispose()
        {
        }
    }
}