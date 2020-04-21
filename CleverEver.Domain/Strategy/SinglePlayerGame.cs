using CleverEver.Game;
using CleverEver.Game.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleverEver.Game.Achievements;

namespace CleverEver.Game.Strategy
{
    class SinglePlayerGame : IGame
    {
        private QuestionSet _set;
        private int _questionIndex;
        private List<int> _answers;
        private Player _player;
        private bool _gameOver;
        private List<IGameEventHandler> _eventHandlers;

        public event EventHandler<GameOverEventArgs> GameFinished;
        public event EventHandler<GameErrorEventArgs> GameError;

        public event QuestionAnsweredEventDelegate QuestionAnswered;
        public event EventHandler<NextQuestionEventArgs> NextQuestion;


        public SinglePlayerGame(QuestionSet set, IPlayer player)
        {
            _set = set;
            _gameOver = false;
            _answers = new List<int>();
            _questionIndex = 0;
            _player = new Model.Player(player);
            _eventHandlers = new List<IGameEventHandler>();

            OnGameStarted(new GameStartedEventArgs(_set.Name, _set.Questions.Length, _set.Questions[_questionIndex]));
        }

        public Question CurrentQuestion
        {
            get
            {
                if (_questionIndex >= _set.Questions.Length)
                    return null;
                return _set.Questions[_questionIndex];
            }
        }

        public Player Player
        {
            get { return _player; }
        }

        public IList<IGameEventHandler> EventHandlers
        {
            get { return _eventHandlers; }
        }

        public void Answer(int answerIndex)
        {
            CheckGame();
            if (answerIndex < 0 || answerIndex > CurrentQuestion.Answers.Length)
                throw new ArgumentOutOfRangeException(nameof(answerIndex));

            _answers.Add(answerIndex);
            OnQuestionAnswered(QuestionAnswer.Create(CurrentQuestion, answerIndex));
            AdvanceQuestion();
        }

        public void Skip()
        {
            CheckGame();
            _answers.Add(-1);
            OnQuestionAnswered(QuestionAnswer.CreateSkipped(CurrentQuestion));
            AdvanceQuestion();
        }

        public void Stop()
        {
            if (!_gameOver)
            {
                OnGameSuccessfullyEnd(true);
            }
        }

        private void AdvanceQuestion()
        {
            CheckGame();
            if (_questionIndex + 1 < this._set.Questions.Length)
            {
                _questionIndex++;
                OnNextQuestion(new NextQuestionEventArgs(_set.Questions[_questionIndex]));
                return;
            }

            OnGameSuccessfullyEnd(false);
        }

        private void OnQuestionAnswered(QuestionAnswer answer)
        {
            foreach (var handler in _eventHandlers)
                handler.Handle(answer);
        }

        private void OnGameStarted(GameStartedEventArgs e)
        {
            foreach (var handler in _eventHandlers)
                handler.Handle(e);
        }

        private void OnNextQuestion(NextQuestionEventArgs e)
        {
            //foreach (var handler in _eventHandlers)
            //    handler.Handle(e);
            NextQuestion?.Invoke(this, e);
        }

        protected void OnGameSuccessfullyEnd(bool isInterrupted)
        {
            CheckGame();

            var answers = new List<QuestionAnswer>();
            for (int i = 0; i < this._answers.Count; i++)
            {
                answers.Add(_answers[i] == -1 ? QuestionAnswer.CreateSkipped(_set.Questions[i]) : QuestionAnswer.Create(_set.Questions[i], this._answers[i]));
            }

            _gameOver = true;
            var gameOverEvent = new GameOverEventArgs(_player, answers, isInterrupted, _set.Questions.Length);

            foreach (var handler in _eventHandlers)
                handler.Handle(gameOverEvent);
            GameFinished?.Invoke(this, gameOverEvent);
        }


        private void CheckGame()
        {
            if (_gameOver)
                throw new InvalidOperationException("The game is already ended");
        }
    }
}
