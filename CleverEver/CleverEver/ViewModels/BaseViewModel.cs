using CleverEver.Composition;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CleverEver.Phone.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        private bool _isBusy;
        private List<string> _operations;

        public bool IsBusy
        {
            get { return _isBusy; } 
            set { SetField(ref _isBusy, value); }
        }

        public INavigation Navigation
        {
            get { return App.Current.Navigation; }
        }

        /*public Plugin.Toasts.IToastNotificator Notification
        {
            get { return DependencyContainer.Resolve<Plugin.Toasts.IToastNotificator>(); }
        }*/

        public event PropertyChangedEventHandler PropertyChanged;

        public BaseViewModel()
        {
            _operations = new List<string>();
        }

        protected PendingOperationScope PendingOperation(string operation)
        {
            return new PendingOperationScope(operation, this);
        }

        protected OperationScope BeginOperation()
        {
            return new OperationScope(this);
        }

        protected void BeginOperation(string operation)
        {
            _operations.Add(operation);
            IsBusy = true;
        }

        protected void EndOperation(string operation)
        {
            _operations.Remove(operation);
            if (_operations.Count == 0)
                IsBusy = false;
        }

        protected bool SetField<T>(ref T member, T value, [CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException("propertyName");

            if (!EqualityComparer<T>.Default.Equals(member, value))
            {
                member = value;
                RaiseOnPropertyChanged(new PropertyChangedEventArgs(propertyName));
                return true;
            }
            return false;
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            RaiseOnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected void RaiseOnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (string.IsNullOrEmpty(args.PropertyName))
                throw new InvalidOperationException("The property named must not be null or empty");

            var handler = PropertyChanged;
            if (handler != null)
                handler(this, args);
        }

        public struct PendingOperationScope : IDisposable
        {
            private string _operation;
            private BaseViewModel _viewModel;

            public PendingOperationScope(string operation, BaseViewModel viewModel)
            {
                this._operation = operation;
                this._viewModel = viewModel;
                viewModel.BeginOperation(operation);
            }

            public void Dispose()
            {
                this._viewModel.EndOperation(_operation);
            }
        }

        public struct OperationScope : IDisposable
        {
            private BaseViewModel _viewModel;

            public OperationScope(BaseViewModel viewModel)
            {
                viewModel.IsBusy = true;
                _viewModel = viewModel;
            }

            public void Dispose()
            {
                _viewModel.IsBusy = false;
            }
        }
    }
}
