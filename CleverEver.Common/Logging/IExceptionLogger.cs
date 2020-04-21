using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Logging
{
    public interface IExceptionLogger
    {
        void LogError(Exception ex, string message = null, [CallerMemberName] string method = null);
    }
}
