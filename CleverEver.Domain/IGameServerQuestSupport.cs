using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Game
{
    public interface IGameServerQuestSupport
    {
        event EventHandler<QuestCompletedEventArgs> QuestCompleted;

        Task<IList<IQuest>> GetOpenQuestsAsync();

        Task<IQuest> Select();
    }

    public enum QuestStatus
    {
        Accepted,
        Completed,
        Open,
        Upcoming
    }

    public enum QuestMilestoneStatus
    {
        /// <summary>
        /// The milestone has not been started by the user
        /// </summary>
        NotStarted,
        /// <summary>
        /// The milestone has not been completed by the user
        /// </summary>
        NotCompleted,
        /// <summary>
        /// The milestone has beem completed by the user
        /// </summary>
        Completed,
        /// <summary>
        /// The quest is completed and claimed
        /// </summary>
        Claimed
    }

    public interface IQuest
    {
        QuestStatus Status { get; }
        QuestMilestoneStatus MilestoneStatus { get; }
        Task<byte[]> Claim();
    }

    public class QuestCompletedEventArgs  : EventArgs
    {
        public IQuest Quest { get; }
        public IPlayer Player { get; }

        public QuestCompletedEventArgs(IQuest quest)
        {
            Quest = quest;
        }
    }
}
