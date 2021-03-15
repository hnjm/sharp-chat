using SharpChat.Events;
using SharpChat.Users;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpChat.Channels {
    public class Channel : IServerPacketTarget, IEventTarget {
        public string Name { get; private set; }
        public string Password { get; private set; } = string.Empty;
        public bool IsTemporary { get; private set; } = false;
        public int MinimumRank { get; private set; } = 0;
        public bool AutoJoin { get; private set; } = false;
        public uint MaxCapacity { get; private set; } = 0;
        public IUser Owner { get; private set; } = null;

        private List<ChatUser> Users { get; } = new List<ChatUser>();
        private List<ChannelTyping> Typing { get; } = new List<ChannelTyping>();

        public bool HasPassword
            => !string.IsNullOrWhiteSpace(Password);

        public bool HasMaxCapacity
            => MaxCapacity > 0;

        public string TargetName { get; private set; }

        public Channel(
            string name,
            bool temp = false,
            int minimumRank = 0,
            string password = null,
            bool autoJoin = false,
            uint maxCapacity = 0,
            IUser owner = null
        ) {
            Name = name;
            TargetName = Name.ToLowerInvariant();
            IsTemporary = temp;
            MinimumRank = minimumRank;
            Password = password ?? string.Empty;
            AutoJoin = autoJoin;
            MaxCapacity = maxCapacity;
            Owner = owner;
        }

        public bool HasUser(ChatUser user) {
            lock(Users)
                return Users.Contains(user);
        }

        public void UserJoin(ChatUser user) {
            if(!user.InChannel(this))
                user.JoinChannel(this);

            lock(Users) {
                if(!HasUser(user))
                    Users.Add(user);
            }
        }

        public void UserLeave(ChatUser user) {
            lock(Users)
                Users.Remove(user);

            if(user.InChannel(this))
                user.LeaveChannel(this);
        }

        public void SendPacket(IServerPacket packet) {
            lock(Users) {
                foreach(ChatUser user in Users)
                    user.SendPacket(packet);
            }
        }

        public IEnumerable<ChatUser> GetUsers(IEnumerable<ChatUser> exclude = null) {
            lock(Users) {
                IEnumerable<ChatUser> users = Users.OrderByDescending(x => x.Rank);

                if(exclude != null)
                    users = users.Except(exclude);

                return users.ToList();
            }
        }

        public bool IsTyping(ChatUser user) {
            if(user == null)
                return false;
            lock(Typing)
                return Typing.Any(x => x.User == user && !x.HasExpired);
        }
        public bool CanType(ChatUser user) {
            if(user == null || !HasUser(user))
                return false;
            return !IsTyping(user);
        }
        public ChannelTyping RegisterTyping(ChatUser user) {
            if(user == null || !HasUser(user))
                return null;
            ChannelTyping typing = new ChannelTyping(user);
            lock(Typing) {
                Typing.RemoveAll(x => x.HasExpired);
                Typing.Add(typing);
            }
            return typing;
        }

        public string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append(Name);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(string.IsNullOrEmpty(Password) ? '0' : '1');
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(IsTemporary ? '1' : '0');

            return sb.ToString();
        }

        public void HandleEvent(IEvent evt) {
            switch(evt) {
                case ChannelUpdateEvent update:
                    if(update.HasName)
                        Name = update.Name;
                    if(update.IsTemporary.HasValue)
                        IsTemporary = update.IsTemporary.Value;
                    if(update.MinimumRank.HasValue)
                        MinimumRank = update.MinimumRank.Value;
                    if(update.HasPassword)
                        Password = update.Password;
                    break;
            }
        }
    }
}
