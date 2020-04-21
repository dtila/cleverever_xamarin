using CleverEver.Game.Achievements;
using CleverEver.Game.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Game
{
    public delegate void QuestionAnsweredEventDelegate(IGame game, QuestionAnswer answer);

    public interface IGame
    {
        event EventHandler<GameOverEventArgs> GameFinished;
        event EventHandler<GameErrorEventArgs> GameError;
        event EventHandler<NextQuestionEventArgs> NextQuestion;
        
        Player Player { get; }
        IList<IGameEventHandler> EventHandlers { get; }

        Question CurrentQuestion { get; }

        void Answer(int answerIndex);
        void Skip();
        void Stop();
    }

    public class NextQuestionEventArgs : EventArgs
    {
        public Question NextQuestion { get; }

        public NextQuestionEventArgs(Question nextQuestion)
        {
            NextQuestion = nextQuestion;
        }
    }

    public class GameStartedEventArgs //: EventArgs
    {
        public string SetName { get; }
        public int TotalQuestions { get; }
        public Question FirstQuestion { get; }

        public GameStartedEventArgs(string setName, int totalQuestions, Question firstQuestion)
        {
            SetName = setName;
            TotalQuestions = totalQuestions;
            FirstQuestion = firstQuestion;
        }
    }


    public class GameOverEventArgs : EventArgs
    {
        public Participant CurrentParticipant { get; }
        public ICollection<ParticipantAnswers> PlayerAnswers { get; }
        public bool IsInterrupted { get; }
        public int TotalQuestions { get; }

        /// <summary>
        /// Create a game over result for a single player
        /// </summary>
        /// <param name="participant"></param>
        /// <param name="answers"></param>
        public GameOverEventArgs(Participant participant, ICollection<QuestionAnswer> answers, bool isInterrupted, int questions)
        {
            CurrentParticipant = participant;
            PlayerAnswers = new[] { new ParticipantAnswers(participant, answers) };
            IsInterrupted = isInterrupted;
            TotalQuestions = questions;
        }

        /// <summary>
        /// Create a game over result when more players are involved
        /// </summary>
        /// <param name="participant"></param>
        /// <param name="answers"></param>
        public GameOverEventArgs(Participant participant, ICollection<ParticipantAnswers> answers, bool isInterrupted, int questions)
        {
            CurrentParticipant = participant;
            PlayerAnswers = answers;
            IsInterrupted = isInterrupted;
            TotalQuestions = questions;
        }

        public bool TryGetPlayerAnswers(out ParticipantAnswers answers)
        {
            if (PlayerAnswers.Count == 0)
            {
                answers = default(ParticipantAnswers);
                return false;
            }

            answers = PlayerAnswers.Single(li => li.Participant.Equals(CurrentParticipant));
            return true;
        }
    }

    public class GameErrorEventArgs : EventArgs
    {
        public GameErrorReason Reason { get; }
        public GameErrorEventArgs(GameErrorReason reason)
        {
            Reason = reason;
        }
    }

    [Flags]
    public enum GameErrorReason
    {
        TooLessPlayers = 1,
        StopGame = 2
    }

    [DebuggerDisplay("{Participant}")]
    public struct ParticipantAnswers
    {
        public Participant Participant { get; }
        public ICollection<QuestionAnswer> Answers { get; }

        public ParticipantAnswers(Participant participant, ICollection<QuestionAnswer> answers)
        {
            Participant = participant;
            Answers = answers;
        }

        public int CorrectAnswersCount
        {
            get
            {
                int score = 0;
                foreach (var answer in Answers)
                {
                    if (answer.IsSkipped)
                        continue;

                    if (answer.IsCorrect)
                        score++;
                }
                return score;
            }
        }
    }

    /// <summary>
    /// A pair of a question and the players index answer
    /// </summary>
    [DebuggerDisplay("{Question}")]
    public struct QuestionAnswer
    {
        public const int SkippedIndexValue = -1;

        public Question Question { get; }
        public int AnswerIndex { get; }

        public bool IsSkipped
        {
            get { return AnswerIndex == SkippedIndexValue; }
        }

        public bool IsCorrect
        {
            get { return Question.CorrectIndex == AnswerIndex; }
        }

        private QuestionAnswer(Question question, int answerIndex)
        {
            Question = question;
            AnswerIndex = answerIndex;
        }

        public static QuestionAnswer Create(Question question, int answerIndex)
        {
            return new QuestionAnswer(question, answerIndex);
        }

        public static QuestionAnswer CreateSkipped(Question question)
        {
            return new QuestionAnswer(question, SkippedIndexValue);
        }
    }
}
