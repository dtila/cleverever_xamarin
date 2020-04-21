using CleverEver.Game.Achievements;
using CleverEver.Game.Multiplayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Game
{
    public interface IGameServer : IDisposable
    {
        event EventHandler IsConnectedChanged;
        bool IsConnected { get; }

        IGameServerMultiplayerSupport Multiplayer { get; }
        IGameServerQuestSupport Quests { get; }
        IGameServerLeaderboardSupport Leaderboards { get; }

        Task<IPlayer> GetPlayerAsync();
        IList<IGameEventHandler> GetEventHandlers();
    }
}
