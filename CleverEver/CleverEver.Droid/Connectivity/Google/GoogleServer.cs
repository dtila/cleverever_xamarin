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
using System.Threading.Tasks;
using Android.Gms.Games.MultiPlayer.RealTime;
using CleverEver.Droid.Messaging;
using Android.Gms.Games.MultiPlayer;
using CleverEver.Game.Multiplayer;
using CleverEver.Game;
using CleverEver.Game.Achievements;
using Android.Gms.Games.Quest;
using CleverEver.Droid.Connectivity.Multiplayer.Contests;
using CleverEver.Droid.Connectivity.Google.Operations;
using CleverEver.Droid.Connectivity.Multiplayer;
using CleverEver.Droid.Connectivity.Google.Multiplayer;
using Android.Gms.Common;

namespace CleverEver.Droid.Connectivity.Google
{
    //https://android-developers.googleblog.com/2016/01/play-games-permissions-are-changing-in.html
    class GoogleServer : Java.Lang.Object, IGameServer
    {
        internal static readonly Newtonsoft.Json.JsonSerializerSettings JsonSettings;
        internal static readonly Newtonsoft.Json.JsonSerializer MessageSerializer;

        private GoogleApiClient _client;
        private IActivity _activity;
        private Task _connectTask;
        private bool _autoConnectCanceled;

        public event EventHandler<GoogleClientConnectedEventArgs> ClientConnected;
        public event EventHandler ClientDisconnected;
        public event EventHandler IsConnectedChanged;

        private GoogleServerMultiplayerSupport _multiplayer;
        private GoogleServerQuestSupport _quests;
        private GoogleServerLeaderboardSupport _leaderboards;

        static GoogleServer()
        {
            JsonSettings = new Newtonsoft.Json.JsonSerializerSettings { TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All };
            MessageSerializer = Newtonsoft.Json.JsonSerializer.Create(JsonSettings);
        }

        public GoogleServer(IActivity activity)
        {
            var gameOptions = Android.Gms.Games.GamesClass.GamesOptions.InvokeBuilder()
                //.SetRequireGooglePlus(true)
                .Build();

            _activity = activity;
            _client = new GoogleApiClient.Builder(activity.Cast)
                .AddApi(Android.Gms.Games.GamesClass.API, gameOptions)
                .AddScope(Android.Gms.Games.GamesClass.ScopeGames)
                .SetGravityForPopups((int)(GravityFlags.Bottom | GravityFlags.Center))

                //.EnableAutoManage(_activity.Cast, this)
                //.EnableAutoManage(activity.Cast, this)
                .Build();

            activity.Started += Activity_Started;   
            activity.Stopped += Activity_Stopped;

            _quests = new GoogleServerQuestSupport(this);
            _multiplayer = new GoogleServerMultiplayerSupport(this);
            _leaderboards = new GoogleServerLeaderboardSupport(this);
        }

        public GoogleApiClient Client
        {
            get { return _client; }
        }

        public IActivity Activity
        {
            get { return _activity; }
        }

        public IGameServerMultiplayerSupport Multiplayer
        {
            get { return _multiplayer; }
        }

        public IGameServerQuestSupport Quests
        {
            get { return _quests; }
        }

        public IGameServerLeaderboardSupport Leaderboards
        {
            get { return _leaderboards; }
        }

        public bool IsConnected
        {
            get { return _client.IsConnected; }
        }

        public async Task<IPlayer> GetPlayerAsync()
        {
            await EnsureConnected();
            return new GooglePlayer(_client);
        }

        public IList<IGameEventHandler> GetEventHandlers()
        {
            return new[]
            {
                new FinishAnyRomanianLesson(_client)
            };
        }

        public async Task EnsureConnected()
        {
            if (_client.IsConnected)
                return;

            if (_connectTask == null)
                _connectTask = ConnectAsync();

            if (!_autoConnectCanceled)
            {
                try
                {
                    await _connectTask.ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    _autoConnectCanceled = true;
                    throw;
                }

                return;
            }

            if (!_client.IsConnecting)
            {
                _connectTask = ConnectAsync();
                await _connectTask.ConfigureAwait(false);
            }
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _activity.Started -= this.Activity_Started;
                _activity.Stopped -= this.Activity_Stopped;

                _multiplayer.Dispose();
                _quests.Dispose();

                if (_client != null && _client.Handle != IntPtr.Zero)
                {
                    _client.Disconnect();
                    _client.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        private async Task ConnectAsync()
        {
            using (var operation = new ConnectOperation(_activity, _client))
            {
                var invitation = await operation.Task;

                var args = new GoogleClientConnectedEventArgs(invitation);
                ClientConnected?.Invoke(this, args);
                IsConnectedChanged?.Invoke(this, args);
            }
        }

        private void Activity_Stopped(object sender, EventArgs e)
        {
            var client = Client;
            if (client != null && client.Handle != IntPtr.Zero && client.IsConnected)
            {
                ClientDisconnected?.Invoke(this, e);
            }

            _client.Disconnect();

            IsConnectedChanged?.Invoke(this, e);
        }

        private void Activity_Started(object sender, EventArgs e)
        {
            /*var op = Android.Gms.Auth.Api.Auth.GoogleSignInApi.SilentSignIn(_client);
            if (op.IsDone)
            {
                //op.AsAsync<Android.Gms.Auth.Api.SignIn.GoogleSignInResult>()
            }*/

            if (!_client.IsConnecting && _connectTask != null)
            {
                _connectTask = ConnectAsync();
            }
        }


        public class GoogleClientConnectedEventArgs : EventArgs
        {
            public Android.Gms.Games.MultiPlayer.IInvitation GameInvitation { get; }

            public GoogleClientConnectedEventArgs(Android.Gms.Games.MultiPlayer.IInvitation invitation)
            {
                GameInvitation = invitation;
            }
        }
    }
}