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
using CleverEver.Storage;
using CleverEver;
using System.IO;
using Newtonsoft.Json;
using CleverEver.Logging;
using CleverEver.Analytics;
using System.Runtime.Serialization;
using Android.Util;
using CleverEver.Game.Model;

namespace CleverEver.Droid.Storage
{
    class FileQuestionsRepository : ILocalStorage<IEnumerable<Category>>
    {
        private string _file;
        private IExceptionLogger _logger;

        public FileQuestionsRepository(IExceptionLogger logger)
        {
            _logger = logger;
            _file = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "questions.file");
        }

        public IEnumerable<Category> Find()
        {
            var serializer = new Newtonsoft.Json.JsonSerializer();
            try
            {
                using (var reader = new StreamReader(_file))
                using (var jsonReader = new JsonTextReader(reader))
                {
                    var serialized = serializer.Deserialize<List<CategoryDTO>>(jsonReader);

                    var result = new Category[serialized.Count];
                    for (int i = 0; i < serialized.Count; i++)
                        result[i] = serialized[i].ToModel();
                    return result;
                }
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (Exception ex)
            {
                Log.Error("FileQuestionsRepository", ex.StackTrace);
                _logger.LogError(ex, "Unable to deserialize the questions");
                return null;
            }
        }

        public void Set(IEnumerable<Category> data)
        {
            var serializer = new Newtonsoft.Json.JsonSerializer();
            try
            {
                using (var writer = new StreamWriter(_file))
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    var serialize = new List<CategoryDTO>();
                    foreach (var item in data)
                        serialize.Add(new CategoryDTO(item));
                    serializer.Serialize(jsonWriter, serialize, typeof(List<CategoryDTO>));
                }
            }
            catch (Exception ex)
            {
                Log.Error("FileQuestionsRepository", ex.StackTrace);
                _logger.LogError(ex, "Unable to serialize the questions");
                throw;
            }
        }


        struct CategoryDTO
        {
            [JsonProperty("t")]
            public string Text { get; set; }

            [JsonProperty("s")]
            public QuestionSetDTO[] Sets { get; set; }

            public CategoryDTO(Category category)
            {
                Text = category.Text;

                var sets = new QuestionSetDTO[category.Sets.Count];
                for (int i = 0; i < category.Sets.Count; i++)
                    sets[i] = new QuestionSetDTO(category.Sets[i]);
                Sets = sets;
            }

            public Category ToModel()
            {
                var sets = new QuestionSet[Sets.Length];
                for (int i = 0; i < Sets.Length; i++)
                    sets[i] = Sets[i].ToModel();
                return new Category(Text, sets);
            }
        }

        struct QuestionSetDTO
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("pi")]
            public string PacketId { get; set; }

            [JsonProperty("Level")]
            public int? Level { get; set; }

            [JsonProperty("n")]
            public string Name { get; set; }

            [JsonProperty("q")]
            public QuestionDTO[] Questions { get; set; }

            public QuestionSetDTO(QuestionSet set)
            {
                Id = set.Id;
                PacketId = set.Packet == null ? null : set.Packet.Id;
                Level = set.Level;
                Name = set.Name;

                var questions = new QuestionDTO[set.Questions.Length];
                for (int i=0; i<set.Questions.Length; i++)
                    questions[i] = new QuestionDTO(set.Questions[i]);
                Questions = questions;
            }

            public QuestionSet ToModel()
            {
                var questions = new Question[Questions.Length];
                for (int i = 0; i < Questions.Length; i++)
                    questions[i] = Questions[i].ToModel();
                return new QuestionSet(Id, CreatePacket(PacketId), Name, Level, questions);
            }

            private static Packet CreatePacket(string packetId)
            {
                if (string.IsNullOrEmpty(packetId))
                    return null;
                return new Packet(packetId);
            }
        }

        struct QuestionDTO
        {
            [JsonProperty("t")]
            public string Text { get; set; }

            [JsonProperty("a")]
            public string[] Answers { get; set; }

            [JsonProperty("c")]
            public int CorrectIndex { get; set; }

            public QuestionDTO(Question question)
            {
                Text = question.Text;
                Answers = question.Answers;
                CorrectIndex = question.CorrectIndex;
            }

            public Question ToModel()
            {
                return new Question(Text, Answers, CorrectIndex);
            }
        }
    }
}