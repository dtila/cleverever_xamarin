using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Game
{
    public interface INetworkGame : IGame
    {
        NetworkType Type { get; }
    }

    public enum NetworkType
    {
        Hosted,
        Joined
    }
}
