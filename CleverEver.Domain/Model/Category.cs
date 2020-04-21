using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Game.Model
{
    [DebuggerDisplay("{Text}")]
    public class Category : IEquatable<Category>
    {
        private List<QuestionSet> _sets;
        public string Text { get; }
        public IReadOnlyList<QuestionSet> Sets { get { return _sets; } }


        public Category(string text, QuestionSet[] sets)
        {
            Text = text;
            _sets = sets.ToList();
        }

        public bool TryReplace(QuestionSet set, out int index)
        {
            for (int i=0; i<_sets.Count; i++)
            {
                if (_sets[i].CanBeReplacedBy(set))
                {
                    _sets[i] = set;
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }

        public bool Equals(Category other)
        {
            if (other == null)
                return false;
            if (object.ReferenceEquals(other, this))
                return true;

            return Text == other.Text;
        }
    }
}
