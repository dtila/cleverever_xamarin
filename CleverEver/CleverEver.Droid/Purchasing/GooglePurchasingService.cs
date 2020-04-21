using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CleverEver.Droid.Messaging;
using CleverEver.Purchasing;
using Xamarin.InAppBilling;
using Xamarin.InAppBilling.Utilities;
using Com.Android.Vending.Billing;
using GPurchase = Xamarin.InAppBilling.Purchase;

namespace CleverEver.Droid.Purchasing
{
    sealed class GooglePurchasingService : IPurchasingService, IDisposable
    {
        private IActivity _activity;
        private InAppBillingServiceConnection _connection;
        private TaskCompletionSource<bool> _connectionTcs;

        public GooglePurchasingService(IActivity activity)
        {
            _activity = activity;
            var key = Security.Unify(new[] 
            {
                "MIIBIjANBgkqhkiG9",
                "w0BAQEFAAOCAQ8",
                "AMIIBC",
                "gKCAQEAlHEus6+5SSh1F78IgevvJBEu",
                "fRPqmk",
                "YBfpl0ea8bOlVHP7fl0iUuIZ2Rk/J3E",
                "wD7HSa3bf1U2D46/Stq5GnxGldKYHIe3K91NejG0YQ2d622wNiqQv/UNmj+OsjreVEVcHhN8wwBA2jD5cPpyikieZr7Kq9mwY+DGq3fVhkgXaCaug91C85ccbdV9OrEq4AINCg1KAfFiyNGkY1W79SrFqNTQMGRzwo0rg9ICi3",
                "gpHODkrxtHEwfx38s3OeJVM7N9K7GcG3dBhGM9k0HQt/CJSPei5jx/kz1Wtz9FIyexay7+D9v2oRvNGGk",
                "evXGZeb1WPuIHChXid7I7Jr0h07pRwIDAQAB"
            }, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 });

            _connection = new InAppBillingServiceConnection(activity.Cast, key);
            _connection.OnConnected += Connection_Connected;
            _connection.OnInAppBillingError += Connection_OnInAppBillingError;

            _connectionTcs = new TaskCompletionSource<bool>();
            if (_connection.Connected)
            {
                _connectionTcs.SetResult(true);
            }
            else
            {
                _connection.Connect();
            }
        }

        public async Task<PurchasedItem> PurchaseAsync(string productId, string payload)
        {
            await EnsureConnected();

            using (var op = new PurchaseOperation(_activity, _connection.Service, _connection.BillingHandler, productId, payload))
                return await op.Task.ConfigureAwait(false);
        }

        public async Task<IList<PurchasedItem>> GetPurchasedItems()
        {
            await EnsureConnected();

            using (var operation = new GetPurchasedOperation(_connection.BillingHandler, ItemType.Product))
                return await operation.Task.ConfigureAwait(false);
        }

        public async Task<IList<PurchasableItem>> GetAvailableItems()
        {
            await EnsureConnected();

            var products = await _connection.BillingHandler.QueryInventoryAsync(new[] { ReservedTestProductIDs.Purchased }, ItemType.Product);

            var result = new List<PurchasableItem>(products.Count);
            foreach (var product in products)
            {
                result.Add(new PurchasableItem(product.ProductId, product.Price));
            }
            return result;
        }


        public void Dispose()
        {
            if (_connection.Connected)
                _connection.Disconnect();

            _connection.Dispose();

            _connection.OnConnected -= Connection_Connected;
            _connection.OnInAppBillingError -= Connection_OnInAppBillingError;
        }

        public async Task EnsureConnected()
        {
            var timeout = Task.Delay(5000);
            var result = await Task.WhenAny(new Task[] { _connectionTcs.Task, timeout }).ConfigureAwait(false);
            if (result == timeout)
                throw new InvalidOperationException(Localization.UserMessages.connection_required);
            await _connectionTcs.Task.ConfigureAwait(false);
        }

        private void Connection_OnInAppBillingError(InAppBillingErrorType error, string message)
        {
            _connectionTcs.SetException(new Exception(message));
        }

        private void Connection_Connected()
        {
            _connectionTcs.SetResult(true);
        }



        sealed class GetPurchasedOperation : IDisposable
        {
            private string _itemType;
            private InAppBillingHandler _billing;
            private TaskCompletionSource<IList<PurchasedItem>> _tcs;

