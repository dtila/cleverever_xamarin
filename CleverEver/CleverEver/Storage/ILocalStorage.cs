using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Storage
{
    public interface ILocalStorage<T>
    {
        T Find();
        void Set(T data);
    }
}
