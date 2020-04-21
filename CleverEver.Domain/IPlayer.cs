using CleverEver.Game.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Game
{
    /// <summary>
    /// The actual player implementation which is not business oriented
    /// </summary>
    public interface IPlayer : IEquatable<IPlayer>
    {
        string Id { get; }
        string Name { get; }
        PersonInfo Info { get; }

        int TotalScore { get; }

        void AddScore(int number);
    }

    public class PersonInfo
    {
        public string Email { get; }

        public PersonInfo(string email)
        {
            Email = email;
        }
    }

    public static class CurrentPlayerExtensions
    {
        public static Participant ToParticipant(this IPlayer player)
        {
            return new Participant(player.Id, player.Name);
        }
    }
}