            public GetPurchasedOperation(InAppBillingHandler billingHandler, string itemType)
            {
                _billing = billingHandler;
                _itemType = itemType;
                _tcs = new TaskCompletionSource<IList<PurchasedItem>>();

                _billing.OnPurchaseFailedValidation += OnPurchaseFailedValidation;
                _billing.OnInvalidOwnedItemsBundleReturned += OnInvalidOwnedItemsBundleReturned;
                _billing.InAppBillingProcesingError += InAppBillingProcessingError;
                _billing.OnProductPurchasedError += OnProductPurchasedError;

                System.Threading.Tasks.Task.Run(new Action(Execute));
            }

            public Task<IList<PurchasedItem>> Task
            {
                get { return _tcs.Task; }
            }

            private void Execute()
            {
                var purchasesResults = _billing.GetPurchases(_itemType);
                if (purchasesResults == null && !_tcs.Task.IsFaulted)
                {
                    _tcs.SetException(new Exception("Unable to perform the purchase"));
                    return;
                }

                if (purchasesResults != null && !_tcs.Task.IsFaulted)
                {
                    var r = new List<PurchasedItem>(purchasesResults.Count);
                    foreach (var item in purchasesResults)
                    {
                        //TODO: Filter the state
                        r.Add(CreateModel(item));
                    }
                    _tcs.SetResult(r);
                }
            }

            private void OnPurchaseFailedValidation(GPurchase purchase, string purchaseData, string purchaseSignature)
            {
                _tcs.SetException(new Exception("Unable to perform the purchase validation"));
            }

            private void OnInvalidOwnedItemsBundleReturned(Bundle ownedItems)
            {
                //_tcs.SetException(new Exception("The items are returned to perform the purchase validation"));
            }

            private void OnProductPurchasedError(int responseCode, string sku)
            {
                //throw new NotImplementedException();
            }
            private void InAppBillingProcessingError(string message)
            {
                _tcs.SetException(new Exception(message));
            }

            public void Dispose()
            {
                _billing.OnPurchaseFailedValidation -= OnPurchaseFailedValidation;
                _billing.OnInvalidOwnedItemsBundleReturned -= OnInvalidOwnedItemsBundleReturned;
                _billing.InAppBillingProcesingError -= InAppBillingProcessingError;
                _billing.OnProductPurchasedError -= OnProductPurchasedError;
            }
        }


        sealed class PurchaseOperation : IDisposable, IActivityHandler
        {
            // https://developer.android.com/google/play/billing/billing_reference.html
            // The purchase state of the order. Possible values are 0 (purchased), 1 (canceled), or 2 (refunded).
            private const int Purchase_Ok = 0;
            private const int Purchase_Canceled = 1;
            private const int Purchase_Refunded = 2;
            private const int IntentCode = 1001; // Xamarin.InAppBilling

            private IActivity _activity;
            private InAppBillingHandler _billing;
            private IInAppBillingService _billingService;
            private TaskCompletionSource<PurchasedItem> _tcs;

            public PurchaseOperation(IActivity activity, IInAppBillingService billingHandler, InAppBillingHandler billing, string productId, string payload)
            {
                _billingService = billingHandler;
                _billing = billing;
                _tcs = new TaskCompletionSource<PurchasedItem>();
                _activity = activity;

                _activity.AddActivityHandler(this);

#if DEBUG
                productId = "android.test.purchased";
                var list = billing.GetPurchases(ItemType.Product);
                foreach (var i in list)
                    billing.ConsumePurchase(i);
#endif

                try
                {
                    Bundle buyIntent = this._billingService.GetBuyIntent(Billing.APIVersion, _activity.Cast.PackageName, productId, ItemType.Product, payload);
                    int responseCodeFromBundle = buyIntent.GetResponseCodeFromBundle();
                    if (responseCodeFromBundle != BillingResult.OK)
                    {
                        OnProductPurchasedError(responseCodeFromBundle, productId);
                    }
                    else
                    {
                        //OnProductPurchased(productId);
                        PendingIntent pendingIntent = buyIntent.GetParcelable(Response.BuyIntent) as PendingIntent;
                        if (pendingIntent != null)
                        {
                            this._activity.Cast.StartIntentSenderForResult(pendingIntent.IntentSender, IntentCode, new Intent(), 0, 0, 0);
                        }
                    }
                }
                catch (Exception ex)
                {
                    InAppBillingProcessingError(string.Format("Error Buy Product: {0}", ex.ToString()));
                }
            }

