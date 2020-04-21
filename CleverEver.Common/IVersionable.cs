using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver
{
    public interface IVersionable<T>
    {
        int Version { get; }
        bool HasNewVersion { get; }

        T CurrentItem { get; }
        void Refresh();
    }
}
