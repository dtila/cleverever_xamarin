using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Authentication
{
    public interface IAuthenticationService
    {
        Task<Instalation> GetInstalationAsync();
    }

    public struct Instalation
    {
        public string Id { get; }

        public Instalation(string id)
        {
            Id = id;
        }
    }

    public struct User
    {
        public string Email { get; }
        public string Username { get; }

        public User(string username, string email)
        {
            Email = email;
            Username = username;
        }
    }
}
