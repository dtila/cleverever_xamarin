using Acr.UserDialogs;
using CleverEver.Analytics;
using CleverEver.Game;
using CleverEver.Game.Achievements;
using CleverEver.Game.Model;
using CleverEver.Localization;
using CleverEver.Phone.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Pages.Game
{
    public class PlayViewModel : BaseViewModel
    {
        private IGame _player;
        private IGameMonitoring _gameAnalytics;
        private IUserDialogs _userDialog;

        public Question CurrentQuestion
        {
            get { return _player?.CurrentQuestion; }
        }

        private string _congratulateText;
        public string CongratulateText
        {
            get { return _congratulateText; }
            set { SetField(ref _congratulateText, value); }
        }

        public int Score
        {
            get { return _player.Player.GameScore; }
        }

        private int _simultaneouslyCorrectAnswers;
        public int ConsecutiveAnswers
        {
            get { return _simultaneouslyCorrectAnswers; }
            set { SetField(ref _simultaneouslyCorrectAnswers, value); }
        }

        public IGame Game
        {
            get { return _player; }
        }

        public PlayViewModel(IUserDialogs userDialogs)
        {
            _userDialog = userDialogs;
        }

        public void Initialize(IGame player, IGameMonitoring gameAnalytics)
        {
            if (_player != null)
            {
                _player.GameError -= Player_OnGameError;
            }

            _gameAnalytics = gameAnalytics;
            _player = player;

            _player.GameError += Player_OnGameError;
            player.EventHandlers.Add(new CongratulateUser(this));
        }

        public async Task OnBackButtonPressed()
        {
            var prompt = await _userDialog.ConfirmAsync(UserMessages.cancel_game, Actions.cancel_game, Actions.yes, Actions.no);
            if (prompt == true)
            {
                _player.Stop();
            }
        }

        private async void Player_OnGameError(object sender, GameErrorEventArgs e)
        {
            await _userDialog.AlertAsync(GetGameErrorMessage(e.Reason), Localization.UserMessages.game_error);

            /*if ((e.Reason & GameErrorReason.StopGame) != 0)
            {
                await Navigation.PopAsync().ConfigureAwait(false);
            }*/
        }

        private string GetGameErrorMessage(GameErrorReason reason)
        {
            var messages = string.Join("\n", GetErrorMessages(reason));
            if (string.IsNullOrEmpty(messages))
                throw new NotImplementedException($"The reason {reason} is not recognised");
            return messages;
        }

        private IEnumerable<string> GetErrorMessages(GameErrorReason reason)
        {
            if ((reason & GameErrorReason.TooLessPlayers) != 0)
                yield return Localization.UserMessages.too_less_players;
            if ((reason & GameErrorReason.StopGame) != 0)
                yield return Localization.UserMessages.game_will_stop;
        }


        /// <summary>
        /// We keep track of the simultaniously correct answers
        /// </summary>
        class CongratulateUser : IGameEventHandler
        {
            private const int MaxConsecutiveAnswers = 5;

            private static readonly string[] Messages = new[] { "Bravo", "Felicitari", "Wow", "Impresionant", "Magnific", "Grozav" };

            private PlayViewModel _owner;
            private int _messageIndex;
            private int _simultaneouslyCorrectAnswers;

            public CongratulateUser(PlayViewModel owner)
            {
                _owner = owner;
                _messageIndex = 0;
                _simultaneouslyCorrectAnswers = 0;
            }

            public void Handle(GameOverEventArgs e)
            {
            }

            public void Handle(QuestionAnswer playerAnswer)
            {
                if (!playerAnswer.IsCorrect)
                {
                    _simultaneouslyCorrectAnswers = 0;
                    return;
                }

                var increment = Math.Min(MaxConsecutiveAnswers, ++_simultaneouslyCorrectAnswers);
                _owner.Game.Player.AddScore(increment, _simultaneouslyCorrectAnswers - 1);
                _owner.RaisePropertyChanged(nameof(Score));

                if (_simultaneouslyCorrectAnswers > 1)
                {
                    if (++_messageIndex >= Messages.Length)
                        _messageIndex = 0;

                    _owner.CongratulateText = Messages[_messageIndex];
                    _owner.ConsecutiveAnswers += 1;
                }
            }

            public void Handle(GameErrorEventArgs e)
            {
            }

            public void Handle(GameStartedEventArgs e)
            {
            }
        }
    }
}
