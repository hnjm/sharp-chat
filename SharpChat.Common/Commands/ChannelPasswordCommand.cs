using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class ChannelPasswordCommand : IChatCommand {
        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"password" || name == @"pwd";

        public IMessageEvent DispatchCommand(IChatCommandContext ctx) {
            if(!ctx.User.Can(UserPermissions.SetChannelPassword) || ctx.Channel.Owner != ctx.User)
                throw new CommandException(LCR.COMMAND_NOT_ALLOWED, $@"/{ctx.Args.First()}");

            string password = string.Join(' ', ctx.Args.Skip(1)).Trim();

            if(string.IsNullOrWhiteSpace(password))
                password = string.Empty;

            ctx.Chat.Channels.Update(ctx.Channel, password: password);
            ctx.User.Send(new LegacyCommandResponse(LCR.CHANNEL_PASSWORD_CHANGED, false));
            return null;
        }
    }
}
