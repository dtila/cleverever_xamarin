using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Localization
{
    class Localization
    {
        public static CultureInfo Culture
        {
            get { return UserMessages.Culture; }
        }

        public static void SetCulture(CultureInfo culture)
        {
            UserMessages.Culture = culture;
            Actions.Culture = culture;
        }
    }
}
