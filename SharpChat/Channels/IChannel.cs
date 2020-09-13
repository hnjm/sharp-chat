using SharpChat.Events;
using SharpChat.Packet;
using System.Collections.Generic;

namespace SharpChat.Channels {
    public interface IChannel : IPacketTarget {
        string Name { get; }
        string Topic { get; }
        bool IsReadOnly { get; }
        bool IsPrivate { get; }
        bool IsTemporary { get; }
        bool HasPassword { get; }

        bool CanEnter(ChatUser user);
        bool VerifyPassword(string password);

        void SetPassword(string password);

        IEnumerable<IChatEvent> GetEvents(int amount, int offset);
        //void PostEvent(IChatEvent evt);

        IEnumerable<ChatUser> GetMembers();
        bool HasMember(ChatUser user);
        void AddMember(ChatUser user);
        void RemoveMember(ChatUser user, UserDisconnectReason reason = UserDisconnectReason.Leave);
    }
}
