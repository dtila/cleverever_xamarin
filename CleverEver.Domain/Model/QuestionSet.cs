using CleverEver.Game.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Game.Model
{
    [DebuggerDisplay("{Name}")]
    public class QuestionSet : IEquatable<QuestionSet>
    {
        public string Id { get; }
        public Packet Packet { get; }
        public int? Level { get; }
        public string Name { get; }
        public Question[] Questions { get; }

        /// <summary>
        /// Create a free question set
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="questions"></param>
        public QuestionSet(string id, string name, Question[] questions)
            : this(id, null, name, null, questions)
        {
        }

        public QuestionSet(string id, Packet packet, string name, int? level, Question[] questions)
        {
            Id = id;
            Name = name;
            Packet = packet;
            Level = level;
            Questions = questions;
        }

        public bool Equals(QuestionSet other)
        {
            return Id == other.Id && Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            var other = obj as QuestionSet;
            if (other == null)
                return false;
            return Equals(other);
        }

        /// <summary>
        /// Check if the current question can be replaced by the other one
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool CanBeReplacedBy(QuestionSet other)
        {
            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public bool CanPurchase
        {
            get
            {
                return Packet != null && Questions.Length == 0; 
            }
        }

        public bool CanPlay
        {
            get
            {
                if (Questions.Length == 0)
                    return false;

                foreach (var question in Questions)
                {
                    if (question == null || string.IsNullOrEmpty(question.Text))
                        return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Randomize and shuffle the questions from the set
        /// </summary>
        /// <param name="rand"></param>
        public void Randomize(Random rand)
        {
            for (int i = 0; i < Questions.Length / 2; i++)
            {
                var index = rand.Next(0, Questions.Length);
                SwapQuestions(index, i);
            }

            foreach (var question in Questions)
            {
                question.Randomize(rand);
            }
        }

        private void SwapQuestions(int index, int newIndex)
        {
            var temp = Questions[index];
            Questions[index] = Questions[newIndex];
            Questions[newIndex] = temp;
        }
    }

    public enum Difficulty
    {
        Easy = 1,
        Medium = 2,
        Hard = 3
    }
}
