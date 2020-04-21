using CleverEver.Common.Collection;
using CleverEver.Game.Achievements;
using CleverEver.Game.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Game.Multiplayer
{
    public interface INetworkClient
    {
        IPlayer Player { get; }

        RangeObservableCollection<Participant> Players { get; }

        event EventHandler<PlayerRespondedEventArgs> PlayerResponded;

        Task SendResponse(int answerIndex);

        void Leave();
    }
}
