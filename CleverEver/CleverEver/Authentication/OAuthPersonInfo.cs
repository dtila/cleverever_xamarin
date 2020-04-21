using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Authentication
{
    public class OAuthPersonInfo
    {
        public string ProviderName { get; }
        public String ProviderUserId { get; }
        public PersonInfo PersonInfo { get; }

        public OAuthPersonInfo(string providerName, String providerUserId, PersonInfo personInfo)
        {
            ProviderName = providerName;
            ProviderUserId = providerUserId;
            PersonInfo = personInfo;
        }
    }

    public class PersonInfo
    {
        public string FamilyName { get; }
        public string Name { get; }
        public string MiddleName { get; }
        public string Email { get; }
        public string CoverImageUrl { get; }
        public string Genre { get; }
        public string Birthday { get; }

        public PersonInfo(string familyName, string name, string middleName, string email, string genre)
        {
            FamilyName = familyName;
            Name = name;
            MiddleName = middleName;
            Email = email;
            Genre = genre;
        }
    }
}
