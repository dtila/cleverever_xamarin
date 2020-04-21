using CleverEver.Pages.Game.Templates;
using CleverEver.Phone.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Pages.Game
{
    public class LeaderboardViewModel : BaseViewModel
    {
        private ObservableCollection<LeaderboardItem> _leaderboard;

        public ObservableCollection<LeaderboardItem> Leaderboard
        {
            get { return _leaderboard; }
            set { SetField(ref _leaderboard, value); }
        }

        private LeaderboardItem _firstPlace;
        public LeaderboardItem FirstPlace
        {
            get { return _firstPlace; }
            set { SetField(ref _firstPlace, value); }
        }

        private LeaderboardItem _secondPlace;
        public LeaderboardItem SecondPlace
        {
            get { return _secondPlace; }
            set { SetField(ref _secondPlace, value); }
        }

        private LeaderboardItem _thirdPlace;
        public LeaderboardItem ThirdPlace
        {
            get { return _thirdPlace; }
            set { SetField(ref _thirdPlace, value); }
        }


        public LeaderboardViewModel()
        {
            var image = "http://wallpaper-gallery.net/images/profile-pics/profile-pics-20.jpg";
            FirstPlace = new LeaderboardItem(1, image, "First place", 140);
            SecondPlace = new LeaderboardItem(2, image, "SecondPlace", 160);
            ThirdPlace = new LeaderboardItem(3, image, "ThirdPlace", 120);

            _leaderboard = new ObservableCollection<LeaderboardItem>();
            foreach (var item in Enumerable.Range(4, 6))
                _leaderboard.Add(new LeaderboardItem(item, image, "First place" + item, 100 + item));
        }




        public class LeaderboardItem : BaseViewModel, IScoredItem
        {
            public LeaderboardItem(int index, string image, string name, decimal score)
            {
                Rank = index;
                Image = image;
                Name = name;
                Score = score;
            }

            public int Rank { get; }

            public string Image { get; }

            public string Name { get; }

            public decimal Score { get; }
        }
    }
}
