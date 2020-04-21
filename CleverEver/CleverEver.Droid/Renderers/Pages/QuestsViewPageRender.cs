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
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using CleverEver.Pages.Game;
using CleverEver.Droid.Messaging;

[assembly: ExportRenderer(typeof(Quests), typeof(CleverEver.Droid.Renderers.Pages.QuestsViewPageRender))]

namespace CleverEver.Droid.Renderers.Pages
{
    public class QuestsViewPageRender : PageRenderer, IActivityHandler
    {
        private const int ID = 23322;
        private Connectivity.Google.GoogleServer _server;

        public QuestsViewPageRender()
        {
            _server = Composition.DependencyContainer.Resolve<Game.IGameServer>() as Connectivity.Google.GoogleServer;
            Visibility = ViewStates.Gone;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                _server.Activity.AddActivityHandler(this);
            }
        }

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();

            var intent = Android.Gms.Games.GamesClass.Quests.GetQuestsIntent(_server.Client, Android.Gms.Games.Quest.Quests.SelectAllQuests.ToArray());
            _server.Activity.Cast.StartActivityForResult(intent, ID);
            this.Element.Navigation.PopAsync(false);
        }

        public bool OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
        {
            if (requestCode != ID)
                return false;

            var selected = data.GetParcelableExtra(Android.Gms.Games.Quest.Quests.ExtraQuest) as Android.Gms.Games.Quest.QuestEntity;
            var q = new Connectivity.Google.Multiplayer.GoogleServerQuestSupport.GoogleQuest(_server, selected);

            return resultCode == Android.App.Result.Ok;
        }
    }
}