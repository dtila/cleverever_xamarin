using Acr.UserDialogs;
using CleverEver.Analytics;
using CleverEver.Composition;
using CleverEver.Game;
using CleverEver.Localization;
using CleverEver.Game.Multiplayer;
using CleverEver.Pages.Game;
using CleverEver.Phone.ViewModels;
using CleverEver.Game.Strategy;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using CleverEver.Pages.Quests;

namespace CleverEver.Phone.Pages.Home
{
    public class MainPageViewModel : BaseViewModel
    {
        enum StartGameType
        {
            None,
            Single,
            Hosted
        }

        private Random _rand;
        private IUserDialogs _dialogs;
        private StartGameType _startGameType;
        private IAnalyticsService _analytics;
        private PlayViewModel _currentGame;

        private IGameServer _gameServer;
        private Connectivity.PlayerSigninBehavior _playerSignInBehavior;

        private bool _isAnyOpenedQuest = false;
        public bool IsAnyOpenedQuest
        {
            get { return _isAnyOpenedQuest; }
            private set { SetField(ref _isAnyOpenedQuest, value); }
        }

        public ICommand PracticeCommand { get; }
        public ICommand CreateGameCommand { get; }
        public ICommand JoinGameCommand { get; }
        public ICommand ViewQuestsCommand { get; }

        public MainPageViewModel(IGameServer serverHost, IUserDialogs dialogs, IAnalyticsService analytics)
        {
            _gameServer = serverHost;
            _dialogs = dialogs;
            _analytics = analytics;
            _rand = new Random();
            _startGameType = StartGameType.None;
            _playerSignInBehavior = new Connectivity.PlayerSigninBehavior(serverHost);

            if (_gameServer.Multiplayer != null)
            {
                _gameServer.Multiplayer.InvitationReceived += InvitationReceived;
            }

            PracticeCommand = new Command(SinglePlayer);
            CreateGameCommand = new Command(CreateGame);
            JoinGameCommand = new Command(JoinGame);
            ViewQuestsCommand = new Command(() => _gameServer.Quests.Select());

            _analytics.ApplicationCreated();
            if (_gameServer.Quests != null)
            {
                _gameServer.Quests.QuestCompleted += Quests_QuestCompleted;
                _gameServer.Quests.GetOpenQuestsAsync().ContinueWith(task =>
                {
                    if (!task.IsCompleted || task.IsFaulted)
                        return;

                    IsAnyOpenedQuest = task.Result.Count > 0;
                });
            }

            MessagingCenter.Unsubscribe<UserSelection>(this, "StartGame");
            MessagingCenter.Subscribe<UserSelection>(this, "StartGame", StartGameByUserSelection);
        }

        public void OnAppearing()
        {
            _analytics.HomeOpened();
        }

        private async void InvitationReceived(object sender, InvitationReceivedEventArgs e)
        {
            var message = string.Format(UserMessages.accept_invitation_format, e.Inviter);
            if (await _dialogs.ConfirmAsync(message, Actions.join_game, Actions.yes, Actions.no) == true)
            {
                await AcceptInvitation(e.Invitation).ConfigureAwait(false);
            }
            else
            {
                e.Invitation.Decline();
            }
        }

        private async void SinglePlayer()
        {
            _startGameType = StartGameType.Single;
            var vm = DependencyContainer.Resolve<ViewCategoriesViewModel>();
            await Navigation.PushAsync(new ViewCategoriesPage(vm));
        }

        private async void CreateGame()
        {
            var vm = DependencyContainer.Resolve<ViewCategoriesViewModel>();
            _startGameType = StartGameType.Hosted;
            await Navigation.PushAsync(new ViewCategoriesPage(vm));
        }

        private async void JoinGame()
        {
            try
            {
                _dialogs.ShowLoading();
                var room = await _gameServer.Multiplayer.JoinRoomAsync();
                var game = GameFactory.CreateInvitedGame(room);
                await StartGame(game, new NetworkGameMonitoring(game));
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert(Actions.join_game, ex.Message, Actions.ok);
            }
            finally
            {
                _dialogs.HideLoading();
            }
        }

