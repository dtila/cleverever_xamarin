using CleverEver.Game.Model;
using CleverEver.Game.Multiplayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Game.Strategy
{
    class JoinedGame : NetworkGame
    {
        private IRoomJoiner _gameRoom;
        private Question _currentQuestion;

        public JoinedGame(IRoomJoiner gameRoom)
            : base (gameRoom)
        {
            _gameRoom = gameRoom;
            gameRoom.PlayerResponded += GameRoom_PlayerResponded;
            gameRoom.GameStarted += GameRoom_GameStarted;
            gameRoom.QuestionChanged += GameRoom_QuestionChanged;
        }

        public override Question CurrentQuestion
        {
            get { return _currentQuestion; }
        }

        public override NetworkType Type
        {
            get { return NetworkType.Joined; }
        }

        public override async void Answer(int answerIndex)
        {
            bool allPlayerResponded;
            AddAnswer(_gameRoom.Player.ToParticipant(), _currentQuestion, answerIndex, out allPlayerResponded);
            await _gameRoom.SendResponse(answerIndex).ConfigureAwait(false);
        }

        protected override void OnGameEnded()
        {
            base.OnGameEnded();
            _gameRoom.PlayerResponded -= GameRoom_PlayerResponded;
            _gameRoom.GameStarted -= GameRoom_GameStarted;
            _gameRoom.QuestionChanged -= GameRoom_QuestionChanged;
        }

        private void GameRoom_PlayerResponded(object sender, PlayerRespondedEventArgs e)
        {
            EnsureGameStarted();

            bool allPlayersResponded;
            if (!AddAnswer(e.Participant, CurrentQuestion, e.ResponseIndex, out allPlayersResponded))
                return;
        }

        private void GameRoom_GameStarted(object sender, GameStartedEventArgs e)
        {
            OnGameStarted(e);
        }

        private void GameRoom_QuestionChanged(object sender, NextQuestionEventArgs e)
        {
            if (e.NextQuestion == null)
            {
                // game over
                OnGameSuccessfullyEnd(false);
                _currentQuestion = null;
                return;
            }

            _currentQuestion = e.NextQuestion;
            OnNextQuestion(e);
        }
    }
}
