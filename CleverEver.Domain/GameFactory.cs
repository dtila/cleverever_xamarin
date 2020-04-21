using CleverEver.Game.Model;
using CleverEver.Game.Multiplayer;
using CleverEver.Game.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Game
{
    public static class GameFactory
    {
        public static IGame CreateSingleGame(QuestionSet set, IPlayer player)
        {
            return new SinglePlayerGame(set, player);
        }

        public static INetworkGame CreateInvitedGame(IRoomJoiner joined)
        {
            return new JoinedGame(joined);
        }

        public static INetworkGame CreateHostGame(QuestionSet set, IRoomHost host)
        {
            return new HostedGame(set, host);
        }
    }
}
