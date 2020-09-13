using SharpChat.Events;
using SharpChat.Packet;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace SharpChat.Channels {
    public class ChatChannel : IChannel {
        public string Name { get; }
        public string Topic { get; }

        public bool IsReadOnly => false;
        public bool IsPrivate => HasPassword;
        public bool IsTemporary { get; }

        private string Password { get; set; }
        public bool HasPassword => Password != null;

        public int RequiredRank { get; set; } = 0;

        private List<ChatUser> Members { get; } = new List<ChatUser>();

        public ChatChannel(string name) {
            Name = name ?? throw new ArgumentNullException(name);
        }

        public void SetPassword(string password) {
            Password = string.IsNullOrWhiteSpace(password) ? null : password.GetSignedHash();
        }
        public bool VerifyPassword(string password) {
            return !HasPassword || (!string.IsNullOrWhiteSpace(password) && Password == password.GetSignedHash());
        }

        public bool CanEnter(ChatUser user) {
            return user != null && user.Rank >= RequiredRank;
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
