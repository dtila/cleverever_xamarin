using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Gms.Analytics;
using CleverEver.Analytics;
using System.Runtime.CompilerServices;
using CleverEver.Purchasing;
using System.Threading.Tasks;
using CleverEver.Droid.Messaging;
using CleverEver.Authentication;
using CleverEver.Logging;
using CleverEver.Game.Model;

namespace CleverEver.Droid.Analytics
{
    class GoogleAnalyticsService : IAnalyticsService
    {
        private const string ClientIdKey = "clientId";

        private const string UsageCategory = "Usage";
        private const string PlayCategory = "Playing";
        private const string PurchasingCategory = "Purchasing";
        private const string WindowOpenedActionName = "WindowOpened";

        private Tracker _tracker;
        private Task<Instalation> _getInstallationTask;

        public GoogleAnalyticsService(IActivity activity, IAuthenticationService parseAuthentication)
        {
            _getInstallationTask = parseAuthentication.GetInstalationAsync();

            var analytics = GoogleAnalytics.GetInstance(activity.Cast);
            analytics.SetLocalDispatchPeriod(10);

            _tracker = analytics.NewTracker("UA-92287269-1");
            _tracker.EnableExceptionReporting(true);
            _tracker.EnableAdvertisingIdCollection(true);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        public void LogError(Exception ex, string message = null, [CallerMemberName] string method = null)
        {
            LogError(ex, false);
        }

        public void SetCurrentScreen(string name)
        {
            _tracker.SetScreenName(name);
        }

        public Task ApplicationCreated()
        {
            var @params = new HitBuilders.EventBuilder()
                .SetCategory(UsageCategory)
                .SetAction(WindowOpenedActionName)
                .SetLabel("Application")
                .Build();

            return Send(@params);
        }

        public Task HomeOpened()
        {
            var @params = new HitBuilders.EventBuilder()
                .SetCategory(UsageCategory)
                .SetAction(WindowOpenedActionName)
                .SetLabel("Home")
                .Build();

            return Send(@params);
        }

        public Task TrackPurchaseCompletedAsync(Packet packet, PurchasedItem purchasedItem)
        {
            var @params = new HitBuilders.EventBuilder()
                .SetCategory(PurchasingCategory)
                .SetAction("Purchased")
                .SetLabel(packet.Id)
                .Build();

            return Send(@params);
        }

        public Task TrackPurchaseCancelledAsync(Packet packet, TimeSpan timeSpent)
        {
            var @event = new HitBuilders.EventBuilder()
                .SetCategory(PurchasingCategory)
                .SetAction("PurchasedCancelled")
                .SetLabel(packet.Id)
                .SetValue((int)timeSpent.TotalSeconds);

            @event.Set("TimeSpentSeconds", $"{timeSpent.TotalSeconds}");

            return Send(@event.Build());
        }

        public Task TrackGamePlayed(SingleGamePlayed events)
        {
            var @event = new HitBuilders.EventBuilder()
                .SetCategory(PlayCategory)
                .SetAction("SingleGamePlayed")
                .SetLabel($"Category: {events.UserSelection.Category.Text}, Set: {GetQuestionSetName(events.UserSelection.QuestionSet)}")
                .SetValue((int)events.Duration.TotalSeconds);

            @event.Set("TimeSpentSeconds", $"{events.Duration.TotalSeconds}");
            @event.Set("Category", events.UserSelection.Category.Text);
            @event.Set("Set", GetQuestionSetName(events.UserSelection.QuestionSet));

            var @timeEvent = new HitBuilders.TimingBuilder()
                .SetCategory(PlayCategory)
                .SetVariable($"Category: {events.UserSelection.Category.Text}, Set: {GetQuestionSetName(events.UserSelection.QuestionSet)}")
                .SetLabel($"SingleGamePlayed")
                .SetValue((int)events.Duration.TotalMilliseconds);

            return Task.WhenAll(Send(@event.Build()), Send(@timeEvent.Build()));
        }

        public Task TrackGamePlayed(HostedNetworkGamePlayed events)
        {
            var @event = new HitBuilders.EventBuilder()
                .SetCategory(PlayCategory)
                .SetAction("HostedGamePlayed")
                .SetLabel($"Category: {events.UserSelection.Category.Text}, Set: {GetQuestionSetName(events.UserSelection.QuestionSet)}")
                .SetValue((int)events.Duration.TotalSeconds);

            @event.Set("TimeSpentSeconds", $"{events.Duration.TotalSeconds}");
            @event.Set("Category", events.UserSelection.Category.Text);
            @event.Set("Set", GetQuestionSetName(events.UserSelection.QuestionSet));

            var @timeEvent = new HitBuilders.TimingBuilder()
              .SetCategory(PlayCategory)
              .SetVariable($"Category: {events.UserSelection.Category.Text}, Set: {GetQuestionSetName(events.UserSelection.QuestionSet)}")
              .SetLabel($"HostedGamePlayed")
              .SetValue((int)events.Duration.TotalMilliseconds);

            return Task.WhenAll(Send(@event.Build()), Send(@timeEvent.Build()));
        }

        public Task TrackGamePlayed(JoinedNetworkGamePlayed events)
        {
            var @event = new HitBuilders.EventBuilder()
                .SetCategory(PlayCategory)
                .SetAction("JoinedGamePlayed")
                .SetValue((int)events.Duration.TotalSeconds);

            @event.Set("TimeSpentSeconds", $"{events.Duration.TotalSeconds}");

             var @timeEvent = new HitBuilders.TimingBuilder()
              .SetCategory(PlayCategory)
              .SetVariable("JoinedGamePlayed")
              .SetValue((int)events.Duration.TotalMilliseconds);

            return Task.WhenAll(Send(@event.Build()), Send(@timeEvent.Build()));
        }

        private async Task Send(IDictionary<string, string> parameters)
        {
            await EnsureLogin().ConfigureAwait(false);
#if !DEBUG
            _tracker.Send(parameters);
#endif
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            if (ex == null)
                return;

            LogError(ex, e.IsTerminating);
        }

        private void LogError(Exception e, bool fatal)
        {
            Android.Util.Log.Error("CleverEver", e.Message);
            Android.Util.Log.Error("CleverEver", e.StackTrace);
            LogError(e, e.Message, fatal);
        }

        private void LogError(Exception e, string message, bool fatal)
        {
            var @event = new HitBuilders.ExceptionBuilder()
                .SetDescription(message ?? "")
                .SetFatal(fatal);

            @event.Set("CallingMethod", e.Source ?? "");
            @event.Set("StackTrace", e.StackTrace);
            @event.Set("ExceptionMessage", e.Message);

            _tracker.Send(@event.Build());
        }

        private async Task EnsureLogin()
        {
            var installation = await _getInstallationTask.ConfigureAwait(false);
            if (string.IsNullOrEmpty(_tracker.Get(ClientIdKey)))
            {
                _tracker.Set(ClientIdKey, installation.Id);
            }
        }

        private static string GetQuestionSetName(QuestionSet set)
        {
            if (set.Level == null)
                return set.Name;
            return $"{set.Name} (level {set.Level})";
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
        }
    }
}