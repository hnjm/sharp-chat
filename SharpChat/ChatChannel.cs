using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpChat {
    public class ChatChannel : IPacketTarget {
        public string Name { get; set; }
        public string Password { get; set; } = string.Empty;
        public bool IsTemporary { get; set; } = false;
        public int Hierarchy { get; set; } = 0;
        public ChatUser Owner { get; set; } = null;

        private readonly List<ChatUser> Users = new List<ChatUser>();

        public bool HasPassword
            => !string.IsNullOrWhiteSpace(Password);

        public string TargetName => Name;

        public ChatChannel() {
        }

        public ChatChannel(string name) {
            Name = name;
        }

        public bool HasUser(ChatUser user) {
            lock (Users)
                return Users.Contains(user);
        }

        public void UserJoin(ChatUser user) {
            if (!user.InChannel(this)) {
                // Remove this, a different means for this should be established for V1 compat.
                user.Channel?.UserLeave(user);
                user.JoinChannel(this);
            }

            lock (Users) {
                if (!HasUser(user))
                    Users.Add(user);
            }
        }

        public void UserLeave(ChatUser user) {
            lock (Users)
                Users.Remove(user);

            if (user.InChannel(this))
                user.LeaveChannel(this);
        }

        public void Send(IServerPacket packet) {
            lock (Users) {
                foreach (ChatUser user in Users)
                    user.Send(packet);
            }
        }

        public IEnumerable<ChatUser> GetUsers(IEnumerable<ChatUser> exclude = null) {
            lock (Users) {
                IEnumerable<ChatUser> users = Users.OrderByDescending(x => x.Rank);

                if (exclude != null)
                    users = users.Except(exclude);

                return users.ToList();
            }
        }

#pragma warning disable IDE0060 // Remove unused parameter
        public string Pack(int targetVersion = 1) {
            StringBuilder sb = new StringBuilder();

            sb.Append(Name);
            sb.Append('\t');
            sb.Append(string.IsNullOrEmpty(Password) ? '0' : '1');
            sb.Append('\t');
            sb.Append(IsTemporary ? '1' : '0');

            return sb.ToString();
        }
#pragma warning restore IDE0060 // Remove unused parameter
    }
}
