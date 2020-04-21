using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Localization
{
    public interface ILocalization
    {
        CultureInfo CurrentCultureInfo { get; }
    }
}
