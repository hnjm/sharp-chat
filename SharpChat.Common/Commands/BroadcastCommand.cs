using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class BroadcastCommand : IChatCommand {
        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"say";

        public IChatMessageEvent DispatchCommand(IChatCommandContext ctx) {
            if(!ctx.User.Can(ChatUserPermissions.Broadcast))
                throw new CommandException(LCR.COMMAND_NOT_ALLOWED, @"/say");

            ctx.Chat.Send(new LegacyCommandResponse(LCR.BROADCAST, false, string.Join(' ', ctx.Args.Skip(1))));
            return null;
        }
    }
}
