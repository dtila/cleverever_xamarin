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
using CleverEver.Game;
using CleverEver.Game.Achievements;
using Android.Gms.Games.Event;
using Android.Gms.Common.Apis;

namespace CleverEver.Droid.Connectivity.Multiplayer.Contests
{
    class FinishAnyRomanianLesson : IGameEventHandler
    {
        private bool _isInterested;
        private GoogleApiClient _client;

        public FinishAnyRomanianLesson(GoogleApiClient client)
        {
            _isInterested = false;
            _client = client;
        }

        public void Handle(GameOverEventArgs e)
        {
            if (e.PlayerAnswers.Count == 0)
                return;

            var playerAnswers = e.PlayerAnswers.Single(li => li.Participant.Equals(e.CurrentParticipant));
            var correctAnswersCount = playerAnswers.Answers.Count(li => !li.IsSkipped && li.IsCorrect);
            if (correctAnswersCount >= 0.9 * e.TotalQuestions)
            {
                //Android.Gms.Games.GamesClass.Events.Increment(_client, Application.Context.GetString(Resource.String.event_finish_the_basic_language_questions), correctAnswersCount);
            }
        }

        public void Handle(QuestionAnswer answer)
        {
        }

        public void Handle(GameErrorEventArgs e)
        {
        }

        public void Handle(GameStartedEventArgs e)
        {
            _isInterested = e.SetName.ToLower() == "romana";
        }
    }
}