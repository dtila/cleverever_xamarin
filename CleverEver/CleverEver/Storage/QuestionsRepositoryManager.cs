using CleverEver.Game.Model;
using CleverEver.Phone.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Storage
{
    /// <summary>
    /// This is the local storage question repository manager. It's purpose is to download the needed 
    /// </summary>
    public class QuestionsRepositoryManager
    {
        private const string LastDownloadedSettingsKey = "questions.last_downloaded";
        private const int DaysExpirationLocalCache = 6;

        private bool _isRefreshing;
        private int _version;
        private Acr.Settings.ISettings _settings;
        private ILocalStorage<IEnumerable<Category>> _inner;
        private Game.Repositories.IQuestionRepository _repository;

        private readonly static Category[] FreeCategories;


        static QuestionsRepositoryManager()
        {
            var packet1 = new Packet("qweasd123");
            var packet2 = new Packet("qweasd124");

            FreeCategories = new[] {
#if DEBUG
                new Category("Test", new []
                {
                    new QuestionSet("test11", packet1, "Very long", null, new Question[] { }),
                    new QuestionSet("qweasd1233-no-packet", packet1, "Payable2", null, new Question[] { }),
                    new QuestionSet("test1", packet2, "Payable2", null, new Question[] { }),

                    new QuestionSet("romana.antonim2e", "Super long", new Question[]
                        {
                            new Question("Alege antonimul termenului zgârcit!", new [] { "lorem ipsulm lorem upasd lorem ip sdassd sdsd", "avasd a asdsdsdssda sdaasd dsasdasdsdsdd sdr", "lsdd sdas dasd asdas ssds dsd ss", "darnasssss sd sds sssssssssssssssssssd sd sdic" }, 3),
                            new Question("Alege antonimul cuvântului caduc!",  new [] { "subred, pieritor", "dur, grosolan", "gentil", "trainic, durabil" }, 3),
                            new Question("Selectează antonimul termenului tardiv!",  new [] { "a întârzia", "întârziere", "târziu", "timpuriu" }, 3),
                        }),
                }),
                new Category("Test 2", new []
                    {
                        new QuestionSet("biologie.3", packet1, "Payable", null, new Question[] { }),
                        new QuestionSet("chimie.8", "Clasa 8", new Question[]
                            {
                                new Question("How are you", new [] { "Correct", "Rau", "Super", "Da" }, 0),
                                new Question("Question 2",  new [] { "Correct", "Bine5678901234567890 super", "Super", "Da" }, 0),
                                new Question("Question 3",  new [] { "Correct", "Bine5678901234567890 super", "Super", "Da" }, 0),
                                new Question("Question 4",  new [] { "Correct", "Bine5678901234567890 super", "Super", "Da" }, 0),
                                new Question("Question 2",  new [] { "Correct", "Bine5678901234567890 super", "Super", "Da" }, 0),
                                new Question("Question 2",  new [] { "Correct", "Bine5678901234567890 super", "Super", "Da" }, 0),
                                new Question("Question 2",  new [] { "Correct", "Bine5678901234567890 super", "Super", "Da" }, 0),
                            })
                    }),
#else
                new Category("Romana", new []
                {
                    new QuestionSet("romana.antonime", "Antonime", new Question[]
                        {
                            new Question("Alege antonimul termenului zgârcit!", new [] { "oneros", "avar", "las", "darnic" }, 3),
                            new Question("Alege antonimul cuvântului caduc!",  new [] { "subred, pieritor", "dur, grosolan", "gentil", "trainic, durabil" }, 3),
                            new Question("Selectează antonimul termenului tardiv!",  new [] { "a întârzia", "întârziere", "târziu", "timpuriu" }, 3),
                        })
                }),
#endif
            };
        }

        public event EventHandler<QuestionRepositoryChangedEventArgs> RepositoryChanged;
        public event EventHandler IsRefreshingChanged;


        public QuestionsRepositoryManager(ILocalStorage<IEnumerable<Category>> inner, Acr.Settings.ISettings settings)
        {
            _isRefreshing = false;
            _inner = inner;
            _settings = settings;
        }

        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            private set
            {
                if (value == _isRefreshing)
                    return;
                _isRefreshing = value;
                IsRefreshingChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int Version
        {
            get { return _version; }
        }

        public DateTime LastUpdated
        {
            get { return _settings.Get<DateTime>(LastDownloadedSettingsKey); }
            private set { _settings.Set(LastDownloadedSettingsKey, value); }
        }

        public IEnumerable<Category> Find()
        {
            var inner = _inner.Find();
#if true//!DEBUG
            if (inner != null)
                return inner;
#endif
            return FreeCategories;
        }

        public void Set(IEnumerable<Category> data)
        {
            _inner.Set(data);
            LastUpdated = DateTime.Now;

            _version++;
            RepositoryChanged?.Invoke(this, new QuestionRepositoryChangedEventArgs(data, _version));
        }

        /// <summary>
        /// Update the supplied question set with what there is saved in the local database
        /// </summary>
        /// <param name="set"></param>
        /// <returns></returns>
        public bool TryUpdate(ref QuestionSet set)
        {
            var categories = Find();
            foreach (var category in categories)
            {
                foreach (var questionSet in category.Sets)
                {
                    if (questionSet.Equals(set))
                    {
                        set = questionSet;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Find and update a question set based on existing saved questions
        /// </summary>
        /// <param name="set"></param>
        public Task Update(QuestionSet set)
        {
            return Task.Run(() =>
            {
                int index;
                var categories = Find();

                Category category_ = null;
                foreach (var category in categories)
                {
                    if (category.TryReplace(set, out index))
                    {
                        category_ = category;
                        break;
                    }
                }

                if (category_ == null)
                    throw new InvalidOperationException($"The question id {set.Id} is not found in any category");

                Set(categories);
            });
        }

        public void LoadIfNecessary()
        {
            try
            {
                var diff = DateTime.Now - LastUpdated;
                if (diff > TimeSpan.FromDays(DaysExpirationLocalCache) || Find() == null)
                {
                    Refresh().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Composition.DependencyContainer.Resolve<Logging.IExceptionLogger>().LogError(ex, "Unable to load the local categories list");
            }
        }

        public async Task<IList<Category>> Refresh()
        {
            IsRefreshing = true;
            try
            {
                EnsureRepositoryLoaded();
                var availableCategories = await _repository.GetAvailableCategories().ConfigureAwait(false);
                if (availableCategories == null)
                    throw new ArgumentNullException(nameof(availableCategories));
                Set(availableCategories);
                return availableCategories;
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private void EnsureRepositoryLoaded()
        {
            if (_repository == null)
                _repository = Composition.DependencyContainer.Resolve<Game.Repositories.IQuestionRepository>();
        }
    }

    public class QuestionRepositoryChangedEventArgs : EventArgs
    {
        public IEnumerable<Category> Categories { get; }
        public int Version { get; }

        public QuestionRepositoryChangedEventArgs(IEnumerable<Category> categories, int version)
        {
            Categories = categories;
        }
    }
}
