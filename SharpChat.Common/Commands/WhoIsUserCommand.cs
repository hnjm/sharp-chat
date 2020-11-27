using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SharpChat.Commands {
    public class WhoIsUserCommand : IChatCommand {
        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"ip" || name == @"whois";

        public IChatMessageEvent DispatchCommand(IChatCommandContext ctx) {
            if(!ctx.User.Can(ChatUserPermissions.SeeIPAddress))
                throw new CommandException(LCR.COMMAND_NOT_ALLOWED, $@"/{ctx.Args.First()}");

            string userName = ctx.Args.ElementAtOrDefault(1);
            ChatUser user = null;
            if(string.IsNullOrEmpty(userName) || (user = ctx.Chat.Users.Get(userName)) == null)
                throw new CommandException(LCR.USER_NOT_FOUND, user?.Username ?? userName ?? @"User");

            foreach(IPAddress ip in user.RemoteAddresses)
                ctx.User.Send(new LegacyCommandResponse(LCR.IP_ADDRESS, false, user.Username, ip));
            return null;
        }
    }
}
