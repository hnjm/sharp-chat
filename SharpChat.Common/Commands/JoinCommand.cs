using SharpChat.Channels;
using SharpChat.Events;
using SharpChat.Packets;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class JoinCommand : IChatCommand {
        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"join";

        public IMessageEvent DispatchCommand(IChatCommandContext ctx) {
            string channelName = ctx.Args.ElementAtOrDefault(1);

            // no error, apparently
            if(string.IsNullOrWhiteSpace(channelName))
                return null;

            Channel channel = ctx.Chat.Channels.Get(channelName);

            if(channel == null) {
                ctx.User.Send(new LegacyCommandResponse(LCR.CHANNEL_NOT_FOUND, true, channelName));
                ctx.User.ForceChannel();
                return null;
            }

            string password = string.Join(' ', ctx.Args.Skip(2));
            ctx.Chat.SwitchChannel(ctx.User, channel, password);

            return null;
        }
    }
}
