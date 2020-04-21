using CleverEver.Game;
using CleverEver.Pages.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Analytics
{
    class NetworkGameMonitoring : IGameMonitoring
    {
        private INetworkGame _game;
        private UserSelection _userSelection;

        public NetworkGameMonitoring(INetworkGame networkGame)
        {
            _game = networkGame;
            _userSelection = null;
        }

        public NetworkGameMonitoring(INetworkGame networkGame, UserSelection userSelection)
        {
            _game = networkGame;
            _userSelection = userSelection;
        }
    }
}
