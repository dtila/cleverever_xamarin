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
using Parse;
using System.Threading.Tasks;
using CleverEver.Authentication;

namespace CleverEver.Droid.Authentication
{
    class ParseAuthentication : IAuthenticationService
    {
        private static Task<Instalation> GetInstallationTask = Task.Run(GetInstallationInternalAsync);

        public bool IsAuthenticated
        {
            get { return ParseUser.CurrentUser != null; }
        }

        public Task<Instalation> GetInstalationAsync()
        {
            return GetInstallationTask;
        }

        private static async Task<Instalation> GetInstallationInternalAsync()
        {
            var instalation = ParseInstallation.CurrentInstallation;
            if (instalation == null)
                throw new InvalidOperationException("Parse instalation is null");

            await instalation.SaveAsync();
            return Transform(instalation);
        }

        private static User Transform(ParseUser parseUser)
        {
            if (parseUser == null)
                throw new ArgumentNullException(nameof(parseUser));

            return new User(parseUser.Username, parseUser.Email);
        }

        private static Instalation Transform(ParseInstallation instalation)
        {
            return new Instalation(instalation.InstallationId.ToString());
        }
    }
}