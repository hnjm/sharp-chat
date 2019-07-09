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

        public bool HasPassword
            => !string.IsNullOrEmpty(Password);

        public bool HasUser(SockChatUser user)
            => Users.Contains(user);

        public void UserJoin(SockChatUser user)
        {
            lock (Users)
            {
                if(user.Channel != this)
                {
                    user.Channel?.UserLeave(user);
                    user.Channel = this;
                }

                if(!Users.Contains(user))
                    Users.Add(user);
            }
        }

        public void UserLeave(SockChatUser user)
        {
            lock (Users)
            {
                if (user.Channel == this)
                    user.Channel = null;

                if(Users.Contains(user))
                    Users.Remove(user);
            }
        }

        public void Send(string data)
            => Users.ForEach(u => u.Send(data));

        public void Send(SockChatClientMessage inst, params string[] parts)
            => Send(parts.Pack(inst));

        public void Send(SockChatUser user, string message, string flags = @"10010")
        {
            message = new[] { Utils.UnixNow, user.UserId.ToString(), message, SockChatMessage.NextMessageId, flags }.Pack(SockChatClientMessage.MessageAdd);
            Send(message);
        }

        public void Send(bool error, string id, params string[] args)
        {
            Send(null, SockChatMessage.PackBotMessage(error ? 1 : 0, id, args));
        }

        public void UpdateUser(SockChatUser user)
        {
            Send(SockChatClientMessage.UserUpdate, user.ToString());
        }

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
