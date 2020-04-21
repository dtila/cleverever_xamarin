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
using Android.Gms.Common.Apis;
using Android.Gms.Games.MultiPlayer.RealTime;
using System.IO;
using Android.Gms.Games.MultiPlayer;
using CleverEver.Droid.Messaging;
using System.Threading;
using CleverEver.Game;
using CleverEver.Game.Multiplayer;

namespace CleverEver.Droid.Connectivity.Multiplayer
{
    class GoogleJoinedRoom : GoogleRoomClient, IRoomJoiner, IActivityHandler, IRoomUpdateListener
    {
        private const int RC_WAITING_ROOM = 10002;
        private const int RC_INVITATION_INBOX = 10001;

        private IActivity _activity;
        private TaskCompletionSource<IRoomJoiner> _tcs;

        public event EventHandler<GameStartedEventArgs> GameStarted;
        public event EventHandler<NextQuestionEventArgs> QuestionChanged;

        private GoogleJoinedRoom(IActivity activity, GoogleApiClient client, Android.Gms.Games.MultiPlayer.IInvitation invitation)
            : base(client)
        {
            _activity = activity;
            _tcs = new TaskCompletionSource<IRoomJoiner>();

            activity.AddActivityHandler(this);
            activity.Stopped += Activity_Stopped;

            if (invitation != null)
            {
                AcceptInvitation(invitation);
                return;
            }

            var intent = Android.Gms.Games.GamesClass.Invitations.GetInvitationInboxIntent(client);
            activity.Cast.StartActivityForResult(intent, RC_INVITATION_INBOX);
        }

        public static async Task<IRoomJoiner> Create(IActivity activity, GoogleApiClient client, Android.Gms.Games.MultiPlayer.IInvitation invitation)
        {
            var room = new GoogleJoinedRoom(activity, client, invitation);
            try
            {
                // if we do not have any exception, we keep the 
                return await room._tcs.Task.ConfigureAwait(false);
            }
            catch (Exception)
            {
                // if we got an exception, we dispose the room, and we throw the exception further
                room.Dispose();
                throw;
            }
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

        protected override bool TryDispatchMessage(object message)
        {
            var gameStarted = message as GameStartedEventArgs;
            if (gameStarted != null)
            {
                GameStarted?.Invoke(this, gameStarted);
                return true;
            }

            var nextQuestion = message as NextQuestionEventArgs;
            if (nextQuestion != null)
            {
                QuestionChanged?.Invoke(this, nextQuestion);
                return true;
            }

            return base.TryDispatchMessage(message);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _activity.RemoveActivityHandler(this);
                _activity.Stopped -= Activity_Stopped;
            }
        }

        private RoomConfig.Builder CreateBuilder()
        {
            return RoomConfig.InvokeBuilder(this)
                .SetMessageReceivedListener(this)
                .SetRoomStatusUpdateListener(this);
        }

        private void AcceptInvitation(Android.Gms.Games.MultiPlayer.IInvitation invitation)
        {
            if (invitation == null)
                throw new InvalidOperationException("Invitation is null");

            var builder = CreateBuilder();
            builder.SetInvitationIdToAccept(invitation.InvitationId);
            Android.Gms.Games.GamesClass.RealTimeMultiplayer.Join(Client, builder.Build());
        }

        private void Activity_Stopped(object sender, EventArgs e)
        {
            RemoveCurrentPlayer();
        }

        bool IActivityHandler.OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
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

                return true;
            }

            if (requestCode == RC_INVITATION_INBOX)
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

                Bundle extras = data.Extras;
                var invitation = extras.GetParcelable(Android.Gms.Games.MultiPlayer.Multiplayer.ExtraInvitation).JavaCast<Android.Gms.Games.MultiPlayer.InvitationEntity>();
                AcceptInvitation(invitation);
                return true;
            }

            return false;
        }

        void IRoomUpdateListener.OnJoinedRoom(int statusCode, IRoom room)
        {
            if (statusCode != Android.Gms.Games.GamesStatusCodes.StatusOk)
            {
                _tcs.SetException(new Exception("OnJoinedRoom exception"));
                return;
            }

            // get waiting room intent
            Intent i = Android.Gms.Games.GamesClass.RealTimeMultiplayer.GetWaitingRoomIntent(Client, room, 2);
            _activity.Cast.StartActivityForResult(i, RC_WAITING_ROOM);
        }

        void IRoomUpdateListener.OnLeftRoom(int statusCode, string roomId)
        {
            Dispose();
        }

        void IRoomUpdateListener.OnRoomConnected(int statusCode, IRoom room)
        {
            //Called when all the participants in a real-time room are fully connected.
            SetRoom(room);
            _tcs.SetResult(this);
        }

        void IRoomUpdateListener.OnRoomCreated(int statusCode, IRoom room)
        {
            if (statusCode != Android.Gms.Games.GamesStatusCodes.StatusOk)
            {
                _tcs.SetException(new Exception(GoogleErrorHandlingHelpers.GetMultiplayerErrorMessage(statusCode)));
            }
        }
    }
}