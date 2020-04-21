using CleverEver.Game.Model;
using CleverEver.Logging;
using CleverEver.Pages.Game;
using CleverEver.Purchasing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Analytics
{
    public interface IAnalyticsService
    {
        Task ApplicationCreated();
        Task HomeOpened();

        void SetCurrentScreen(string name);
        Task TrackPurchaseCompletedAsync(Packet packet, PurchasedItem purchasedItem);
        Task TrackPurchaseCancelledAsync(Packet packet, TimeSpan timeSpent);

        Task TrackGamePlayed(SingleGamePlayed events);
        Task TrackGamePlayed(HostedNetworkGamePlayed events);
        Task TrackGamePlayed(JoinedNetworkGamePlayed events);
    }


    public struct SingleGamePlayed
    {
        public TimeSpan Duration { get; }
        public UserSelection UserSelection { get; }

        public SingleGamePlayed(TimeSpan duration, UserSelection userSelection)
        {
            Duration = duration;
            UserSelection = userSelection;
        }
    }

    public struct HostedNetworkGamePlayed
    {
        public UserSelection UserSelection { get; }
        public TimeSpan Duration { get; }

        public HostedNetworkGamePlayed(UserSelection userSelection)
        {
            UserSelection = userSelection;
        }
    }

    public struct JoinedNetworkGamePlayed
    {
        public TimeSpan Duration { get; }

        public JoinedNetworkGamePlayed(TimeSpan duration)
        {
            Duration = duration;
        }
    }
}
