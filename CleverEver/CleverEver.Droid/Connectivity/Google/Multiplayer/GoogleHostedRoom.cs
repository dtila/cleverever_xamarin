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
using Android.Gms.Games.MultiPlayer.RealTime;
using System.Threading.Tasks;
using CleverEver.Common;
using System.Collections.ObjectModel;
using CleverEver.Common.Collection;
using System.IO;
using Newtonsoft.Json;
using CleverEver.Droid.Messaging;
using CleverEver.Localization;
using CleverEver.Game.Multiplayer;
using CleverEver.Game.Model;
using CleverEver.Game;

namespace CleverEver.Droid.Connectivity.Multiplayer
{
    class GoogleHostedRoom : GoogleRoomClient, IRoomHost, IActivityHandler, IRoomUpdateListener
    {
        private const int RC_WAITING_ROOM = 10002;
        private const int RC_SELECT_PLAYERS = 10000;

        private IActivity _activity;
        private TaskCompletionSource<IRoomHost> _tcs;

        private GoogleHostedRoom(IActivity activity, GoogleApiClient client, bool invitePlayers = true)
            : base(client)
        {
            _activity = activity;
            _tcs = new TaskCompletionSource<IRoomHost>();

            activity.AddActivityHandler(this);
            activity.Stopped += Activity_Stopped;

            if (!invitePlayers)
            {
                // Auto match does not have sense for now, since we need to implement auto matching by the players wich they have selected the same game
                CreateAutoMatch();
            }
            else
            {
                Intent intent = Android.Gms.Games.GamesClass.RealTimeMultiplayer.GetSelectOpponentsIntent(Client, 1, 1);
                activity.Cast.StartActivityForResult(intent, RC_SELECT_PLAYERS);
            }
        }

        public static async Task<IRoomHost> Create(IActivity activity, GoogleApiClient client, bool invitePlayers = true)
        {
            var room = new GoogleHostedRoom(activity, client, invitePlayers);
            try
            {
                return await room._tcs.Task.ConfigureAwait(false);
            }
            catch (Exception)
            {
                room.Dispose();
                throw;
            }
        }

        public Task StartGame(QuestionSet set)
        {
            return SendMessageAsync(new GameStartedEventArgs(set.Name, set.Questions.Length, set.Questions[0]));
        }

        public Task SendQuestion(Question question)
        {
            return SendMessageAsync(new NextQuestionEventArgs(question));
        }

        public override void Leave()
        {
            if (Handle == IntPtr.Zero)
                return;

            var id = RoomId;
            if (id != null)
            {
                Android.Gms.Games.GamesClass.RealTimeMultiplayer.Leave(Client, this, id);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _activity.RemoveActivityHandler(this);
                _activity.Stopped -= Activity_Stopped;
            }

            base.Dispose(disposing);
        }

        private void CreateAutoMatch()
        {
            Bundle am = RoomConfig.CreateAutoMatchCriteria(1, 1, GetAutoMatchMask());

            // build the room config
            var roomConfigBuilder = CreateBuilder();
            roomConfigBuilder.SetAutoMatchCriteria(am);
            RoomConfig roomConfig = roomConfigBuilder.Build();

            Android.Gms.Games.GamesClass.RealTimeMultiplayer.Create(Client, roomConfig);
        }

        private RoomConfig.Builder CreateBuilder()
        {
            return RoomConfig.InvokeBuilder(this)
                .SetMessageReceivedListener(this)
                .SetRoomStatusUpdateListener(this);
        }

        private long GetAutoMatchMask()
        {
            // TODO: Implement the auto match mask
            return 0;
        }

        private void Activity_Stopped(object sender, EventArgs e)
        {
            RemoveCurrentPlayer();
        }

        bool IActivityHandler.OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == RC_SELECT_PLAYERS)
            {
                if (resultCode == Result.Canceled)
                {
                    _tcs.SetCanceled();
                    return true;
                }

                if (resultCode != Result.Ok)
                {
                    _tcs.SetException(new Exception());
                    return true;
                }

                // get the invitee list
                Bundle extras = data.Extras;
                var invitees = data.GetStringArrayListExtra(Android.Gms.Games.GamesClass.ExtraPlayerIds);

                // get auto-match criteria
                Bundle autoMatchCriteria = null;
                int minAutoMatchPlayers = data.GetIntExtra(Android.Gms.Games.MultiPlayer.Multiplayer.ExtraMinAutomatchPlayers, 0);
                int maxAutoMatchPlayers = data.GetIntExtra(Android.Gms.Games.MultiPlayer.Multiplayer.ExtraMaxAutomatchPlayers, 0);

                if (minAutoMatchPlayers > 0)
                    autoMatchCriteria = RoomConfig.CreateAutoMatchCriteria(minAutoMatchPlayers, maxAutoMatchPlayers, GetAutoMatchMask());

                var roomConfigBuilder = CreateBuilder();
                roomConfigBuilder.AddPlayersToInvite(invitees);
                if (autoMatchCriteria != null)
                    roomConfigBuilder.SetAutoMatchCriteria(autoMatchCriteria);

                RoomConfig roomConfig = roomConfigBuilder.Build();
                Android.Gms.Games.GamesClass.RealTimeMultiplayer.Create(Client, roomConfig);
                return true;
            }

            if (requestCode == RC_WAITING_ROOM)
            {
                // Waiting room was dismissed with the back button.The meaning of this
                // action is up to the game. You may choose to leave the room and cancel the
                // match, or do something else like minimize the waiting room and
                // continue to connect in the background
                if (resultCode == Result.Canceled || (int)resultCode == Android.Gms.Games.GamesActivityResultCodes.ResultLeftRoom)
                {
                    if ((int)resultCode == Android.Gms.Games.GamesActivityResultCodes.ResultLeftRoom)
                        Leave();
                    _tcs.SetCanceled();
                    return true;
                }

                if (resultCode != Result.Ok)
                {
                    _tcs.SetException(new Exception());
                    return true;
                }

                // start game
                //_tcs.SetResult(this);
                return true;
            }

            return false;
        }

        void IRoomUpdateListener.OnJoinedRoom(int statusCode, IRoom room)
        {
        }

        void IRoomUpdateListener.OnLeftRoom(int statusCode, string roomId)
        {
            Dispose();
        }

        void IRoomUpdateListener.OnRoomConnected(int statusCode, IRoom room)
        {
            //Called when all the participants in a real-time room are fully connected.
            if (statusCode != Android.Gms.Games.GamesStatusCodes.StatusOk)
            {
                _tcs.SetException(new Exception("OnRoomConnected exception"));
                return;
            }

            SetRoom(room);
            _tcs.SetResult(this);
        }

        void IRoomUpdateListener.OnRoomCreated(int statusCode, IRoom room)
        {
            if (statusCode == Android.Gms.Games.GamesStatusCodes.StatusOk)
            {
                Intent i = Android.Gms.Games.GamesClass.RealTimeMultiplayer.GetWaitingRoomIntent(Client, room, 1);
                _activity.Cast.StartActivityForResult(i, RC_WAITING_ROOM);
                return;
            }

             _tcs.SetException(new Exception(GoogleErrorHandlingHelpers.GetMultiplayerErrorMessage(statusCode)));
        }
    }
}