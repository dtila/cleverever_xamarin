using Acr.UserDialogs;
using CleverEver.Analytics;
using CleverEver.Logging;
using CleverEver.Pages.Game.Templates;
using CleverEver.Phone.ViewModels;
using CleverEver.Purchasing;
using CleverEver.Services;
using CleverEver.Specification;
using CleverEver.Storage;
using CleverEver.Game.Strategy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using CleverEver.Game.Model;
using CleverEver.Localization;

namespace CleverEver.Pages.Game
{
    public class ViewCategoryViewModel : BaseViewModel
    {
        private IEnumerable<QuestionSet> _questions;
        private IList<object> _items;

        private int _version;
        private IUserDialogs _dialogs;
        private QuestionsRepositoryManager _localRepository;
        private Action<QuestionSet> _questionSetSelectedCallback;

        public string Title { get; private set; }

        public IList<object> Items
        {
            get { return _items; }
            private set { SetField(ref _items, value); }
        }

        public ViewCategoryViewModel(QuestionsRepositoryManager localRepository, IUserDialogs dialogs)
        {
            _dialogs = dialogs;
            _localRepository = localRepository;
        }

        public void Init(string title, IEnumerable<QuestionSet> items, int version, Action<QuestionSet> questionSelectedCallback)
        {
            _questionSetSelectedCallback = questionSelectedCallback;
            _questions = items;

            Title = title;
            RaisePropertyChanged(nameof(Title));

            Refresh(items, version);
        }

        public void OnAppearing()
        {
            IsBusy = _localRepository.IsRefreshing;
            if (_version != _localRepository.Version)
                Refresh(_localRepository.Find(), _localRepository.Version);

            _localRepository.RepositoryChanged += CategoryQuestionsUpdated;
            _localRepository.IsRefreshingChanged += IsRefreshingChanged;
        }

        public void OnDisappearing()
        {
            _localRepository.RepositoryChanged -= CategoryQuestionsUpdated;
            _localRepository.IsRefreshingChanged -= IsRefreshingChanged;
        }

        internal async Task Select(object selectedItem)
        {
            if (selectedItem == null)
                throw new InvalidOperationException("The selected question set can not be null");

            var multiSelectableItem = selectedItem as QuestionNameGroupViewModel;
            if (multiSelectableItem != null)
            {
                var categoryVm = Composition.DependencyContainer.Resolve<ViewCategoryViewModel>();
                categoryVm.Init(multiSelectableItem.Name, multiSelectableItem.Sets, _version, _questionSetSelectedCallback);

                await Navigation.PushAsync(new ViewCategoryPage(categoryVm)).ConfigureAwait(false);
                return;
            }

            var questionSet = selectedItem as SingleQuestionSetViewModel;
            if (questionSet.Set.CanPurchase)
            {
                await Purchase(questionSet.Set).ConfigureAwait(false);
                return;
            }

            _questionSetSelectedCallback?.Invoke(questionSet.Set);
        }

