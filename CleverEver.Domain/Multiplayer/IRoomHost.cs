using CleverEver.Common.Collection;
using CleverEver.Game.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Game.Multiplayer
{

    public interface IRoomHost : INetworkClient
    {
        Task StartGame(QuestionSet set);
        Task SendQuestion(Question question);
    }
}
