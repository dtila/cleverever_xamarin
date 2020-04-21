using CleverEver.Game;
using CleverEver.Game.Model;
using CleverEver.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Connectivity
{
    class PlayerSigninBehavior
    {
        private const string SigninRequestDateKey = "player.last_signin_canceled";
        private const string LocalStorageScoreKey = "player.score";

        private IGameServer _gameServer;
        private Task<IPlayer> _playerTask;
        private Acr.Settings.ISettings _settings;

        public PlayerSigninBehavior(IGameServer gameServer)
        {
            _gameServer = gameServer;
            _settings = Composition.DependencyContainer.Resolve<Acr.Settings.ISettings>();
            _playerTask = InternalGetCurrentPlayerAsync();
        }

        /// <summary>
        /// Get the current authenticated
        /// </summary>
        /// <returns></returns>
        public Task<IPlayer> GetCurrentPlayerAsync(bool forceSignIn = false)
        {
            if (forceSignIn)
                return InternalGetCurrentPlayerAsync(forceSignIn);
            return _playerTask;
        }

        private async Task<IPlayer> InternalGetCurrentPlayerAsync(bool forceSignIn = false)
        {
            var anonymousPlayer = new AnonymousPlayer(_settings);

#if !DEBUG
            var diff = DateTime.Now - _settings.Get<DateTime>(SigninRequestDateKey, DateTime.Now);
            if (!forceSignIn && diff.TotalDays < 2)
                return anonymousPlayer;
#endif

            try
            {
                var player = await _gameServer.GetPlayerAsync();

                if (anonymousPlayer.TotalScore != 0)
                {
                    // upgrade the score if any to the loggedin player and reset the local store score
                    player.AddScore(anonymousPlayer.TotalScore);
                    anonymousPlayer.AddScore(-anonymousPlayer.TotalScore);
                }

                return player;
            }
            catch (TaskCanceledException)
            {
                _settings.Set<DateTime>(SigninRequestDateKey, DateTime.Now);
                return anonymousPlayer;
            }
            catch (Exception ex)
            {
                Composition.DependencyContainer.Resolve<IExceptionLogger>().LogError(ex, "Unable to sign in");
                return anonymousPlayer;
            }
        }


        class AnonymousPlayer : IPlayer
        {
            private Acr.Settings.ISettings _settings;

            public string Id { get; }
            public string Name { get; }
            public bool IsAuthenticated
            {
                get { return false; }
            }

            public int TotalScore
            {
                get { return _settings.Get<int>(LocalStorageScoreKey); }
            }

            public string Email
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public PersonInfo Info
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public AnonymousPlayer(Acr.Settings.ISettings settings)
            {
                _settings = settings;
                Id = "me";
                Name = "anonymous";
            }

            public bool Equals(IPlayer other)
            {
                return other != null && string.Equals(Id, other.Id) && string.Equals(Name, other.Name);
            }

            public void AddScore(int number)
            {
                _settings.Set<int>(LocalStorageScoreKey, TotalScore + number);
            }
        }
    }
}