        private async void StartGameByUserSelection(UserSelection selection)
        {
            var gameType = _startGameType;
            if (gameType == StartGameType.None)
                return;

            if (!selection.QuestionSet.CanPlay)
            {
                await _dialogs.AlertAsync(UserMessages.game_not_playable).ConfigureAwait(false);
                return;
            }

            if (gameType == StartGameType.Single)
            {
                await StartSingleGame(selection).ConfigureAwait(false);
                return;
            }

            if (gameType == StartGameType.Hosted)
            {
                await StartHostedGame(selection).ConfigureAwait(false);
                return;
            }
        }

        private async Task StartSingleGame(UserSelection selection)
        {
            try
            {
                _dialogs.ShowLoading();

                selection.QuestionSet.Randomize(_rand);
                var player = await _playerSignInBehavior.GetCurrentPlayerAsync();
                var game = GameFactory.CreateSingleGame(selection.QuestionSet, player);
                await StartGame(game, new SingleGameMonitoring(selection, game));
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert(Actions.create_game, ex.Message, Actions.ok);
            }
            finally
            {
                _dialogs.HideLoading();
            }
        }

        private async Task StartHostedGame(UserSelection userSelection)
        {
            try
            {
                _dialogs.ShowLoading();
                userSelection.QuestionSet.Randomize(_rand);
                var room = await _gameServer.Multiplayer.CreateRoomAsync();
                var game = GameFactory.CreateHostGame(userSelection.QuestionSet, room);
                await StartGame(game, new NetworkGameMonitoring(game, userSelection));
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert(Actions.create_game, ex.Message, Actions.ok);
            }
            finally
            {
                _dialogs.HideLoading();
            }
        }

        private async Task AcceptInvitation(IInvitation invitation)
        {
            try
            {
                _dialogs.ShowLoading();
                var game = GameFactory.CreateInvitedGame(await invitation.Accept());
                await StartGame(game, new NetworkGameMonitoring(game));
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert(Actions.join_game, ex.Message, Actions.ok);
            }
            finally
            {
                _dialogs.HideLoading();
            }
        }

        private async Task StartGame(IGame game, IGameMonitoring gameAnalytics)
        {
            foreach (var handler in _gameServer.GetEventHandlers())
            {
                game.EventHandlers.Add(handler);
            }

            await Task.Delay(1500);//TODO: Check this bug https://bugzilla.xamarin.com/show_bug.cgi?id=36907
            var playModel = DependencyContainer.Resolve<PlayViewModel>();
            playModel.Initialize(game, gameAnalytics);
            game.GameFinished += Game_OnGameFinished;

            await Navigation.PushAsync(new PlayPage(playModel));
            _currentGame = playModel;
        }

        private async void Game_OnGameFinished(object sender, GameOverEventArgs e)
        {
            var game = sender as IGame;
            game.GameFinished -= Game_OnGameFinished;
            game.EventHandlers.Clear();

            game.Stop();

            //await _dialogs.AlertAsync(string.Format(UserMessages.game_correct_questions_format, playerAnswers.CorrectAnswersCount));

            await Navigation.PopAsync();

            var resultVm = Composition.DependencyContainer.Resolve<ResultViewModel>();
            resultVm.Init(e, _currentGame);
            await Navigation.PushAsync(new Result { BindingContext = resultVm });

            _currentGame = null;
            // show the leaderboard
            //await Navigation.PopToRootAsync(false).ConfigureAwait(false);
        }

        private void ViewQuests()
        {
            Navigation.PushAsync(new Quests());
        }

        private async void Quests_QuestCompleted(object sender, QuestCompletedEventArgs e)
        {
            try
            {
                var claimResult = await e.Quest.Claim();
                JsonContestSecret contestSecret;
                if (JsonContestSecret.TryCreate(claimResult, out contestSecret))
                {
                    // it's a contest, we need to see if he won or now
                    return;
                }

                QuestCompletedViewModel vm;
                if (!QuestCompletedViewModel.TryCreate(claimResult, e.Quest, out vm))
                {
                    await _dialogs.AlertAsync("You have completed a quest").ConfigureAwait(false);
                    return;
                }

                await Navigation.PushAsync(new CleverEver.Pages.Quests.QuestCompleted()).ConfigureAwait(false);
                return;
            }
            catch (Exception ex)
            {
                await _dialogs.AlertAsync("");
            }
        }
    }
}
