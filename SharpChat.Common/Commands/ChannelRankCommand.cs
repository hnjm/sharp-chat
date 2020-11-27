using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class ChannelRankCommand : IChatCommand {
        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"rank" || name == @"hierarchy" || name == @"priv";

        public IChatMessageEvent DispatchCommand(IChatCommandContext ctx) {
            if(!ctx.User.Can(ChatUserPermissions.SetChannelHierarchy) || ctx.Channel.Owner != ctx.User)
                throw new CommandException(LCR.COMMAND_NOT_ALLOWED, $@"/{ctx.Args.First()}");

            if(!int.TryParse(ctx.Args.ElementAtOrDefault(1), out int rank) || rank > ctx.User.Rank)
                throw new CommandException(LCR.INSUFFICIENT_RANK);

            ctx.Chat.Channels.Update(ctx.Channel, rank: rank);
            ctx.User.Send(new LegacyCommandResponse(LCR.CHANNEL_HIERARCHY_CHANGED, false));
            return null;
        }
    }
}
