using CleverEver.Game;
using CleverEver.Pages.Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Analytics
{
    class SingleGameMonitoring : IGameMonitoring
    {
        private IGame _game;
        private Stopwatch _stopWatch;
        private UserSelection _gameSelection;
        private IAnalyticsService _analyticsService;

        public SingleGameMonitoring(UserSelection gameSelection, IGame game)
        {
            _gameSelection = gameSelection;
            _game = game;
            _analyticsService = CleverEver.Composition.DependencyContainer.Resolve<IAnalyticsService>();
            _stopWatch = Stopwatch.StartNew();
            
            game.GameFinished += GameFinished;
            game.GameError += GameError;
        }

        private void GameError(object sender, GameErrorEventArgs e)
        {
            Dispose();
            _analyticsService.TrackGamePlayed(new SingleGamePlayed(_stopWatch.Elapsed, _gameSelection));
        }

        private void GameFinished(object sender, GameOverEventArgs e)
        {
            Dispose();
            _analyticsService.TrackGamePlayed(new SingleGamePlayed(_stopWatch.Elapsed, _gameSelection));
        }
        
        private void Dispose()
        {
            _stopWatch.Stop();
            _game.GameFinished -= GameFinished;
            _game.GameError -= GameError;
        }
    }
}
