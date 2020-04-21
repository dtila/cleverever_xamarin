using CleverEver.Composition;
using CleverEver.Game.Model;
using CleverEver.Game.Repositories;
using CleverEver.Pages.Game.Templates;
using CleverEver.Phone.ViewModels;
using CleverEver.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace CleverEver.Pages.Game
{
    public class ViewCategoriesViewModel : BaseViewModel
    {
        private static Dictionary<string, string> _categoriesIcons = new Dictionary<string, string>
        {
            { "chimie", "\uf0c3" }, //
            { "matematica", "\uf12b" }, //fa-superscript http://fontawesome.io/icon/superscript/
            { "romana", "\uf02d" }, // fa-book http://fontawesome.io/icon/book/
            { "biologie", "\uf299" }, //fa-envira http://fontawesome.io/icon/envira/
            { "geografie", "\uf0ac" }, //fa-globe
            { "medicina", "\uf21e" } //fa-heartbeat http://fontawesome.io/icon/heartbeat/
        };

        private int _version;
        private IList<CategoryViewModelItem> _items;
        private QuestionsRepositoryManager _localRepository;

        public IList<CategoryViewModelItem> Items
        {
            get { return _items; }
            private set
            {
                SetField(ref _items, value);
                _version = _localRepository.Version;
            }
        }

        public ICommand RefreshCommand { get; }

        public ViewCategoriesViewModel(QuestionsRepositoryManager localRepository)
        {
            _localRepository = localRepository;

            Refresh(localRepository.Find(), localRepository.Version);
            RefreshCommand = new Command(HandleRefresh);
        }

        internal async Task Select(CategoryViewModelItem selectedItem)
        {
            var categoryVm = DependencyContainer.Resolve<ViewCategoryViewModel>();
            categoryVm.Init(selectedItem.Category.Text, selectedItem.Category.Sets, _version, set =>
            {
                MessagingCenter.Send(new UserSelection(selectedItem.Category, set), "StartGame");
            });

            await Navigation.PushAsync(new ViewCategoryPage(categoryVm));
        }

        private async void HandleRefresh()
        {
            for (int i = 0; ; i++)
            {
                try
                {
                    var categories = await _localRepository.Refresh();
                    if (categories == null)
                        throw new ArgumentNullException(nameof(categories));
                    Refresh(categories, _localRepository.Version);
                    break;
                }
                catch (Exception ex)
                {
                    DependencyContainer.Resolve<Logging.IExceptionLogger>().LogError(ex, "Unable to refresh the categories list");
                    if (i > 2)
                        break;
                }
            }
        }

        internal void OnAppearing()
        {
            IsBusy = _localRepository.IsRefreshing;
            if (_version != _localRepository.Version)
                Refresh(_localRepository.Find(), _localRepository.Version);

            _localRepository.IsRefreshingChanged += RefreshStatusChanged;
            _localRepository.RepositoryChanged += RepositoryChanged;
        }

        internal void OnDissapearing()
        {
            _localRepository.IsRefreshingChanged -= RefreshStatusChanged;
            _localRepository.RepositoryChanged -= RepositoryChanged;
        }

        private void RefreshStatusChanged(object sender, EventArgs e)
        {
            IsBusy = _localRepository.IsRefreshing;
        }

        private void RepositoryChanged(object sender, QuestionRepositoryChangedEventArgs e)
        {
            Refresh(e.Categories, e.Version);
        }

        private void Refresh(ICollection<Category> categories, int version)
        {
            var categoriesViewModels = new List<CategoryViewModelItem>(categories.Count);
            categoriesViewModels.AddRange(categories.Select(li => new CategoryViewModelItem(li)));
            Items = categoriesViewModels;
            _version = version;
        }

        private void Refresh(IEnumerable<Category> categories, int version)
        {
            Items = categories.Select(li => new CategoryViewModelItem(li)).ToList();
            _version = version;
        }

        class VersionableCategory : IVersionable<Category>
        {
            private int _version;
            private Category _category;

            public int Version
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public bool HasNewVersion
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public Category CurrentItem
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public void Refresh()
            {
                throw new NotImplementedException();
            }
        }


        [DebuggerDisplay("{Name}")]
        public class CategoryViewModelItem : IPurchasedItem
        {
            public Category Category { get; }
            public CategoryViewModelItem(Category category)
            {
                Category = category;
            }

            public string Icon
            {
                get
                {
                    string icon;
                    if (_categoriesIcons.TryGetValue(Category.Text.ToLower(), out icon))
                        return icon;
                    return "\uf02d"; //fa-book
                }
            }

            public bool IsLocked
            {
                get { return false; }
            }

            public string Name
            {
                get { return Category.Text; }
            }

            public string Description
            {
                get
                {
                    if (IsLocked)
                        return null;
                    return string.Format(Localization.UserMessages.levels_count_format, Category.Sets.Count);
                }
            }
        }
    }

    public class UserSelection
    {
        public Category Category { get; }
        public QuestionSet QuestionSet { get; }

        public UserSelection(Category category, QuestionSet set)
        {
            Category = category;
            QuestionSet = set;
        }
    }
}
