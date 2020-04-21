using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Game.Achievements
{
    public interface IAchievement : IGameEventHandler
    {
        string Id { get; }

        event EventHandler Accomplished;
    }

    public interface IGameEventHandler
    {
        void Handle(GameStartedEventArgs e);
        void Handle(GameErrorEventArgs e);
        void Handle(GameOverEventArgs e);

        void Handle(QuestionAnswer playerAnswer);
    }
}
