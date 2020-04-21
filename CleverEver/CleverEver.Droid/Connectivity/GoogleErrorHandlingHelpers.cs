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
using Android.Gms.Common;
using CleverEver.Localization;

namespace CleverEver.Droid.Connectivity
{
    class GoogleErrorHandlingHelpers
    {
        public static bool ResolveConnectionFailure(Activity activity, GoogleApiClient client, ConnectionResult result, int requestCode, string fallbackErrorMessage)
        {
            if (result.HasResolution)
            {
                try
                {
                    result.StartResolutionForResult(activity, requestCode);
                    return true;
                }
                catch (IntentSender.SendIntentException)
                {
                    // The intent was canceled before it was sent.  Return to the default
                    // state and attempt to connect to get an updated ConnectionResult.
                    client.Connect();
                    return false;
                }
            }
            else
            {
                // not resolvable... so show an error message
                int errorCode = result.ErrorCode;
                Dialog dialog = GoogleApiAvailability.Instance.GetErrorDialog(activity, errorCode, requestCode);
                if (dialog != null)
                {
                    dialog.Show();
                }
                else
                {
                    // no built-in dialog: show the fallback error message
                    MakeSimpleDialog(activity, fallbackErrorMessage).Show();
                }
                return false;
            }
        }


        public static AlertDialog MakeSimpleDialog(Activity activity, String message)
        {
            return (new AlertDialog.Builder(activity))
                .SetMessage(message)
                .SetNeutralButton(Android.Resource.String.Ok, listener: null)
                .Create();
        }


        internal static string FindErrorMessage(int requestCode, int responseCode)
        {
            switch (responseCode)
            {
                case Android.Gms.Games.GamesActivityResultCodes.ResultAppMisconfigured:
                    return Localization.UserMessages.app_misconfigured;
                case Android.Gms.Games.GamesActivityResultCodes.ResultSignInFailed:
                    return Localization.UserMessages.sign_in_failed;
                case Android.Gms.Games.GamesActivityResultCodes.ResultLicenseFailed:
                    return Localization.UserMessages.license_failed;

                default:
                    // No meaningful Activity response code, so generate default Google
                    // Play services dialog
                    return null;
            }
        }

        internal static void ShowActivityResultError(Activity activity, int requestCode, int responseCode, string errorDescription)
        {
            string error = FindErrorMessage(requestCode, responseCode) ?? errorDescription;

            Dialog errorDialog = MakeSimpleDialog(activity, error);
            errorDialog.Show();
        }

        public static string GetMultiplayerErrorMessage(int statusCode)
        {
            switch (statusCode)
            {
                case Android.Gms.Games.GamesStatusCodes.StatusClientReconnectRequired:
                    return UserMessages.connection_required;
                case Android.Gms.Games.GamesStatusCodes.StatusRealTimeConnectionFailed:
                    return UserMessages.unable_to_connection;
                case Android.Gms.Games.GamesStatusCodes.StatusMultiplayerDisabled:
                    return UserMessages.multiplayer_disabled;
                default:
                    return UserMessages.internal_error;
            }
        }
    }
}