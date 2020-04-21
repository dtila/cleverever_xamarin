using CleverEver.Common.Collection;
using CleverEver.Game;
using CleverEver.Game.Model;
using CleverEver.Game.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Game.Multiplayer
{
    public interface IRoomJoiner : INetworkClient
    {
        event EventHandler<GameStartedEventArgs> GameStarted;
        event EventHandler<NextQuestionEventArgs> QuestionChanged;
    }

    public class PlayerRespondedEventArgs : EventArgs
    {
        public Participant Participant { get; }
        public int ResponseIndex { get; }

        public PlayerRespondedEventArgs(Participant participant, int responseIndex)
        {
            Participant = participant;
            ResponseIndex = responseIndex;
        }
    }
}
