using Acr.UserDialogs;
using CleverEver.Game.Model;
using CleverEver.Game.Repositories;
using CleverEver.Pages.Game.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Services
{
    public class DebuggingQuestionService : IQuestionRepository
    {
        public async Task<QuestionSet> PurchaseSet(string setId, string purchaseToken, string payload)
        {
            return new QuestionSet(setId, "My purchased set", new Question[]
            {
                new Question("The purchased question",  new [] { "Super", "Rau", "Super", "Da" }, 1),
                new Question("The purchased Question 2 from server",  new [] { "Bine", "DAAAA", "Super", "Da" }, 1),
            });
        }

        public async Task<QuestionSet> PurchaseSet(string setId, string purchaseToken, string payload, string signedData, string signature)
        {
            return new QuestionSet(setId, "My signed purchased set", new Question[]
            {
                new Question("The signed purchased question",  new [] { "Super", "Rau", "Super", "Da" }, 1),
                new Question("The signed purchased Question 2 from server",  new [] { "Bine", "DAAAA", "Super", "Da" }, 1),
            });
        }

        public Task<IList<Category>> Purchase(string packetId, string purchaseToken, string payload)
        {
            return GetAvailableCategories();
        }

        public Task<IList<Category>> Purchase(string packetId, string purchaseToken, string payload, string signedData, string signature)
        {
            return GetAvailableCategories();
        }

        public async Task<IList<Category>> GetAvailableCategories()
        {
            IList<Category> list = new List<Category>();

            return null;
        }
    }
}