            private void OnPurchaseFailedValidation(GPurchase purchase)
            {
                _tcs.SetException(new Exception($"Unable to validate the payment"));
            }

            public Task<PurchasedItem> Task
            {
                get { return _tcs.Task; }
            }

            bool IActivityHandler.OnActivityResult(int requestCode, Result resultCode, Intent data)
            {
                int response = 0;
                string text = string.Empty;
                string signature = string.Empty;
                if (requestCode != IntentCode || data == null)
                    return false;

                if (resultCode == Result.Canceled)
                {
                    _tcs.SetCanceled();
                    return true;
                }

                try
                {
                    response = data.GetReponseCodeFromIntent();
                    text = data.GetStringExtra(Response.InAppPurchaseData);
                    signature = data.GetStringExtra(Response.InAppDataSignature);
                }
                catch (Exception ex)
                {
                    InAppBillingProcessingError(string.Format("Error Decoding Returned Packet Information: {0}", ex.ToString()));
                    return true;
                }

                GPurchase purchase;
                try
                {
                    purchase = Newtonsoft.Json.JsonConvert.DeserializeObject<Purchase>(text);
                }
                catch (Exception ex2)
                {
                    InAppBillingProcessingError(string.Format("Unable to deserialize purchase: {0}\nError: {1}", text, ex2.ToString()));
                    purchase = new Purchase();
                    purchase.DeveloperPayload = text;
                    OnPurchaseFailedValidation(purchase);
                    return true;
                }

                try
                {
                    if (purchase.ProductId.Contains("android.test."))
                    {
                        OnProductPurchaseCompleted(response, purchase, text, signature);
                    }
                    else if (Security.VerifyPurchase(_billing.PublicKey, text, signature))
                    {
                        OnProductPurchaseCompleted(response, purchase, text, signature);
                    }
                    else
                    {
                        OnPurchaseFailedValidation(purchase);
                    }
                }
                catch (Exception ex3)
                {
                    InAppBillingProcessingError(string.Format("Error Decoding Returned Packet Information: {0}", ex3.ToString()));
                }

                return true;
            }

            private void OnProductPurchaseCompleted(int response, Purchase purchase, string signedData, string signature)
            {
                if (purchase.PurchaseState == Purchase_Ok)
                {
                    var purchasedItem = !string.IsNullOrEmpty(signedData) && !string.IsNullOrEmpty(signature)
                        ? CreateSignedModel(purchase, signedData, signature)
                        : CreateModel(purchase);

                    _tcs.SetResult(purchasedItem);
                    return;
                }

                if (purchase.PurchaseState == Purchase_Canceled)
                {
                    _tcs.SetCanceled();
                    return;
                }

                _tcs.SetException(new Exception($"In app billing response code: {purchase.PurchaseState} for {purchase.ProductId} and {purchase.PurchaseToken}"));
            }

            private void OnProductPurchasedError(int responseCode, string sku)
            {
                if (responseCode == BillingResult.ItemAlreadyOwned)
                {
                    _tcs.SetException(new Exception($"The item is already owned"));
                    return;
                }

                _tcs.SetException(new Exception($"In app billing response code: {responseCode} for SKU {sku}"));
            }

            private void InAppBillingProcessingError(string message)
            {
                // this if is intended to fix a bug in in app bulding
                if (message.Contains("Error Decoding Returned Packet Information: System.NullReferenceException"))
                {
                    _tcs.SetCanceled();
                    return;
                }

                _tcs.SetException(new Exception(message));
            }

            public void Dispose()
            {
                _activity.RemoveActivityHandler(this);

                /*_billingService.OnProductPurchasedError -= OnProductPurchasedError;
                _billingService.InAppBillingProcesingError -= InAppBillingProcessingError;
                _billingService.OnProductPurchaseCompleted -= OnProductPurchaseCompleted;*/
            }
        }

        private static PurchasedItem CreateModel(GPurchase purchase)
        {
            return new PurchasedItem(purchase.ProductId, purchase.PurchaseToken, purchase.DeveloperPayload, purchase.OrderId);
        }


        private static PurchasedItem CreateSignedModel(GPurchase purchase, string signedData, string signature)
        {
            return new SignedPurchasedItem(purchase.ProductId, purchase.PurchaseToken, purchase.DeveloperPayload, purchase.OrderId, signedData, signature);
        }
    }
}