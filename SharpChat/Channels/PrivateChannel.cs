using SharpChat.Events;
using SharpChat.Packet;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace SharpChat.Channels {
    public class PrivateChannel : IChannel {
        public string Name { get; }
        public string Topic { get; }

        public bool IsReadOnly { get; private set; }

        public bool IsPrivate => true;
        public bool IsTemporary => true;
        public bool HasPassword => false;

        private List<long> InvitedUsers { get; } = new List<long>();
        private List<ChatUser> Members { get; }

        public PrivateChannel(IEnumerable<ChatUser> users) {
            if(users == null)
                throw new ArgumentNullException(nameof(users));
            if(!users.Any())
                throw new ArgumentException(@"No members supplied.", nameof(users));

            Members = new List<ChatUser>(users);

            StringBuilder sb = new StringBuilder();
            sb.Append(@"@pm");
            foreach(ChatUser user in Members)
                sb.AppendFormat(@"-{0}", user.UserId);
            Name = sb.ToString();
        }

        public bool CanEnter(ChatUser user) {
            if(user == null)
                return false;

            if(HasMember(user))
                return true;

            lock(InvitedUsers)
                if(InvitedUsers.Contains(user.UserId))
                    return true;

            return false;
        }

        public void SetPassword(string password) {}
        public bool VerifyPassword(string password) {
            return true;
        }

        public IEnumerable<IChatEvent> GetEvents(int amount, int offset) {
            return Database.GetEvents(this, amount, offset);
        }
        public void PostEvent(IChatEvent evt) {
            Database.LogEvent(evt);
        }

        public void Send(IServerPacket packet) {
            lock(Members)
                foreach(ChatUser user in Members)
                    user.Send(packet);
        }

        public IEnumerable<ChatUser> GetMembers() {
            lock(Members)
                return Members.ToImmutableList();
        }
        public bool HasMember(ChatUser user) {
            lock(Members)
                return Members.Contains(user);
        }
        public void AddMember(ChatUser user) {
            lock(Members)
                if(!Members.Contains(user))
                    Members.Add(user);
        }
        public void RemoveMember(ChatUser user, UserDisconnectReason reason = UserDisconnectReason.Leave) {
            lock(Members)
                Members.Remove(user);
        }
    }
}
