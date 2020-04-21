using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Games.MultiPlayer.RealTime;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Gms.Common.Apis;
using CleverEver.Common.Collection;
using CleverEver.Logging;
using CleverEver.Composition;
using CleverEver.Analytics;
using CleverEver.Game.Multiplayer;
using CleverEver.Game.Model;
using CleverEver.Game;
using CleverEver.Game.Achievements;
using CleverEver.Droid.Connectivity.Google;

namespace CleverEver.Droid.Connectivity.Multiplayer
{
    abstract class GoogleRoomClient : Java.Lang.Object, INetworkClient, IRoomStatusUpdateListener, IRealTimeMessageReceivedListener
    {
        private string _roomId;
        private Game.Model.Participant _player;
        private IExceptionLogger _logger;
        private GoogleApiClient _client;
        private RangeObservableCollection<Game.Model.Participant> _players;

        public event EventHandler<PlayerRespondedEventArgs> PlayerResponded;

        protected GoogleRoomClient(GoogleApiClient client)
        {
            _client = client;
            _players = new RangeObservableCollection<Game.Model.Participant>();
            _logger = DependencyContainer.Resolve<IExceptionLogger>();
        }

        public static Game.Model.Participant CreateParticipant(Android.Gms.Games.MultiPlayer.IParticipant participant)
        {
            return new Game.Model.Participant(participant.ParticipantId, participant.DisplayName);
        }

        public GoogleApiClient Client
        {
            get { return _client; }
        }

        public IPlayer Player
        {
            get
            {
                if (_roomId == null || _player == null)
                    throw new InvalidOperationException("The room has not been set");

                return new GooglePlayer(_client);
            }
        }

        public RangeObservableCollection<Game.Model.Participant> Players
        {
            get { return _players; }
        }

        public abstract void Leave();

        public string RoomId { get { return _roomId; } }

        public IList<IGameEventHandler> EventHandlers
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void SetRoom(IRoom room)
        {
            _roomId = room.RoomId;
            _players.ReplaceCollection(room.Participants.Select(CreateParticipant));

            if (_player == null)
            {
                using (var player = Android.Gms.Games.GamesClass.Players.GetCurrentPlayer(_client))
                using (var participant = room.GetParticipant(room.GetParticipantId(player.PlayerId)))
                    _player = CreateParticipant(participant);
            }
        }

        public Task SendResponse(int answerIndex)
        {
            return SendMessageAsync(new PlayerRespondedEventArgs(Player.ToParticipant(), answerIndex));
        }

        public void RemoveCurrentPlayer()
        {
            if (_player == null)
                throw new InvalidOperationException("There is no player active");

            _players.Remove(_player);
        }

        protected Task SendMessageAsync<T>(T message)
        {
            return Task.Run(() => SendMessage(message));
        }

        protected void SendMessage<T>(T message)
        {
            var bson = GoogleSerializationHelpers.SerializeMessage(message);
            foreach (var participant in _players)
            {
                if (participant.Equals(_player))
                    continue;

                try
                {
                    var code = Android.Gms.Games.GamesClass.RealTimeMultiplayer.SendReliableMessage(_client, null, bson, _roomId, participant.Id);
                    if (code < 0)
                        throw new InvalidOperationException($"Unreliable message was not sent");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Unable to send message to {participant.Name}");
                }
            }
        }

        protected void RemoveParticipants(IList<string> participantIds)
        {
            foreach (var participantId in participantIds)
            {
                for (int i = _players.Count - 1; i >= 0; i--)
                {
                    if (_players[i].Id == participantId)
                        _players.RemoveAt(i);
                }
            }
        }

        protected void AddParticipants(IRoom room, ICollection<string> participantIds)
        {
            foreach (var participantId in participantIds)
            {
                if (!ExistsParticipant(participantId))
                {
                    var participant = room.GetParticipant(participantId);
                    _players.Add(CreateParticipant(participant));
                }
            }
        }

        protected bool ExistsParticipant(string participantId)
        {
            foreach (var player in _players)
            {
                if (player.Id == participantId)
                    return true;
            }
            return false;
        }

        protected virtual bool TryDispatchMessage(object message)
        {
            var playerResponse = message as PlayerRespondedEventArgs;
            if (playerResponse != null)
            {
                PlayerResponded?.Invoke(this, playerResponse);
                return true;
            }

            return false;
        }

        private void CheckDisposed()
        {
            //if ()
        }

        void IRealTimeMessageReceivedListener.OnRealTimeMessageReceived(RealTimeMessage message)
        {
            try
            {
                var json = GoogleSerializationHelpers.DeserializeMessage(message);
                if (!TryDispatchMessage(json))
                    throw new InvalidOperationException($"The message '{json}' it is not recognized");
            }
            catch (Newtonsoft.Json.JsonException ex)
            {
                _logger.LogError(ex, "The received message is in an invalid format");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
            }
        }

        void IRoomStatusUpdateListener.OnConnectedToRoom(IRoom room)
        {
        }

        void IRoomStatusUpdateListener.OnDisconnectedFromRoom(IRoom room)
        {
        }

        void IRoomStatusUpdateListener.OnP2PConnected(string participantId)
        {
        }

        void IRoomStatusUpdateListener.OnP2PDisconnected(string participantId)
        {
        }

        void IRoomStatusUpdateListener.OnPeerDeclined(IRoom room, IList<string> participantIds)
        {
            //RemoveParticipants(participantIds);
        }

        void IRoomStatusUpdateListener.OnPeerInvitedToRoom(IRoom room, IList<string> participantIds)
        {
        }

        void IRoomStatusUpdateListener.OnPeerJoined(IRoom room, IList<string> participantIds)
        {
        }

        void IRoomStatusUpdateListener.OnPeerLeft(IRoom room, IList<string> participantIds)
        {
        }

        void IRoomStatusUpdateListener.OnPeersConnected(IRoom room, IList<string> participantIds)
        {
            AddParticipants(room, participantIds);
        }

        void IRoomStatusUpdateListener.OnPeersDisconnected(IRoom room, IList<string> participantIds)
        {
            RemoveParticipants(participantIds);
        }

        void IRoomStatusUpdateListener.OnRoomAutoMatching(IRoom room)
        {
        }

        void IRoomStatusUpdateListener.OnRoomConnecting(IRoom room)
        {
        }
    }
}