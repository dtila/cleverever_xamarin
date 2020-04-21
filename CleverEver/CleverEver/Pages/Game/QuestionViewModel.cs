using Acr.UserDialogs;
using CleverEver.Game;
using CleverEver.Game.Model;
using CleverEver.Phone.ViewModels;
using CleverEver.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Pages.Game
{
    public class QuestionViewModel : BaseViewModel
    {
        private IGame _player;

        public string Text
        {
            get { return Question?.Text; }
        }
        public string[] Answers
        {
            get { return Question?.Answers; }
        }

        public Question Question
        {
            get { return _player?.CurrentQuestion; }
        }

        public void Initialize(IGame player)
        {
            if (_player != null)
            {
                _player.NextQuestion -= Game_OnNextQuestionResponded;
            }

            _player = player;
            _player.NextQuestion += Game_OnNextQuestionResponded;
            Refresh();
        }

        public bool IsAnswerCorrect(int answerIndex, out int correctIndex)
        {
            if (Question == null)
                throw new InvalidOperationException("No active question");

            correctIndex = Question.CorrectIndex;
            return Question.CorrectIndex == answerIndex;
        }

        public void Answer(int answerIndex)
        {
            _player.Answer(answerIndex);
        }

        private void Game_OnNextQuestionResponded(object sender, NextQuestionEventArgs e)
        {
            Refresh();
        }

        private void Refresh()
        {
            RaisePropertyChanged(nameof(Question));
            RaisePropertyChanged(nameof(Text));
            RaisePropertyChanged(nameof(Answers));
        }
    }
}
