using SharpChat.Channels;
using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpChat.Commands {
    public class WhoCommand : IChatCommand {
        public bool IsMatch(string name, IEnumerable<string> args)
            => name == @"who";

        private static string MakeUserList(ChatUser currentUser, IEnumerable<ChatUser> users) {
            StringBuilder sb = new StringBuilder();

            foreach(ChatUser whoUser in users) {
                sb.Append(@"<a href=""javascript:void(0);"" onclick=""UI.InsertChatText(this.innerHTML);""");

                if(whoUser == currentUser)
                    sb.Append(@" style=""font-weight: bold;""");

                sb.Append('>');
                sb.Append(whoUser.DisplayName);
                sb.Append(@"</a>, ");
            }

            if(sb.Length > 2)
                sb.Length -= 2;

            return sb.ToString();
        }

        private static void WhoServer(IChatCommandContext ctx) {
            ctx.User.Send(new LegacyCommandResponse(LCR.USERS_LISTING_SERVER, false, MakeUserList(ctx.User, ctx.Chat.Users.All())));
        }

        private static void WhoChannel(IChatCommandContext ctx, string channelName) {
            ChatChannel whoChan = ctx.Chat.Channels.Get(channelName);

            if(whoChan == null)
                throw new CommandException(LCR.CHANNEL_NOT_FOUND, channelName);

            if(whoChan.Rank > ctx.User.Rank || (whoChan.HasPassword && !ctx.User.Can(ChatUserPermissions.JoinAnyChannel)))
                throw new CommandException(LCR.USERS_LISTING_ERROR, channelName);

            ctx.User.Send(new LegacyCommandResponse(LCR.USERS_LISTING_CHANNEL, false, whoChan.Name, MakeUserList(ctx.User, whoChan.GetUsers())));
        }

        public IChatMessageEvent Dispatch(IChatCommandContext ctx) {
            string channelName = ctx.Args.ElementAtOrDefault(1) ?? string.Empty;

            if(string.IsNullOrEmpty(channelName))
                WhoServer(ctx);
            else
                WhoChannel(ctx, channelName);

            return null;
        }
    }
}
