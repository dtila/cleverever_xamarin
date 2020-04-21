using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Platform
{
    public interface IApplicationInfo
    {
        int AppVersion { get; }
        string AppIdentifier { get; }
    }
}
