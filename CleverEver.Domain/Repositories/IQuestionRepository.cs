using CleverEver.Game.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Game.Repositories
{
    public interface IQuestionRepository
    {
        Task<IList<Category>> Purchase(string packetId, string purchaseToken, string payload);
        Task<IList<Category>> Purchase(string packetId, string purchaseToken, string payload, string receipt, string signature);

        Task<IList<Category>> GetAvailableCategories();
    }
}
