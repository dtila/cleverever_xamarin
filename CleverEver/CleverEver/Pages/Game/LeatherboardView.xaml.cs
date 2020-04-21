using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CleverEver.Pages.Game
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LeaderboardView : ContentPage
    {
        public LeaderboardView()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        private void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }
    }
}
