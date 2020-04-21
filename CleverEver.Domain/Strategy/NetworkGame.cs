using CleverEver.Game.Model;
using CleverEver.Game.Multiplayer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleverEver.Game.Achievements;

namespace CleverEver.Game.Strategy
{
    abstract class NetworkGame : INetworkGame
    {
        public const int MinimumPlayers = 2;
        public const int MaximumPlayers = 10;

        private bool _gameOver;
        private Player _player;
        private List<PlayerResponse> _answers;
        private int _initialPlayersCount;
        private INetworkClient _gameRoom;
        private GameStartedEventArgs _startedEvent;
        private List<IGameEventHandler> _eventHandlers;

        public abstract Question CurrentQuestion { get; }

        public event EventHandler<GameStartedEventArgs> GameStarted;
        public event EventHandler<GameOverEventArgs> GameFinished;
        public event EventHandler<GameErrorEventArgs> GameError;

        public event QuestionAnsweredEventDelegate QuestionAnswered;
        public event EventHandler<NextQuestionEventArgs> NextQuestion;

        public NetworkGame(INetworkClient gameRoom)
        {
            if (gameRoom.Players.Count < MinimumPlayers)
                throw new HostedGameException($"Minimum players are required to create game: 2 instead of {gameRoom.Players.Count}");

            _answers = new List<PlayerResponse>(gameRoom.Players.Count * 10);
            _gameRoom = gameRoom;
            _player = new Player(gameRoom.Player);
            _initialPlayersCount = _gameRoom.Players.Count;
            _gameOver = false;
            gameRoom.Players.CollectionChanged += Players_CollectionChanged;
            _eventHandlers = new List<Achievements.IGameEventHandler>();
        }

        public abstract NetworkType Type { get; }

        public Player Player
        {
            get { return _player; }
        }

        public IList<IGameEventHandler> EventHandlers
        {
            get { return _eventHandlers; }
        }

        public abstract void Answer(int answerIndex);

        public void Skip()
        {
            Answer(PlayerResponse.SkippedIndex);
        }

        public void Stop()
        {
            if (!_gameOver)
            {
                OnGameSuccessfullyEnd(_startedEvent.TotalQuestions, true);
            }

            _gameRoom.Leave();
        }

        /// <summary>
        /// Try to add an answer for the give pair player question
        /// </summary>
        /// <param name="player"></param>
        /// <param name="question"></param>
        /// <param name="responseIndex"></param>
        /// <param name="allPlayersResponded"></param>
        /// <returns></returns>
        protected bool AddAnswer(Participant player, Question question, int responseIndex, out bool allPlayersResponded)
        {
            EnsureGameStarted();
            if (HasResponse(player))
            {
                allPlayersResponded = false;
                return false;
            }

            _answers.Add(responseIndex == PlayerResponse.SkippedIndex
                ? PlayerResponse.CreateSkipped(question, player)
                : PlayerResponse.Create(question, player, responseIndex));
            allPlayersResponded = AllPlayersResponded();
            return true;
        }

        protected void EnsureGameStarted()
        {
            if (_gameOver)
                throw new InvalidOperationException("The game is already ended");
        }

        protected void OnGameFailureEnded(GameErrorReason reason)
        {
            EnsureGameStarted();
            GameError?.Invoke(this, new GameErrorEventArgs(reason | GameErrorReason.StopGame));
            OnGameEnded();
        }

        protected void OnGameSuccessfullyEnd(bool isInterrupted)
        {
            OnGameSuccessfullyEnd(_startedEvent.TotalQuestions, isInterrupted);
        }

        protected void OnGameSuccessfullyEnd(int totalQuestions, bool isInterrupted)
        {
            EnsureGameStarted();
            var query = from playerAnswers in _answers.GroupBy(li => li.Player)
                        let answers = from answer in playerAnswers
                                      select answer.IsSkipped
                                            ? QuestionAnswer.CreateSkipped(answer.Question)
                                            : QuestionAnswer.Create(answer.Question, answer.ResponseIndex)
                        select new ParticipantAnswers(playerAnswers.Key, answers.ToList());

            GameFinished?.Invoke(this, new GameOverEventArgs(_gameRoom.Player.ToParticipant(), query.ToList(), isInterrupted, totalQuestions));
            OnGameEnded();
        }

        protected void OnGameStarted(GameStartedEventArgs e)
        {
            _startedEvent = e;
            GameStarted?.Invoke(this, e);
        }

        protected void OnNextQuestion(NextQuestionEventArgs e)
        {
            NextQuestion?.Invoke(this, e);
        }

        protected void OnQuestionAnswered(QuestionAnswer answer)
        {
            QuestionAnswered?.Invoke(this, answer);
        }


        protected virtual void OnGameEnded()
        {
            EnsureGameStarted();

            // I have noticed that it needs to pass a few seconds until we need to leave the room
            // The reason is that, the ending game message is sent after 
            Task.Delay(5000).ContinueWith(t => _gameRoom.Leave());

            _gameOver = true;
            _gameRoom.Players.CollectionChanged -= Players_CollectionChanged;
        }

        private void Players_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            EnsureGameStarted();
            if (_gameRoom.Players.Count < _initialPlayersCount)
            {
                var reason = GameErrorReason.StopGame;
                if (_gameRoom.Players.Contains(_gameRoom.Player.ToParticipant())) // if we have the current player
                    reason = reason | GameErrorReason.TooLessPlayers;

                // if we have less players than old ones, we need to disconnect the game (because this is a business logic)
                OnGameFailureEnded(reason);
            }
        }

        private bool AllPlayersResponded()
        {
            foreach (var player in _gameRoom.Players)
            {
                if (!HasResponse(player))
                    return false;
            }
            return true;
        }

        private bool HasResponse(Participant player)
        {
            foreach (var answer in _answers)
            {
                if (answer.Player.Equals(player) && answer.Question.Equals(CurrentQuestion))
                    return true;
            }
            return false;
        }


        [DebuggerDisplay("{Player.Name} - {Question}")]
        internal struct PlayerResponse
        {
            public const int SkippedIndex = -1;

            public Question Question { get; }
            public Participant Player { get; }
            public int ResponseIndex { get; }
            public bool IsSkipped
            {
                get { return ResponseIndex == SkippedIndex; }
            }

            private PlayerResponse(Question question, Participant player, int responseIndex)
            {
                Question = question;
                Player = player;
                ResponseIndex = responseIndex;
            }

            public static PlayerResponse Create(Question question, Participant player, int responseIndex)
            {
                return new PlayerResponse(question, player, responseIndex);
            }

            public static PlayerResponse CreateSkipped(Question question, Participant player)
            {
                return new PlayerResponse(question, player, SkippedIndex);
            }
        }
    }
}
