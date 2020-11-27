using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class BanListCommand : IChatCommand {
        public bool IsMatch(string name, IEnumerable<string> args)
            => name == @"bans" || name == @"banned";

        public IChatMessageEvent Dispatch(IChatCommandContext ctx) {
            if(!ctx.User.Can(ChatUserPermissions.BanUser | ChatUserPermissions.KickUser))
                throw new CommandException(LCR.COMMAND_NOT_ALLOWED, $@"/{ctx.Args.First()}");
            ctx.User.Send(new BanListPacket(ctx.Chat.Bans.All()));
            return null;
        }
    }
}
