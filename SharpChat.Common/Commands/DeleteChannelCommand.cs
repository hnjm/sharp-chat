using SharpChat.Channels;
using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class DeleteChannelCommand : IChatCommand {
        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"delchan" || (name == @"delete" && args.ElementAtOrDefault(1)?.All(char.IsDigit) == false);

        public IChatMessageEvent DispatchCommand(IChatCommandContext ctx) {
            string channelName = string.Join('_', ctx.Args.Skip(1));

            if(string.IsNullOrWhiteSpace(channelName))
                throw new CommandException(LCR.COMMAND_FORMAT_ERROR);

            ChatChannel channel = ctx.Chat.Channels.Get(channelName);
            if(channel == null)
                throw new CommandException(LCR.CHANNEL_NOT_FOUND, channelName);

            if(!ctx.User.Can(ChatUserPermissions.DeleteChannel) && channel.Owner != ctx.User)
                throw new CommandException(LCR.CHANNEL_DELETE_FAILED, channel.Name);

            ctx.Chat.Channels.Remove(channel);
            ctx.User.Send(new LegacyCommandResponse(LCR.CHANNEL_DELETED, false, channel.Name));
            return null;
        }
    }
}
