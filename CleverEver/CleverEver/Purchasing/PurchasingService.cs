using Acr.UserDialogs;
using CleverEver.Analytics;
using CleverEver.Game.Model;
using CleverEver.Game.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Purchasing
{
    /// <summary>
    /// The question domain service
    /// </summary>
    public class PurchasingService
    {
        private IAnalyticsService _analytics;
        private IPurchasingService _purchasingService;
        private IQuestionRepository _questionRepository;
        private Storage.QuestionsRepositoryManager _localRepository;

        public PurchasingService(IAnalyticsService analytics, IPurchasingService purchasingService, IQuestionRepository questionRepository, Storage.QuestionsRepositoryManager localRepository)
        {
            _analytics = analytics;
            _purchasingService = purchasingService;
            _questionRepository = questionRepository;
            _localRepository = localRepository;
        }

        public async Task Purchase(Packet packet)
        {
            if (packet == null)
                throw new ArgumentNullException(nameof(packet));

            var stopWatch = Stopwatch.StartNew();
            try
            {
                var purchasedItem = await _purchasingService.PurchaseAsync(packet.Id, "").ConfigureAwait(false);
                var signedPurchasedItem = purchasedItem as SignedPurchasedItem;
                var purchaseTask = signedPurchasedItem == null
                    ? _questionRepository.Purchase(packet.Id, purchasedItem.PurchaseToken, purchasedItem.DeveloperPayload)
                    : _questionRepository.Purchase(packet.Id, purchasedItem.PurchaseToken, purchasedItem.DeveloperPayload, signedPurchasedItem.SignedData, signedPurchasedItem.Signature);

                await _analytics.TrackPurchaseCompletedAsync(packet, purchasedItem).ConfigureAwait(false);
                
                var purchasedSet = await purchaseTask.ConfigureAwait(false);
                _localRepository.Set(purchasedSet);
            }
            catch (TaskCanceledException)
            {
                await _analytics.TrackPurchaseCancelledAsync(packet, stopWatch.Elapsed);
                throw;
            }
            catch (Exception ex)
            {
                Composition.DependencyContainer.Resolve<Logging.IExceptionLogger>().LogError(ex);
                await _analytics.TrackPurchaseCancelledAsync(packet, stopWatch.Elapsed);
                throw;
            }
        }
    }
}
