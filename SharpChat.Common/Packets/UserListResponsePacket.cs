using SharpChat.Channels;
using SharpChat.Users;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packets {
    public class UserListResponsePacket : BotResponsePacket {
        public UserListResponsePacket(IUser sender, IUser requester, IEnumerable<ChatUser> users)
            : base(sender, BotArguments.Notice(@"who", MakeUserList(requester, users))) { }

        public UserListResponsePacket(IUser sender, Channel channel, IUser requester, IEnumerable<ChatUser> users)
            : this(sender, channel.Name, requester, users) { }

        public UserListResponsePacket(IUser sender, string channelName, IUser requester, IEnumerable<ChatUser> users)
            : base(sender, BotArguments.Notice(@"whochan", channelName, MakeUserList(requester, users))) { }

        private static string MakeUserList(IUser requester, IEnumerable<IUser> users) {
            StringBuilder sb = new StringBuilder();

            foreach(IUser user in users) {
                sb.Append(@"<a href=""javascript:void(0);"" onclick=""UI.InsertChatText(this.innerHTML);""");

                if(user == requester)
                    sb.Append(@" style=""font-weight: bold;""");

                sb.Append('>');
                sb.Append(user.DisplayName);
                sb.Append(@"</a>, ");
            }

            if(sb.Length > 2)
                sb.Length -= 2;

            return sb.ToString();
        }
    }
}
