using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Game.Repositories
{
    public interface IContestRepository
    {
        /// <summary>
        /// Redeem a quest
        /// </summary>
        /// <param name="quest"></param>
        /// <param name="secret"></param>
        /// <param name="email"></param>
        /// <returns>True or false if the quest has been redeemed</returns>
        Task<bool> Redeem(IQuest quest, string secret, string email);
    }
}
