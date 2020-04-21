using System;
using System.Net;
using Xamarin.Forms;
using CleverEver.Composition;
using System.Threading.Tasks;
using CleverEver.Phone.Pages;

//#if !DEBUG
[assembly: Xamarin.Forms.Xaml.XamlCompilation(Xamarin.Forms.Xaml.XamlCompilationOptions.Compile)]
//#endif

namespace CleverEver.Phone
{
	public partial class App : Xamarin.Forms.Application
	{
		public static new App Current
		{
			get { return Xamarin.Forms.Application.Current as App; }
		}

        private Storage.QuestionsRepositoryManager _questionManager;

        public INavigation Navigation { get; private set; }

        public App()
		{
#if DEBUG
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debugger.Break();
            }
#else
            InitializeComponent();
#endif
            Localization.Localization.SetCulture(new System.Globalization.CultureInfo("ro"));

            Dependencies.Register();

            MainPage = new CleverEver.Pages.Navigation.RootPage();
            Navigation = MainPage.Navigation;
        }

        protected override void OnStart()
        {
            base.OnStart();

            _questionManager = DependencyContainer.Resolve<Storage.QuestionsRepositoryManager>();
            _questionManager.LoadIfNecessary();
        }
    }
}

