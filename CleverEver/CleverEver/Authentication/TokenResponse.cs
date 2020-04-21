using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Authentication
{
    [DataContract]
    public class TokenResponse
    {
        [DataMember(Name = "token")]
        public string Token { get; set; }

        [IgnoreDataMember]
        public DateTime ExpirationDate { get; set; }

        public TokenResponse()
        {
        }

        public TokenResponse(String token, DateTime expirationDate)
        {
            this.Token = token;
            this.ExpirationDate = expirationDate;
        }

        [IgnoreDataMember]
        public bool IsExpired
        {
            get { return DateTime.Now > ExpirationDate; }
        }

        [IgnoreDataMember]
        public bool IsEmpty
        {
            get { return Token == null; }
        }
    }
}
