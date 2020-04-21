using CleverEver.Phone.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CleverEver.Phone.Base
{
    public abstract class BaseListViewModel<TDto, TViewModel> : BaseViewModel where TViewModel : class
    {
        private Func<TDto, TViewModel> _viewModelSelector;
        protected ObservableCollection<TViewModel> _items;

        public Command RefreshCommand { get; set; }

        public ObservableCollection<TViewModel> Items
        {
            get { return _items; }
            set { SetField(ref _items, value); }
        }

        public BaseListViewModel(Func<TDto, TViewModel> viewModelSelector)
        {
            _viewModelSelector = viewModelSelector;
            RefreshCommand = new Command(InternalRefresh, () => !IsBusy);
        }

        public void Refresh()
        {
            InternalRefresh();
        }


        protected abstract Task<List<TDto>> OnRefresh();
        protected virtual void OnDataLoaded(ICollection<TDto> dtos) { }

        protected void SetItems(IEnumerable<TDto> items)
        {
            Items = new ObservableCollection<TViewModel>(items.Select(_viewModelSelector));
        }

        private async void InternalRefresh()
        {
            IsBusy = true;
            RefreshCommand.ChangeCanExecute();

            try
            {
                var dtos = await OnRefresh();
                SetItems(dtos);
                OnDataLoaded(dtos);
            }
            /*catch(UnauthenticatedServerRequest)
            {
                await App.Current.HandleSessionExpiration();
            }
            catch (WebServerException ex)
            {
                if (ex.IsRequestError)
                {
                    App.Current.ConnectivityHandler.RegisterRefreshable(this);
                }

                throw;
            }*/
            finally
            {
                IsBusy = false;
                RefreshCommand.ChangeCanExecute();
            }
        }
    }
}
