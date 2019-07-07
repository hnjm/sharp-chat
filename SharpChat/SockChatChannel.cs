using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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

        public bool HasUser(SockChatUser user)
            => Users.Contains(user);

        public void UserJoin(SockChatUser user)
        {
            Users.Add(user);
            // do other shit too
        }

        public void UserLeave(SockChatUser user)
        {
            Users.Remove(user);
        }

        public void Send(string data)
            => Users.ForEach(u => u.Send(data));

        public void Send(SockChatClientMessage inst, params string[] parts)
            => Send(parts.Pack(inst));

        public string GetUsersString(IEnumerable<SockChatUser> exclude = null)
        {
            StringBuilder sb = new StringBuilder();
            IEnumerable<SockChatUser> users = Users;

            if (exclude != null)
                users = users.Except(exclude);

            sb.Append(users.Count());
            
            foreach(SockChatUser user in users)
            {
                sb.Append('\t');
                sb.Append(user);
            }

            return sb.ToString();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Name);
            sb.Append('\t');
            sb.Append(string.IsNullOrEmpty(Password) ? '0' : '1');
            sb.Append('\t');
            sb.Append(IsTemporary ? '1' : '0');

            return sb.ToString();
        }
    }
}