        private async Task Purchase(QuestionSet set)
        {
            try
            {
                using (var loading = _dialogs.Loading())
                {
                    if (!set.CanPurchase)
                        throw new InvalidOperationException($"The question set {set.Id} can not be purchased");

                    var purchasingService = Composition.DependencyContainer.Resolve<PurchasingService>();
                    await purchasingService.Purchase(set.Packet);
                    if (!_localRepository.TryUpdate(ref set))
                        throw new InvalidOperationException($"The set {set.Id} could not be updated");

                    if (!set.CanPlay)
                        throw new InvalidOperationException($"The set {set.Id} it is not playable ");

                    _questionSetSelectedCallback?.Invoke(set);
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                Composition.DependencyContainer.Resolve<Logging.IExceptionLogger>().LogError(ex);
                _dialogs.ShowError(Localization.UserMessages.purchase_error);
            }
        }

        private void CategoryQuestionsUpdated(object sender, QuestionRepositoryChangedEventArgs e)
        {
            Refresh(e.Categories, _localRepository.Version);
        }

        private void Refresh(IEnumerable<Category> categories, int version)
        {
            var refreshables = from set in categories.SelectMany(li => li.Sets)
                               from item in _questions
                               where set.Equals(item)
                               select set;
            Refresh(refreshables, version);
        }

        private void Refresh(IEnumerable<QuestionSet> sets, int version)
        {
            var total = sets.Count();
            var allQuestionSets = true;
            var allSameName = true;
            string previousName = null;
            var list = new List<object>();
            var levelComparer = new LevelComparer();

            foreach (var group in sets.GroupBy(li => li.Name))
            {
                var groupList = group.ToList();
                if (groupList.Count > 1 && groupList.Count != total)
                {
                    groupList.Sort(levelComparer);
                    list.Add(new QuestionNameGroupViewModel(group.Key, groupList));
                    allQuestionSets = false;
                    continue;
                }

                foreach (var item in groupList)
                {
                    list.Add(new SingleQuestionSetViewModel(item));

                    if (previousName == null)
                        previousName = item.Name;
                    else
                    {
                        if (!string.Equals(previousName, item.Name))
                            allSameName = false;
                    }
                }
            }

            if (allQuestionSets && allSameName)
            {
                list.Sort(new CategoryLevelTryComparer());
                foreach (SingleQuestionSetViewModel item in list)
                {
                    item.DisplayLevelOnly();
                }
            }

            Items = list;
            _questions = sets;
            _version = version;
        }

        private void IsRefreshingChanged(object sender, EventArgs e)
        {
            IsBusy = _localRepository.IsRefreshing;
        }



        public class QuestionNameGroupViewModel : BaseViewModel, IPurchasedItem
        {
            public IList<QuestionSet> Sets { get; private set; }

            public QuestionNameGroupViewModel(string name, IList<QuestionSet> set)
            {
                Name = name;
                Sets = set;
            }

            public string Icon
            {
                get { return null; }
            }

            public bool IsLocked
            {
                get { return false; }
            }

            public string Name { get; private set; }

            public string Description
            {
                get { return string.Format(UserMessages.levels_count_format, Sets.Count); }
            }

            private void Refresh()
            {
                RaisePropertyChanged(nameof(IsLocked));
                RaisePropertyChanged(nameof(Name));
                RaisePropertyChanged(nameof(Description));
            }
        }


        [DebuggerDisplay("{Name}")]
        public class SingleQuestionSetViewModel : BaseViewModel, IPurchasedItem
        {
            public QuestionSet Set { get; private set; }

            public SingleQuestionSetViewModel(QuestionSet set)
            {
                Set = set;
                Name = CreateName();
            }

            public string Icon
            {
                get { return null; }
            }

            public bool IsLocked
            {
                get { return Set.CanPurchase && !Set.CanPlay; }
            }

            public string Name
            {
                get; private set;
            }

            public void DisplayLevelOnly()
            {
                if (Set.Level != null)
                    Name = $"{UserMessages.level} {Set.Level}";
            }

            public string Description
            {
                get
                {
                    if (IsLocked)
                        return null;
                    return string.Format(UserMessages.questions_count_format, Set.Questions.Length);
                }
            }

            private void Refresh()
            {
                RaisePropertyChanged(nameof(IsLocked));
                RaisePropertyChanged(nameof(Name));
                RaisePropertyChanged(nameof(Description));
            }

            private string CreateName()
            {
                //if (Set.Level == null)
                return Set.Name;
                //return $"{Set.Name} ({UserMessages.level} {Set.Level})";
            }
        }

        class LevelComparer : IComparer<QuestionSet>
        {
            public int Compare(QuestionSet x, QuestionSet y)
            {
                if (x == null || y == null || x.Level == null || y.Level == null)
                    return -1;

                return x.Level.Value.CompareTo(y.Level.Value);
            }
        }

        class CategoryLevelTryComparer : IComparer<object>, IComparer<SingleQuestionSetViewModel>
        {
            public int Compare(object x, object y)
            {
                var a = x as SingleQuestionSetViewModel;
                var b = y as SingleQuestionSetViewModel;
                if (a == null || b == null)
                    throw new NotImplementedException($"The comparer type needs to be '{nameof(SingleQuestionSetViewModel)}' type instead '{a.GetType()}'");
                return Compare(a, b);
            }

            public int Compare(SingleQuestionSetViewModel x, SingleQuestionSetViewModel y)
            {
                return x.Set.Level.GetValueOrDefault().CompareTo(y.Set.Level.GetValueOrDefault());
            }
        }
    }
}
