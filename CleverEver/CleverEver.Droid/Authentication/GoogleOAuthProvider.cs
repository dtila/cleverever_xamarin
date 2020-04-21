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
using Android.Gms.Common.Apis;
using Android.Gms.Plus;
using Android.Gms.Common;
using CleverEver.Authentication;
using CleverEver.Droid;
using CleverEver.Droid.Messaging;

namespace CleverEver.Phone.Droid.Authentication
{
    sealed class GoogleOAuthProvider : IOAuthProvider
    {
        public const string ProviderName = "Google";

        private const int PLAY_SERVICES_RESOLUTION_REQUEST = 9000;
        private const int REQ_SIGN_IN_REQUIRED = 55664;

        private GoogleApiClient _client;
        private Task<OAuthPersonInfo> _userInfoResult;
        private MainActivity _activity;

        public GoogleOAuthProvider(MainActivity context, GoogleApiClient client)
        {
            _activity = context;
            _client = client;
        }

        public GoogleOAuthProvider(MainActivity context)
        {
            _activity = context;
            _client = new GoogleApiClient.Builder(context)
                   .AddApi(PlusClass.API)
                   .AddScope(PlusClass.ScopePlusLogin)
                   .AddScope(PlusClass.ScopePlusProfile)
                   .Build();
        }

        public string Name { get { return ProviderName; } }

        public async Task<OAuthConnectedResult> ConnectAsyncTask()
        {
            using (var operation = new ConnectOperation(_activity, _client))
                return await operation.Task;
        }

        public Task<OAuthPersonInfo> GetUserInfoAsyncTask()
        {
            var person = PlusClass.PeopleApi.GetCurrentPerson(_client);
            if (person == null)
                throw new InvalidOperationException("Unable to get the user info because no connection has been made");

            if (_userInfoResult == null)
            {
                var loggedAccountName = PlusClass.AccountApi.GetAccountName(_client);
                var personInfo = new PersonInfo(person.Name.FamilyName, person.Name.GivenName, person.Name.MiddleName, loggedAccountName, GetGender(person));
                _userInfoResult = Task.FromResult(new OAuthPersonInfo(ProviderName, loggedAccountName, personInfo));
            }

            return _userInfoResult;
        }

        private static string GetGender(Android.Gms.Plus.Model.People.IPerson person)
        {
            if (!person.HasGender)
                return null;
            if (person.Gender == 1)
                return "F";
            if (person.Gender == 0)
                return "M";
            return "?";
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        sealed class ConnectOperation : Java.Lang.Object, GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener, IActivityHandler
        {
            private MainActivity _activity;
            private GoogleApiClient _client;
            private bool _isResolving, _shouldResolve;
            private TaskCompletionSource<OAuthConnectedResult> _tcs;

            public ConnectOperation(MainActivity context, GoogleApiClient client)
            {
                _activity = context;
                _tcs = new TaskCompletionSource<OAuthConnectedResult>();
                _client = client;
                _shouldResolve = true;

                client.RegisterConnectionCallbacks(this);
                client.RegisterConnectionFailedListener(this);
                _activity.AddActivityHandler(this);

                Connect();
            }

            public Task<OAuthConnectedResult> Task
            {
                get { return _tcs.Task; }
            }

            async void GoogleApiClient.IConnectionCallbacks.OnConnected(Bundle connectionHint)
            {
                try
                {
                    var loggedAccountName = PlusClass.AccountApi.GetAccountName(_client);
                    var token = await GoogleAuthenticationHelper.GetTokenAsync();
                    _tcs.SetResult(new OAuthConnectedResult(ProviderName, loggedAccountName, token.Token));
                }
                catch (Exception ex)
                {
                    _tcs.SetException(ex);
                }
            }
            void GoogleApiClient.IConnectionCallbacks.OnConnectionSuspended(int cause)
            {
            }

            void GoogleApiClient.IOnConnectionFailedListener.OnConnectionFailed(ConnectionResult result)
            {
                if (!_isResolving && _shouldResolve)
                {
                    if (!result.HasResolution)
                    {
                        GoogleApiAvailability.Instance.GetErrorDialog(_activity, result.ErrorCode, 0).Show();
                        _tcs.SetException(new AuthenticationException(result.ErrorMessage ?? "Unable to login to Google"));
                        return;
                    }

                    try
                    {
                        result.StartResolutionForResult(_activity, REQ_SIGN_IN_REQUIRED);
                        _isResolving = true;
                    }
                    catch (IntentSender.SendIntentException e)
                    {
                        Android.Util.Log.Error("TAG", "Could not resolve ConnectionResult.", e);
                        _isResolving = false;
                        Connect();
                    }
                }
                else
                {
                    UpdateUI(false);
                }
            }


            bool IActivityHandler.OnActivityResult(int requestCode, Result resultCode, Intent data)
            {
                if (requestCode == REQ_SIGN_IN_REQUIRED)
                {
                    if (resultCode == Result.Canceled)
                    {
                        _tcs.SetCanceled();
                        return true;
                    }

                    if (resultCode != Result.Ok)
                    {
                        // If we've got an error we can't resolve, we're no longer in the midst of signing
                        // in, so we can stop the progress spinner.
                        _shouldResolve = false;
                    }

                    _isResolving = false;
                    Connect();
                    return true;
                }
                return false;
            }

            private void Connect()
            {
                if (!_client.IsConnected && !_client.IsConnecting)
                    _client.Connect();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _client.UnregisterConnectionCallbacks(this);
                    _client.UnregisterConnectionFailedListener(this);
                    _activity.RemoveActivityHandler(this);
                }

                base.Dispose(disposing);
            }

            void ShowErrorDialog(ConnectionResult connectionResult)
            {
                int errorCode = connectionResult.ErrorCode;

                if (GooglePlayServicesUtil.IsUserRecoverableError(errorCode))
                {
                    _shouldResolve = false;
                    UpdateUI(false);

                    /*var listener = new DialogInterfaceOnCancelListener();
                    listener.OnCancelImpl = (dialog) => {
                        mShouldResolve = false;
                        UpdateUI(false);
                    };
                    GooglePlayServicesUtil.GetErrorDialog(errorCode, this, REQ_SIGN_IN_REQUIRED, listener).Show();*/
                }
                else
                {
                    //var errorstring = string.Format(_activity.GetString(Resource.String.play_services_error_fmt), errorCode);
                    //Toast.MakeText(_activity, errorstring, ToastLength.Short).Show();

                    _shouldResolve = false;
                    UpdateUI(false);
                }
            }

            void UpdateUI(bool isSignedIn)
            {

            }
        }
    }
}