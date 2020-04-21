using CleverEver.Game;
using CleverEver.Phone.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace CleverEver.Pages.Game
{
    public class ResultViewModel : BaseViewModel
    {
        private IGameServer _gameServer;

        public int TotalScore { get; private set; }
        public int BonusScore { get; private set; }

        public int TotalQuestions { get; private set; }
        public int CorrectAnswered { get; private set; }
        public int WrongAnswered { get; private set; }
        public int ConsecutiveAnswers { get; private set; }

        public ICommand ShowLeaderboardsCommand { get; private set; }

        public ResultViewModel(IGameServer gameServer)
        {
            _gameServer = gameServer;
            ShowLeaderboardsCommand = new Command(ShowLeaderboard, () => _gameServer.IsConnected);
        }

        public void OnAppearing()
        {
            _gameServer.IsConnectedChanged += _gameServer_IsConnectedChanged;
        }

        public void OnDisappearing()
        {
            _gameServer.IsConnectedChanged -= _gameServer_IsConnectedChanged;
        }

        private void _gameServer_IsConnectedChanged(object sender, EventArgs e)
        {
            ShowLeaderboardsCommand.CanExecute(null);
        }

        public void Init(GameOverEventArgs gameOver, PlayViewModel playViewModel)
        {
            TotalQuestions = gameOver.TotalQuestions;

            ParticipantAnswers playerAnswers;
            if (gameOver.TryGetPlayerAnswers(out playerAnswers))
            {
                CorrectAnswered = playerAnswers.CorrectAnswersCount;
                WrongAnswered = playerAnswers.Answers.Count - CorrectAnswered;
            }

            ConsecutiveAnswers = playViewModel.ConsecutiveAnswers;
            TotalScore = playViewModel.Game.Player.TotalScore;
            BonusScore = playViewModel.Game.Player.BonusScore;
        }

        private void ShowLeaderboard()
        {
            _gameServer.Leaderboards.ShowTopLeaderboardAsync();
        }
    }
}
