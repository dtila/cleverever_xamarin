using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Authentication
{
    public class AuthenticationException : Exception
    {
        public AuthenticationException(string message, Exception inner = null)
            : base(message, inner)
        {
        }
    }
}
