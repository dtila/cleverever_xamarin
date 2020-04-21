using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Pages.Game.Templates
{
    public interface IScoredItem
    {
        int Rank { get; }
        string Image { get; }
        string Name { get; }
        decimal Score { get; }
    }
}
