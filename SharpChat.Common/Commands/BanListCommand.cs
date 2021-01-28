using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class BanListCommand : IChatCommand {
        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"bans" || name == @"banned";

        public IChatMessageEvent DispatchCommand(IChatCommandContext ctx) {
            if(!ctx.User.Can(UserPermissions.BanUser | UserPermissions.KickUser))
                throw new CommandException(LCR.COMMAND_NOT_ALLOWED, $@"/{ctx.Args.First()}");
            ctx.User.Send(new BanListPacket(ctx.Chat.Bans.All()));
            return null;
        }
    }
}
