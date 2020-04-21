using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Pages.Game.Templates
{
    public interface IPurchasedItem
    {
        string Icon { get; }
        string Name { get; }

        bool IsLocked { get; }
        string Description { get; }
    }
}
