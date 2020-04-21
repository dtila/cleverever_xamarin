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
using CleverEver.Droid.Messaging;
using System.Threading.Tasks;
using CleverEver.Game;

namespace CleverEver.Droid.Connectivity.Google.Operations
{
    sealed class SelectQuestOperation : Java.Lang.Object, Messaging.IActivityHandler
    {
        private const int ID = 23322;

        private GoogleServer _server;
        private TaskCompletionSource<IQuest> _tcs;

        public Task<IQuest> Task => _tcs.Task;

        public SelectQuestOperation(GoogleServer server)
        {
            _server = server;
            _server.Activity.AddActivityHandler(this);
            _tcs = new TaskCompletionSource<IQuest>();

            var intent = Android.Gms.Games.GamesClass.Quests.GetQuestsIntent(_server.Client, Android.Gms.Games.Quest.Quests.SelectAllQuests.ToArray());
            _server.Activity.Cast.StartActivityForResult(intent, ID);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _server.Activity.RemoveActivityHandler(this);
            }

            base.Dispose(disposing);
        }

        bool IActivityHandler.OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode != ID)
                return false;

            if (resultCode == Result.Canceled)
            {
                _tcs.SetCanceled();
                return true;
            }

            if (resultCode == Result.Ok)
            {
                var selected = data.GetParcelableExtra(Android.Gms.Games.Quest.Quests.ExtraQuest) as Android.Gms.Games.Quest.QuestEntity;
                _tcs.SetResult(new Connectivity.Google.Multiplayer.GoogleServerQuestSupport.GoogleQuest(_server, selected));
                return true;
            }

            return false;
        }
    }
}