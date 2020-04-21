using CleverEver.Common;
using CleverEver.Game.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Specification
{
    public class PurchasedQuestionSpecification : ISpecification<QuestionSet>
    {
        public bool IsSatisfiedBy(QuestionSet item)
        {
            if (item.Questions.Length == 0)
                return false;

            foreach (var question in item.Questions)
            {
                if (question == null || string.IsNullOrEmpty(question.Text))
                    return false;
            }

            return true;
        }
    }
}
