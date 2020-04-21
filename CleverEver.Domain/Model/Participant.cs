using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Game.Model
{
    /// <summary>
    /// A participant that is implied in a game. This can be a remote one or the current player
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public class Participant : IEquatable<Participant>
    {
        public string Id { get; }
        public string Name { get; }

        public Participant(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public bool Equals(Participant other)
        {
            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var player = obj as Participant;
            if (player == null)
                return false;
            return Equals(player);
        }
    }
}
