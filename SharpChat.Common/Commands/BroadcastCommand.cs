using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System.Linq;

namespace SharpChat.Commands {
    public class BroadcastCommand : IChatCommand {
        public bool IsMatch(string name)
            => name == @"say";

        public IChatMessageEvent Dispatch(IChatCommandContext ctx) {
            if(!ctx.User.Can(ChatUserPermissions.Broadcast))
                throw new CommandException(LCR.COMMAND_NOT_ALLOWED, @"/say");

            ctx.Chat.Send(new LegacyCommandResponse(LCR.BROADCAST, false, string.Join(' ', ctx.Args.Skip(1))));
            return null;
        }
    }
}
