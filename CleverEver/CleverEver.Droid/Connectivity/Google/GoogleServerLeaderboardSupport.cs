using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CleverEver.Game;
using CleverEver.Game.Achievements;

namespace CleverEver.Droid.Connectivity.Google
{
    class GoogleServerLeaderboardSupport : Game.IGameServerLeaderboardSupport
    {
        private GoogleServer _gameServer;

        public GoogleServerLeaderboardSupport(GoogleServer gameServer)
        {
            _gameServer = gameServer;
        }

        public async Task ShowTopLeaderboardAsync()
        {
            await _gameServer.EnsureConnected();
            var scoreLeaderboardName = Application.Context.GetString(Resource.String.leaderboard_top_answers);

            var intent = Android.Gms.Games.GamesClass.Leaderboards.GetLeaderboardIntent(_gameServer.Client, scoreLeaderboardName);
            _gameServer.Activity.Cast.StartActivityForResult(intent, 9009);
        }
    }
}