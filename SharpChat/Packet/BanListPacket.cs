using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SharpChat.Packet
{
    public class BanListPacket : IServerPacket
    {
        public IEnumerable<SockChatUser> Users { get; private set; }
        public Dictionary<IPAddress, DateTimeOffset> IPs { get; private set; }

        public BanListPacket(IEnumerable<SockChatUser> users, Dictionary<IPAddress, DateTimeOffset> ips)
        {
            Users = users ?? throw new ArgumentNullException(nameof(users));
            IPs = ips ?? throw new ArgumentNullException(nameof(ips));
        }

        public string Pack(int version, int eventId)
        {
            StringBuilder sb = new StringBuilder();

            if(version >= 2)
            {
                // construct proper packet
            } else
            {
                sb.Append((int)SockChatServerPacket.MessageAdd);
                sb.Append(Constants.SEPARATOR);
                sb.Append(DateTimeOffset.Now.ToUnixTimeSeconds());
                sb.Append(Constants.SEPARATOR);
                sb.Append(-1);
                sb.Append(Constants.SEPARATOR);
                sb.Append("0\fbanlist\f");

                foreach (SockChatUser user in Users)
                    sb.AppendFormat(@"<a href=""javascript:void(0);"" onclick=""Chat.SendMessageWrapper('/unban '+ this.innerHTML);"">{0}</a>, ", user.Username);
                foreach (KeyValuePair<IPAddress, DateTimeOffset> ip in IPs)
                    sb.AppendFormat(@"<a href=""javascript:void(0);"" onclick=""Chat.SendMessageWrapper('/unban '+ this.innerHTML);"">{0}</a>, ", ip.Key);

                if(Users.Any() || IPs.Any())
                    sb.Length -= 2;

                sb.Append(Constants.SEPARATOR);
                sb.Append(eventId);
                sb.Append(Constants.SEPARATOR);
                sb.Append(SockChatMessageFlags.RegularUser.Serialise());
            }

            return sb.ToString();
        }
    }
}
