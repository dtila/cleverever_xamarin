using Acr.UserDialogs;
using CleverEver.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Purchasing
{
    class NullPurchasingServer : IPurchasingService
    {
        private IUserDialogs _dialogs;

        public NullPurchasingServer(IUserDialogs dialogs)
        {
            _dialogs = dialogs;
        }

        public async Task<IList<PurchasableItem>> GetAvailableItems()
        {
            throw new TaskCanceledException();
        }

        public async Task<IList<PurchasedItem>> GetPurchasedItems()
        {
            throw new TaskCanceledException();
        }

        public async Task<PurchasedItem> PurchaseAsync(string productId, string payload)
        {
            await Task.Delay(1000);
            await _dialogs.AlertAsync(UserMessages.service_unavailable);
            throw new TaskCanceledException();
        }
    }
}
