using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Game.Model
{
    public class Quest
    {
    }


    public class ContestSecret
    {
        public string Key { get; }

        public ContestSecret(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (key.Length < 25)
                throw new ArgumentException("key lenght is too weak");

            Key = key;
        }
    }
}
