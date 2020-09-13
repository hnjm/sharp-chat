using SharpChat.Events;
using SharpChat.Packet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Channels {
    public class LogChannel : IChannel {
        public string Name { get; } = @"@log";
        public string Topic { get; }

        public bool IsReadOnly => true;
        public bool IsPrivate => true;
        public bool IsTemporary => true;
        public bool HasPassword => false;

        public ChatUserSession Session { get; }

        private List<IChatEvent> Log { get; } = new List<IChatEvent>();

        public LogChannel(ChatUserSession session) {
            Session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public bool CanEnter(ChatUser user) {
            return Session.User == user;
        }

        public void Send(IServerPacket packet) {
            Session.Send(packet);
        }

        public void AddMember(ChatUser user) { }
        public void RemoveMember(ChatUser user, UserDisconnectReason reason = UserDisconnectReason.Leave) { }
        public IEnumerable<ChatUser> GetMembers() {
            yield return Session.User;
        }
        public bool HasMember(ChatUser user) {
            return user == Session.User;
        }

        public void SetPassword(string password) {}

        public bool VerifyPassword(string password) {
            return true;
        }

        public IEnumerable<IChatEvent> GetEvents(int amount, int offset) {
            lock(Log)
                return Log.OrderByDescending(x => x.SequenceId).Skip(offset).Take(amount);
        }
    }
}
