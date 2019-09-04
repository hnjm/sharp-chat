using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpChat
{
    public class SockChatChannel
    {
        public string Name { get; set; }
        public string Password { get; set; } = string.Empty;
        public bool IsTemporary { get; set; } = false;
        public int Hierarchy { get; set; } = 0;
        public SockChatUser Owner { get; set; } = null;

        public readonly List<SockChatUser> Users = new List<SockChatUser>();

        public bool HasPassword
            => !string.IsNullOrEmpty(Password);

        public SockChatChannel()
        {
        }

        public SockChatChannel(string name)
        {
            Name = name;
        }

        public bool HasUser(SockChatUser user)
            => Users.Contains(user);

        public void UserJoin(SockChatUser user)
        {
            lock (Users)
            {
                lock (user.Channels)
                    if (!user.Channels.Contains(this))
                    {
                        user.Channel?.UserLeave(user);
                        user.Channels.Add(this);
                    }

                if(!Users.Contains(user))
                    Users.Add(user);
            }
        }

        public void UserLeave(SockChatUser user)
        {
            lock (Users)
            {
                lock(user.Channels)
                    if (user.Channels.Contains(this))
                        user.Channels.Remove(this);

                if(Users.Contains(user))
                    Users.Remove(user);
            }
        }

        public void Send(IServerPacket packet, int eventId = 0)
        {
            lock (Users)
                Users.ForEach(u => u.Send(packet, eventId));
        }

        public IEnumerable<SockChatUser> GetUsers(IEnumerable<SockChatUser> exclude = null)
        {
            lock (Users)
            {
                IEnumerable<SockChatUser> users = Users.OrderByDescending(x => x.Hierarchy);

                if (exclude != null)
                    users = users.Except(exclude);

                return users.ToList();
            }
        }

        public string Pack(int targetVersion = 1)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Name);
            sb.Append(Constants.SEPARATOR);
            sb.Append(string.IsNullOrEmpty(Password) ? '0' : '1');
            sb.Append(Constants.SEPARATOR);
            sb.Append(IsTemporary ? '1' : '0');

            return sb.ToString();
        }
    }
}
