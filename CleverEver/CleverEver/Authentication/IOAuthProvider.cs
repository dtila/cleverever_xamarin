using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Authentication
{
    public interface IOAuthProvider : IDisposable
    {
        string Name { get; }

        Task<OAuthConnectedResult> ConnectAsyncTask();

        Task<OAuthPersonInfo> GetUserInfoAsyncTask();
    }

    public class OAuthConnectedResult
    {
        public string ProviderName { get; }
        public string ProviderUserId { get; }
        public string AccessToken { get; }

        public OAuthConnectedResult(string provider, string providerUserId, string accessToken)
        {
            this.ProviderName = provider;
            this.ProviderUserId = providerUserId;
            this.AccessToken = accessToken;
        }
    }

}
