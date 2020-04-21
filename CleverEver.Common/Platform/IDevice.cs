using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Platform
{
    public interface IDevice
    {
        string Platform { get; }
        string PlatformVersion { get; }

        string Display { get; }
        string Manufacturer { get; }
        string Model { get; }

        void DebugWrite(string content);
        void Dial(string phoneNumber);
    }
}
