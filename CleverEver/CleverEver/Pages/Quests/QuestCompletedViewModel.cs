using CleverEver.Game;
using CleverEver.Phone.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CleverEver.Pages.Quests
{
    class QuestCompletedViewModel : BaseViewModel
    {
        private IQuest _quest;
        private JsonContestSecret _contestSecret;
        private CleverEver.Game.Repositories.IContestRepository _contestRepository;

        private string _email;
        public string Email
        {
            get { return _email; }
            set { SetField(ref _email, value); }
        }

        public Command RedeemCommand { get; }

        private QuestCompletedViewModel(CleverEver.Game.Repositories.IContestRepository contestRepository)
        {
            _contestRepository = contestRepository;

            RedeemCommand = new Command(() => Redeem().ConfigureAwait(false));
        }

        public static bool TryCreate(byte[] claimResult, IQuest quest, out QuestCompletedViewModel vm)
        {
            JsonContestSecret contestSecret;
            if (!JsonContestSecret.TryCreate(claimResult, out contestSecret))
            {
                vm = null;
                return false;
            }

            vm = new QuestCompletedViewModel(
                Composition.DependencyContainer.Resolve<CleverEver.Game.Repositories.IContestRepository>()
            );
            vm._quest = quest;
            vm._contestSecret = contestSecret;
            return true;
        }

        public async Task Redeem()
        {
            var redeemed = await _contestRepository.Redeem(_quest, "", "");
            if (redeemed)
            {
                await Navigation.PushAsync(new CleverEver.Pages.Quests.QuestCompleted()).ConfigureAwait(false);
                return;
            }

            //_dialogs.AlertAsync("You have completed a quest");
            throw new NotImplementedException();
        }
    }

    public class JsonContestSecret
    {
        [JsonProperty("key")]
        public string Key { get; }

        private JsonContestSecret(string key)
        {
            Key = key;
        }

        public static bool TryCreate(byte[] content, out JsonContestSecret contest)
        {
            try
            {
                using (var ms = new System.IO.MemoryStream(content))
                using (var sreader = new System.IO.StreamReader(ms))
                using (var reader = new JsonTextReader(sreader))
                {
                    contest = JsonSerializer.CreateDefault().Deserialize<JsonContestSecret>(reader);
                    return true;
                }
            }
            catch (Exception ex)
            {
                contest = null;
                return false;
            }
        }

        public CleverEver.Game.Model.ContestSecret ToModel()
        {
            return new CleverEver.Game.Model.ContestSecret(Key);
        }
    }
}
