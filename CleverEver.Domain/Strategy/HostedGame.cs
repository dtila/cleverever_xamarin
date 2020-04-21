using CleverEver.Game.Model;
using CleverEver.Game.Multiplayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Game.Strategy
{
    class HostedGame : NetworkGame, IGame
    {
        private QuestionSet _set;
        private IRoomHost _gameRoom;
        private int _questionIndex;

        /// <summary>
        /// Create a hosted game for a specified question set and a game room
        /// </summary>
        /// <param name="set"></param>
        /// <param name="gameRoom">
        /// The host game room platform specific implementation. By design, the room specified here needs to have all the participants
        /// joined at the moment of creation. This is actually necessary, since the game does not know any information about that
        /// </param>
        public HostedGame(QuestionSet set, IRoomHost gameRoom)
            : base(gameRoom)
        {
            _gameRoom = gameRoom;
            _set = set;
            _questionIndex = 0;

            gameRoom.PlayerResponded += GameRoom_OnPlayerResponded;
            SendGameStarted().ConfigureAwait(false);
        }

        public override Question CurrentQuestion
        {
            get
            {
                EnsureGameStarted();
                if (_questionIndex >= _set.Questions.Length)
                    return null;
                return _set.Questions[_questionIndex];
            }
        }

        public override NetworkType Type
        {
            get { return NetworkType.Hosted; }
        }

        public async override void Answer(int answerIndex)
        {
            EnsureGameStarted();
            await _gameRoom.SendResponse(answerIndex);
            await HandlePlayerResponse(_gameRoom.Player.ToParticipant(), answerIndex);
            OnQuestionAnswered(QuestionAnswer.Create(CurrentQuestion, answerIndex));
        }

        protected override void OnGameEnded()
        {
            base.OnGameEnded();
            _gameRoom.PlayerResponded -= GameRoom_OnPlayerResponded;
        }

        private async void GameRoom_OnPlayerResponded(object sender, PlayerRespondedEventArgs e)
        {
            EnsureGameStarted();
            await HandlePlayerResponse(e.Participant, e.ResponseIndex).ConfigureAwait(false);
        }

        private async Task HandlePlayerResponse(Participant player, int responseIndex)
        {
            bool allPlayersResponded;
            if (!AddAnswer(player, CurrentQuestion, responseIndex, out allPlayersResponded) || !allPlayersResponded)
                return;

            if (_questionIndex + 1 < _set.Questions.Length)
            {
                _questionIndex++;
                await SendCurrentQuestion().ConfigureAwait(false);
                return;
            }

            await _gameRoom.SendQuestion(null);
            OnGameSuccessfullyEnd(_set.Questions.Length, false);
        }

        private async Task SendGameStarted()
        {
            await _gameRoom.StartGame(_set);
            OnGameStarted(new GameStartedEventArgs(_set.Name, _set.Questions.Length, _set.Questions[_questionIndex]));
        }

        private async Task SendCurrentQuestion()
        {
            await _gameRoom.SendQuestion(_set.Questions[_questionIndex]);
            OnNextQuestion(new NextQuestionEventArgs(_set.Questions[_questionIndex]));
        }
    }
}
