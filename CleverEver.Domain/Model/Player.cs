using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Game.Model
{
    /// <summary>
    /// The player for the current game
    /// </summary>
    public class Player : Participant, IPlayer
    {
        private IPlayer _player;

        /// <summary>
        /// Gets the score of the current game
        /// </summary>
        public int GameScore { get; private set; }

        public int BonusScore { get; private set; }

        public PersonInfo Info
        {
            get { return _player.Info; }
        }

        public int TotalScore
        {
            get { return _player.TotalScore; }
        }

        public Player(IPlayer player) 
            : base(player.Id, player.Name)
        {
            _player = player;
        }

        /// <summary>
        /// Add score to the user
        /// </summary>
        /// <param name="incrementScore">The total score that needs to be added</param>
        /// <param name="bonus">The bonus that is included in the total score</param>
        public void AddScore(int incrementScore, int bonus)
        {
            _player.AddScore(incrementScore);
            GameScore += incrementScore;
            BonusScore += bonus;
        }

        public void AddScore(int number)
        {
            AddScore(number, 0);
        }

        public bool Equals(IPlayer other)
        {
            return _player.Equals(other);
        }
    }
}
