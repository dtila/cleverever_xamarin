using CleverEver.Game.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Game.Model
{
    [DebuggerDisplay("{Text}")]
    public class Question : IEquatable<Question>
    {
        private int _correntIndex;

        public string Text { get; }
        public string[] Answers { get; }
        public int CorrectIndex
        {
            get { return _correntIndex; }
        }

        public Question(string text, string[] answers, int correctIndex)
        {
            if (string.IsNullOrEmpty(text))
                throw new InvalidOperationException("Question text can not be empty");

            if (answers == null)
                throw new ArgumentNullException(nameof(answers));

            if (answers.Length != 4)
                throw new InvalidOperationException("Only questions with 4 answers are allowed for now");

            if (correctIndex < 0 || correctIndex > answers.Length)
                throw new InvalidOperationException("Correct index needs to be between 0 and the number of questions");

            Text = text;
            Answers = answers;
            _correntIndex = correctIndex;
        }

        public bool Equals(Question other)
        {
            return string.Equals(Text, other.Text);
        }

        public override int GetHashCode()
        {
            return Text.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as Question;
            if (other == null)
                return false;
            return Equals(other);
        }

        public void Randomize(Random random)
        {
            for (int i = 0; i<Answers.Length / 2; i++)
            {
                var index = random.Next(0, Answers.Length);
                SwapQuestions(index, CorrectIndex);
                _correntIndex = index;
            }
        }

        private void SwapQuestions(int index, int newIndex)
        {
            var temp = Answers[index];
            Answers[index] = Answers[newIndex];
            Answers[newIndex] = temp;
        }
    }

}
