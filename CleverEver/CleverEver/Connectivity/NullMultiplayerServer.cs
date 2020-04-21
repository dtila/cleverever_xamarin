using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleverEver.Common.Collection;
using Acr.UserDialogs;
using CleverEver.Localization;
using CleverEver.Game.Model;
using CleverEver.Game.Multiplayer;
using CleverEver.Game;
using CleverEver.Game.Achievements;

namespace CleverEver.Connectivity
{
    class NullMultiplayerServer : IGameServer
    {
        private static readonly Task Completed;
        private static readonly Task Cancelled;


        private IUserDialogs _dialogs;
        public event EventHandler<InvitationReceivedEventArgs> InvitationReceived;
        public event EventHandler IsConnectedChanged;

        static NullMultiplayerServer()
        {
            var tcs = new TaskCompletionSource<bool>();
            tcs.SetResult(true);
            Completed = tcs.Task;

            tcs = new TaskCompletionSource<bool>();
            tcs.SetCanceled();
            Cancelled = tcs.Task;
        }

        public bool HasMultiplayerSupport { get { return false; } }

        public IGameServerMultiplayerSupport Multiplayer
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IGameServerQuestSupport Quests
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IGameServer Leaderboards
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        IGameServerLeaderboardSupport IGameServer.Leaderboards
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsConnected
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public NullMultiplayerServer(IUserDialogs dialogs)
        {
            _dialogs = dialogs;
        }

        public async Task<IRoomHost> CreateRoomAsync()
        {
            await _dialogs.AlertAsync(UserMessages.feature_not_supported, UserMessages.game_error).ConfigureAwait(false);
            throw new TaskCanceledException();
        }

        public async Task<IRoomJoiner> JoinRoomAsync()
        {
            await _dialogs.AlertAsync(UserMessages.feature_not_supported, UserMessages.game_error).ConfigureAwait(false);
            throw new TaskCanceledException();
        }

        public void Dispose()
        {
        }

        public Task<IPlayer> GetPlayerAsync()
        {
            throw new TaskCanceledException();
        }

        public IList<IGameEventHandler> GetEventHandlers()
        {
            throw new NotImplementedException();
        }

        public Task<IList<IQuest>> GetOpenQuestsAsync()
        {
            throw new NotImplementedException();
        }

        class RoomHost : IRoomHost
        {
            public IList<IGameEventHandler> EventHandlers
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public IPlayer Player
            {
                get { return null; }
            }

            public RangeObservableCollection<Participant> Players
            {
                get { return new RangeObservableCollection<Participant>(); }
            }

            public event EventHandler<PlayerRespondedEventArgs> PlayerResponded;

            public void Leave()
            {
            }

            public Task SendQuestion(Question question)
            {
                return Completed;
            }

            public Task SendResponse(int answerIndex)
            {
                return Completed;
            }

            public Task StartGame(QuestionSet set)
            {
                return Completed;
            }
        }

        class RoomJoiner : IRoomJoiner
        {
            private RangeObservableCollection<Participant> _players;
            public RoomJoiner()
            {
                _players = new RangeObservableCollection<Participant>();
            }
            public IPlayer Player
            {
                get { return null; }
            }

            public RangeObservableCollection<Participant> Players
            {
                get { return _players; }
            }

            public IList<IGameEventHandler> EventHandlers
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public event EventHandler<GameStartedEventArgs> GameStarted;
            public event EventHandler<NextQuestionEventArgs> QuestionChanged;
            public event EventHandler<PlayerRespondedEventArgs> PlayerResponded;

            public void Leave()
            {
            }

            public Task SendResponse(int answerIndex)
            {
                return Completed;
            }
        }
    }
}
