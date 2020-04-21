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
using CleverEver.Game;
using CleverEver.Droid.Connectivity.Google;
using Android.Gms.Games.Quest;

namespace CleverEver.Droid.Connectivity.Google.Multiplayer
{
    class GoogleServerQuestSupport : Java.Lang.Object, IGameServerQuestSupport, IQuestUpdateListener
    {
        private GoogleServer _gameServer;

        public event EventHandler<QuestCompletedEventArgs> QuestCompleted;

        public GoogleServerQuestSupport(GoogleServer gameServer)
        {
            _gameServer = gameServer;

            _gameServer.ClientConnected += GameServer_ClientConnected;
            _gameServer.ClientDisconnected += GameServer_ClientDisconnected;
        }

        public async Task<IList<Game.IQuest>> GetOpenQuestsAsync()
        {
            await _gameServer.EnsureConnected().ConfigureAwait(false);
            var quests = await Android.Gms.Games.GamesClass.Quests.LoadAsync(_gameServer.Client,
                new[] { Quests.SelectOpen, Quests.SelectUpcoming, Quests.SortOrderEndingSoonFirst }, 1, false).ConfigureAwait(false);

            var results = new List<Game.IQuest>();
            foreach (var quest in quests.Quests.ToEnumerable<Android.Gms.Games.Quest.IQuest>())
                results.Add(new GoogleQuest(_gameServer, quest));
            return results;
        }

        void IQuestUpdateListener.OnQuestCompleted(Android.Gms.Games.Quest.IQuest quest)
        {
            QuestCompleted?.Invoke(this, new QuestCompletedEventArgs(new GoogleQuest(_gameServer, quest)));
        }

        public async Task<Game.IQuest> Select()
        {
            using (var operation = new Operations.SelectQuestOperation(_gameServer))
                return await operation.Task;
        }

        private void GameServer_ClientConnected(object sender, GoogleServer.GoogleClientConnectedEventArgs e)
        {
            Android.Gms.Games.GamesClass.Quests.RegisterQuestUpdateListener(_gameServer.Client, this);
        }

        private void GameServer_ClientDisconnected(object sender, EventArgs e)
        {
            var client = _gameServer.Client;
            if (client != null && client.Handle != IntPtr.Zero && client.IsConnected)
            {
                Android.Gms.Games.GamesClass.Quests.UnregisterQuestUpdateListener(client);
            }
        }


        public class GoogleQuest : Game.IQuest
        {
            private GoogleServer _gameServer;
            private string _milestoneId;

            public string Id { get; }
            public QuestMilestoneStatus MilestoneStatus { get; }
            public QuestStatus Status { get; }

            public GoogleQuest(GoogleServer gameServer, Android.Gms.Games.Quest.IQuest quest)
            {
                _gameServer = gameServer;
                Id = quest.QuestId;

                var state = quest.State;
                var state1 = quest.CurrentMilestone.State;

                if (quest.CurrentMilestone != null)
                    _milestoneId = quest.CurrentMilestone.MilestoneId;
                MilestoneStatus = GetMilestoneStatus(quest.CurrentMilestone.State);
                Status = GetStatus(quest.State);
            }

            public async Task<byte[]> Claim()
            {
                if (string.IsNullOrEmpty(_milestoneId))
                    throw new InvalidOperationException($"The quest {Id} can not be claimed because it does not have a milestone");

                using (var claim = await Android.Gms.Games.GamesClass.Quests.ClaimAsync(_gameServer.Client, Id, _milestoneId))
                    return claim.Milestone.GetCompletionRewardData();
            }

            private static QuestStatus GetStatus(int status)
            {
                if (status == Quest.StateAccepted)
                    return QuestStatus.Accepted;
                if (status == Quest.StateCompleted)
                    return QuestStatus.Completed;
                if (status == Quest.StateOpen)
                    return QuestStatus.Open;
                if (status == Quest.StateUpcoming)
                    return QuestStatus.Upcoming;

                throw new NotImplementedException($"Unable to convert quest status from {status}");
            }

            private static QuestMilestoneStatus GetMilestoneStatus(int milestoneStatus)
            {
                if (milestoneStatus == Android.Gms.Games.Quest.Milestone.StateClaimed)
                    return QuestMilestoneStatus.Claimed;
                if (milestoneStatus == Android.Gms.Games.Quest.Milestone.StateCompletedNotClaimed)
                    return QuestMilestoneStatus.Completed;
                if (milestoneStatus == Android.Gms.Games.Quest.Milestone.StateNotCompleted)
                    return QuestMilestoneStatus.NotCompleted;
                if (milestoneStatus == Android.Gms.Games.Quest.Milestone.StateNotStarted)
                    return QuestMilestoneStatus.NotStarted;

                throw new InvalidOperationException($"Unable to create the QuestMilestoneStatus from the value {milestoneStatus}");
            }
        }
    }
}