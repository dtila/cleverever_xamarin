using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Game.Model
{
    public class Packet
    {
        public string Id { get; }

        public Packet(string id)
        {
            Id = id;
        }
    }
}
