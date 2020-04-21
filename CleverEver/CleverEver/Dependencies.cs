using CleverEver.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CleverEver.Phone
{
    public class Dependencies
    {
        public static void Register()
        {
            // view models
            DependencyContainer.Register<Pages.Home.MainPageViewModel>();
            DependencyContainer.Register<CleverEver.Pages.Game.ViewCategoriesViewModel>();
            DependencyContainer.Register<CleverEver.Pages.Game.ViewCategoryViewModel>();
            DependencyContainer.Register<CleverEver.Pages.Game.PlayViewModel>();
            DependencyContainer.Register<CleverEver.Pages.Game.ResultViewModel>();


            // Not implemented services
            DependencyContainer.Register<Purchasing.IPurchasingService, Purchasing.NullPurchasingServer>();
            //DependencyContainer.Register<Game.IGameServer, Connectivity.NullMultiplayerServer>();


            // services
            /*DependencyContainer.Register<CleverEver.ApplicationServices.Authentication.AuthenticationService>();
            DependencyContainer.Register<CleverEver.ApplicationServices.DataServices.AdvertsService>();
            DependencyContainer.Register<CleverEver.ApplicationServices.DataServices.PlacesService>();
            DependencyContainer.Register<CleverEver.ApplicationServices.DataServices.SearchService>();
            DependencyContainer.Register<CleverEver.ApplicationServices.DataServices.FeedbackUserService>();
            DependencyContainer.Register<CleverEver.ApplicationServices.DataServices.NotificationsService>();


            DependencyContainer.Register<IAuthenticatedUserSessionStore, CleverEver.Phone.Settings.LocalAuthenticationSettingsStorage>();*/

            DependencyContainer.Register<Purchasing.PurchasingService>();
            
            //DependencyContainer.Register<Game.Repositories.IQuestionRepository, Services.QuestionService>();
            DependencyContainer.Register<Storage.QuestionsRepositoryManager>(DryIoc.Reuse.Singleton);
        }
    }
}
