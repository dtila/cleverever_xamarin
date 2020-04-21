using CleverEver.Game.Multiplayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Game
{
    public interface IGameServerMultiplayerSupport
    {
        event EventHandler<InvitationReceivedEventArgs> InvitationReceived;

        Task<IRoomHost> CreateRoomAsync();
        Task<IRoomJoiner> JoinRoomAsync();
    }


    public interface IInvitation
    {
        Task<IRoomJoiner> Accept();
        void Decline();
    }

    public class InvitationReceivedEventArgs : EventArgs
    {
        public string Inviter { get; }
        public IInvitation Invitation { get; }

        public InvitationReceivedEventArgs(string inviter, IInvitation invitation)
        {
            Inviter = inviter;
            Invitation = invitation;
        }
    }
}
