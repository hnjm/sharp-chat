using SharpChat.Events;
using SharpChat.Users;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpChat.Channels {
    public class Channel : IServerPacketTarget, IEventTarget {
        public string Name { get; private set; }
        public string Password { get; set; } = string.Empty;
        public bool IsTemporary { get; set; } = false;
        public int MinimumRank { get; set; } = 0;
        public bool AutoJoin { get; set; } = false;
        public uint MaxCapacity { get; set; } = 0;
        public ChatUser Owner { get; set; } = null;

        private List<ChatUser> Users { get; } = new List<ChatUser>();
        private List<ChannelTyping> Typing { get; } = new List<ChannelTyping>();

        public bool HasPassword
            => !string.IsNullOrWhiteSpace(Password);

        public bool HasMaxCapacity
            => MaxCapacity > 0;

        public string TargetName { get; private set; }

        public Channel(string name) {
            Name = name;
            TargetName = Name.ToLowerInvariant();
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
            //
        }
    }
}
