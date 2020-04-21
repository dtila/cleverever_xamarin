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
using CleverEver.Game.Multiplayer;
using CleverEver.Droid.Connectivity.Multiplayer;

namespace CleverEver.Droid.Connectivity.Google.Multiplayer
{
    class GoogleServerMultiplayerSupport : Java.Lang.Object, IGameServerMultiplayerSupport, Android.Gms.Games.MultiPlayer.IOnInvitationReceivedListener
    {
        private GoogleServer _gameServer;

        public event EventHandler<InvitationReceivedEventArgs> InvitationReceived;

        public GoogleServerMultiplayerSupport(GoogleServer gameServer)
        {
            _gameServer = gameServer;

            _gameServer.ClientConnected += GameServer_ClientConnected;
            _gameServer.ClientDisconnected += GameServer_ClientDisconnected;
        }

        public async Task<IRoomHost> CreateRoomAsync()
        {
            await _gameServer.EnsureConnected();
            _gameServer.Activity.Cast.Window.AddFlags(WindowManagerFlags.KeepScreenOn);

            try
            {
                return await GoogleHostedRoom.Create(_gameServer.Activity, _gameServer.Client);
            }
            finally
            {
                _gameServer.Activity.Cast.Window.ClearFlags(WindowManagerFlags.KeepScreenOn);
            }
        }

        public async Task<IRoomJoiner> JoinRoomAsync()
        {
            await _gameServer.EnsureConnected();
            _gameServer.Activity.Cast.Window.AddFlags(WindowManagerFlags.KeepScreenOn);

            try
            {
                return await GoogleJoinedRoom.Create(_gameServer.Activity, _gameServer.Client, null);
            }
            finally
            {
                _gameServer.Activity.Cast.Window.ClearFlags(WindowManagerFlags.KeepScreenOn);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                var client = _gameServer.Client;
                if (client != null && client.Handle != IntPtr.Zero)
                {
                    Android.Gms.Games.GamesClass.Invitations.UnregisterInvitationListener(client);
                }

                _gameServer.ClientConnected -= GameServer_ClientConnected;
                _gameServer.ClientDisconnected -= GameServer_ClientDisconnected;
            }

            base.Dispose(disposing);
        }

        private void GameServer_ClientConnected(object sender, GoogleServer.GoogleClientConnectedEventArgs e)
        {
            if (e.GameInvitation != null)
            {
                AcceptInvitation(e.GameInvitation);
            }

            Android.Gms.Games.GamesClass.Invitations.RegisterInvitationListener(_gameServer.Client, this);
        }

        private void GameServer_ClientDisconnected(object sender, EventArgs e)
        {
            Android.Gms.Games.GamesClass.Invitations.UnregisterInvitationListener(_gameServer.Client);
        }

        private void AcceptInvitation(Android.Gms.Games.MultiPlayer.IInvitation invitation)
        {
            var gameInvitation = new GameInvitation(_gameServer, invitation);
            var invitationArgs = new Game.InvitationReceivedEventArgs(invitation.Inviter.DisplayName, gameInvitation);
            InvitationReceived?.Invoke(this, invitationArgs);
        }


        void Android.Gms.Games.MultiPlayer.IOnInvitationReceivedListener.OnInvitationReceived(Android.Gms.Games.MultiPlayer.IInvitation invitation)
        {
            AcceptInvitation(invitation);
        }

        void Android.Gms.Games.MultiPlayer.IOnInvitationReceivedListener.OnInvitationRemoved(string invitationId)
        {
        }



        class GameInvitation : Game.IInvitation
        {
            private GoogleServer _gameServer;
            private Android.Gms.Games.MultiPlayer.IInvitation _invitation;

            public GameInvitation(GoogleServer gameServer, Android.Gms.Games.MultiPlayer.IInvitation invitation)
            {
                _gameServer = gameServer;
                _invitation = invitation;
            }

            public Task<IRoomJoiner> Accept()
            {
                return GoogleJoinedRoom.Create(_gameServer.Activity, _gameServer.Client, _invitation);
            }

            public void Decline()
            {
                Android.Gms.Games.GamesClass.RealTimeMultiplayer.DeclineInvitation(_gameServer.Client, _invitation.InvitationId);
            }
        }
    }
}