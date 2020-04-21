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
using Android.Gms.Common.Apis;
using System.Threading.Tasks;
using CleverEver.Droid.Messaging;
using Android.Gms.Common;
using CleverEver.Authentication;

namespace CleverEver.Droid.Connectivity.Google.Operations
{
    sealed class ConnectOperation : Java.Lang.Object, GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener, IActivityHandler
    {
        private const int RC_SIGN_IN = 9001;

        private IActivity _activity;
        private GoogleApiClient _client;
        private bool mResolvingConnectionFailure = false;
        private bool mAutoStartSignInflow = true;
        private bool mSignInClicked = false;

        private TaskCompletionSource<Android.Gms.Games.MultiPlayer.IInvitation> _tcs;

        public ConnectOperation(IActivity context, GoogleApiClient client)
        {
            _activity = context;
            _tcs = new TaskCompletionSource<Android.Gms.Games.MultiPlayer.IInvitation>();
            _client = client;

            client.RegisterConnectionCallbacks(this);
            client.RegisterConnectionFailedListener(this);
            _activity.AddActivityHandler(this);

            Connect();
        }

        public Task<Android.Gms.Games.MultiPlayer.IInvitation> Task
        {
            get { return _tcs.Task; }
        }

        void GoogleApiClient.IConnectionCallbacks.OnConnected(Bundle connectionHint)
        {
            try
            {
                /*var loggedAccountName = Android.Gms.Plus.PlusClass.AccountApi.GetAccountName(_client);

                var x = Android.Gms.Auth.Api.Auth.GoogleSignInApi.SilentSignIn(null);

                var x = Android.Gms.Auth.Api.Auth.GoogleSignInApi.SilentSignIn(null);*/

                Android.Gms.Games.MultiPlayer.InvitationEntity invitation = null;
                if (connectionHint != null)
                {
                    invitation = connectionHint.GetParcelable(Android.Gms.Games.MultiPlayer.Multiplayer.ExtraInvitation).JavaCast<Android.Gms.Games.MultiPlayer.InvitationEntity>();
                }

                _tcs.SetResult(invitation);
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
            /*var notInstalledCodes = new[] { ConnectionResult.ServiceInvalid, ConnectionResult.ServiceMissing, ConnectionResult.ServiceUpdating };
            if (!result.HasResolution && notInstalledCodes.Contains(result.ErrorCode))
            {
                _tcs.SetException(new Exception(Localization.UserMessages.));
                return;
            }*/
            //if (!result.HasResolution)
            //{
            //    GoogleApiAvailability.Instance.GetErrorDialog(_activity, result.ErrorCode, 0).Show();
            //    _tcs.SetException(new AuthenticationException(result.ErrorMessage ?? "Unable to login to Google"));
            //    return;
            //}

            if (mResolvingConnectionFailure)
            {
                // Already resolving
                return;
            }

            if (mSignInClicked || mAutoStartSignInflow)
            {
                mAutoStartSignInflow = false;
                mSignInClicked = false;
                mResolvingConnectionFailure = true;

                // Attempt to resolve the connection failure using BaseGameUtils.
                // The R.string.signin_other_error value should reference a generic
                // error string in your strings.xml file, such as "There was
                // an issue with sign in, please try again later."
                if (!GoogleErrorHandlingHelpers.ResolveConnectionFailure(_activity.Cast, _client, result, RC_SIGN_IN, Localization.UserMessages.signin_other_error))
                {
                    mResolvingConnectionFailure = false;
                    // We display a modal error ... so we cancel the task ... this is hacky but ...
                    _tcs.SetCanceled();
                }
            }
        }


        bool IActivityHandler.OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == RC_SIGN_IN)
            {
                mSignInClicked = false;
                mResolvingConnectionFailure = false;

                switch (resultCode)
                {
                    case Result.Canceled:
                        _tcs.SetCanceled();
                        break;
                    case Result.Ok:
                        Connect();
                        break;
                    default:
                        {
                            var error = GoogleErrorHandlingHelpers.FindErrorMessage(requestCode, (int)resultCode) ?? Localization.UserMessages.signin_other_error;
                            _tcs.SetException(new Exception(error));
                        }
                        break;
                }
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
    }
}