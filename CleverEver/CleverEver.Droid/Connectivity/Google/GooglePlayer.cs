using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Gms.Common.Apis;
using CleverEver.Game;
using System.Diagnostics;
using Android.Gms.Games.LeaderBoard;
using System.Threading.Tasks;

namespace CleverEver.Droid.Connectivity.Google
{
    [DebuggerDisplay("GooglePlayer ({Name})")]
    class GooglePlayer : IPlayer
    {
        private string _localStorageScoreKey;
        private GoogleApiClient _client;

        public string Id { get; }
        public string Name { get; }
        public bool IsAuthenticated { get { return true; } }

        private int _score;
        public int TotalScore
        {
            get { return _score; }
            private set
            {
                _score = value;
                Acr.Settings.Settings.Current.Set<int>(_localStorageScoreKey, _score);
            }
        }

        public PersonInfo Info
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public GooglePlayer(GoogleApiClient client)
        {
            _client = client;

            if (!client.IsConnected)
                throw new InvalidOperationException("The google client is not connected");

            using (var player = Android.Gms.Games.GamesClass.Players.GetCurrentPlayer(client))
            {
                Id = player.PlayerId;
                Name = player.DisplayName;
            }

            _localStorageScoreKey = "leaderboard_" + Application.Context.GetString(Resource.String.leaderboard_top_answers);
            _score = Acr.Settings.Settings.Current.Get<int>(_localStorageScoreKey);

            LoadSubmitedScoreAsync();
        }

        public bool Equals(IPlayer other)
        {
            if (other == null)
                return false;
            return string.Equals(other.Id, Id);
        }

        public void AddScore(int number)
        {
            TotalScore += number;

            var scoreLeaderboardName = Application.Context.GetString(Resource.String.leaderboard_top_answers);
            Android.Gms.Games.GamesClass.Leaderboards.SubmitScore(_client, scoreLeaderboardName, TotalScore);
        }

        private async void LoadSubmitedScoreAsync()
        {
            var scoreLeaderboardName = Application.Context.GetString(Resource.String.leaderboard_top_answers);

            var scoreResult = await Android.Gms.Games.GamesClass.Leaderboards.LoadCurrentPlayerLeaderboardScoreAsync(_client, scoreLeaderboardName, LeaderboardVariant.TimeSpanAllTime, LeaderboardVariant.CollectionPublic);

            var playServicesScore = 0;
            if (scoreResult != null && scoreResult.Score != null && scoreResult.Status.StatusCode == Android.Gms.Games.GamesStatusCodes.StatusOk)
                playServicesScore = (int)scoreResult.Score.RawScore;

            if (playServicesScore > TotalScore)
                TotalScore = playServicesScore;
        }
    }
}