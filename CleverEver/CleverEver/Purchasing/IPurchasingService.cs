using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Purchasing
{
    public interface IPurchasingService
    {
        Task<PurchasedItem> PurchaseAsync(string productId, string payload);
        Task<IList<PurchasedItem>> GetPurchasedItems();
        Task<IList<PurchasableItem>> GetAvailableItems();
    }


    public class PurchasedItem
    {
        public string ProductId { get; }
        public string PurchaseToken { get; }
        public string DeveloperPayload { get; }
        public string TransactionId { get; }

        public PurchasedItem(string productId, string purchaseToken, string developerPayload, string transactionId)
        {
            ProductId = productId;
            PurchaseToken = purchaseToken;
            DeveloperPayload = developerPayload;
            TransactionId = transactionId;
        }
    }


    /// <summary>
    /// Represents a signed purchased which can be verified by someones else if the keys are known
    /// </summary>
    public class SignedPurchasedItem : PurchasedItem
    {
        public string SignedData { get; }
        public string Signature { get; }

        public SignedPurchasedItem(string productId, string purchaseToken, string developerPayload, string transactionId, string signedData, string signature)
            : base(productId, purchaseToken, developerPayload, transactionId)
        {
            SignedData = signedData;
            Signature = signature;
        }
    }

    public class PurchasableItem
    {
        public string ProductId { get; }
        public string Price { get; }

        public PurchasableItem(string id, string price)
        {
            ProductId = id;
            Price = price;
        }
    }
}
