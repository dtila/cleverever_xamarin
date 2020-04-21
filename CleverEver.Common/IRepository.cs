using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver
{
    public interface IRepository<T>
    {
        IReadOnlyList<T> Find(int id);
        void Set(T set);
    }
}
