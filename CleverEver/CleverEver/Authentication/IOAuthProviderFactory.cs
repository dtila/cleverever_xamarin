using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Authentication
{
    public interface IOAuthProviderFactory
    {
        bool IsAvailable { get; }
        IOAuthProvider Create();
    }
}
