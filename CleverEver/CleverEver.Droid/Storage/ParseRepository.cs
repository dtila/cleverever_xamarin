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
using CleverEver.Game.Repositories;
using Parse;
using System.IO;
using Newtonsoft.Json;
using CleverEver.Game.Model;
using CleverEver.Game;

namespace CleverEver.Droid.Storage
{
    class ParseRepository : IQuestionRepository, IContestRepository
    {
        public async Task<IList<Category>> GetAvailableCategories()
        {
            var answersText = await Parse.ParseCloud.CallFunctionAsync<string>("userQuestions", new Dictionary<string, object>()).ConfigureAwait(false);
            if (string.IsNullOrEmpty(answersText))
                throw new InvalidOperationException("Empty server response");

            var answers = Newtonsoft.Json.JsonConvert.DeserializeObject<IList<ParseCategoryDTO>>(answersText);

            return CreateModel(answers);
        }

        public Task<IList<Category>> Purchase(string packetId, string purchaseToken, string payload)
        {
            return Purchase(packetId, purchaseToken, payload, null, null);
        }

        public async Task<IList<Category>> Purchase(string packetId, string purchaseToken, string payload, string receipt, string signature)
        {
            var authenticationService = Composition.DependencyContainer.Resolve<Authentication.ParseAuthentication>();

            if (!authenticationService.IsAuthenticated)
                throw new InvalidOperationException("You need to authentication");

            var @params = new Dictionary<string, object>
            {
                { "packetId", packetId },
                { "store", "google" },
                { "receipt", receipt },
                { "signature", signature },
            };

            var answersText = await Parse.ParseCloud.CallFunctionAsync<string>("purchasePacket", @params).ConfigureAwait(false);
            if (string.IsNullOrEmpty(answersText))
                throw new InvalidOperationException("Empty server response");

            var answers = Newtonsoft.Json.JsonConvert.DeserializeObject<IList<ParseCategoryDTO>>(answersText);

            return CreateModel(answers);
        }

        private static IList<Category> CreateModel(IList<ParseCategoryDTO> categories)
        {
            var list = new Category[categories.Count];
            for (var i = 0; i < categories.Count; i++)
                list[i] = categories[i].ToModel();
            return list;
        }

        Task<bool> IContestRepository.Redeem(IQuest quest, string secret, string email)
        {
            throw new NotImplementedException();
        }

        struct ParseCategoryDTO
        {
            [JsonProperty("Id")]
            public string Id { get; set; }

            [JsonProperty("Name")]
            public string Name { get; set; }

            [JsonProperty("Sets")]
            public ParseQuestionSetDTO[] Sets { get; set; }

            public Category ToModel()
            {
                try
                {
                    var sets = new QuestionSet[Sets.Length];
                    for (var i = 0; i < Sets.Length; i++)
                        sets[i] = Sets[i].ToModel();

                    return new Category(Name, sets);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Unable to create the model for category {Id ?? "null"}", ex);
                }
            }
        }

        struct ParseQuestionSetDTO
        {
            [JsonProperty("Id")]
            public string Id { get; set; }

            [JsonProperty("PacketId")]
            public string PacketId { get; set; }

            [JsonProperty("Level")]
            public int? Level { get; set; }

            [JsonProperty("Name")]
            public string Name { get; set; }

            [JsonProperty("Questions")]
            public ParseQuestionDTO[] Questions { get; set; }

            public QuestionSet ToModel()
            {
                try
                {
                    var questions = new Question[Questions.Length];
                    for (int i = 0; i < questions.Length; i++)
                        questions[i] = Questions[i].ToModel();

                    Packet packet = null;
                    if (!string.IsNullOrEmpty(PacketId))
                        packet = new Packet(PacketId);

                    return new QuestionSet(Id, packet, Name, Level, questions);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Unable to create the model for question set {Id ?? "null"}", ex);
                }
            }
        }

        struct ParseQuestionDTO
        {
            [JsonProperty("Id")]
            public string Id { get; set; }

            [JsonProperty("CorrectIndex")]
            public int[] CorrectIndex { get; set; }

            [JsonProperty("Text")]
            public string Text { get; set; }

            [JsonProperty("Options")]
            public string[] Options { get; set; }

            public Question ToModel()
            {
                try
                {
                    return new Question(Text, Options, CorrectIndex[0] - 1);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Unable to create the model for question {Id ?? "null"}", ex);
                }
            }
        }
    }
}