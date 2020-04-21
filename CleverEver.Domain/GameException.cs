using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Game
{
    public class GameException : Exception
    {
        public GameException(string message, Exception inner = null)
            : base(message, inner)
        {
        }
    }

    public class HostedGameException : GameException
    {
        public HostedGameException(string message, Exception inner = null)
            : base(message, inner)
        {
        }
    }
}
