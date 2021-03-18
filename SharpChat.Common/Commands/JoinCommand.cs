using SharpChat.Channels;
using SharpChat.Events;
using SharpChat.Users;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class JoinCommand : ICommand {
        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"join";

        public MessageCreateEvent DispatchCommand(ICommandContext ctx) {
            string channelName = ctx.Args.ElementAtOrDefault(1);

            // no error, apparently
            if(string.IsNullOrWhiteSpace(channelName))
                return null;

            IChannel channel = ctx.Chat.Channels.Get(channelName);

            // the original server sends ForceChannel before sending the error message, but this order probably makes more sense.

            if(channel == null) {
                ctx.Session.ForceChannel();
                throw new ChannelNotFoundCommandException(channelName);
            }

            if(ctx.User.InChannel(channel)) {
                ctx.Session.ForceChannel();
                throw new AlreadyInChannelCommandException(channel);
            }

            string password = string.Join(' ', ctx.Args.Skip(2));

            if(!ctx.User.Can(UserPermissions.JoinAnyChannel) && channel.Owner != ctx.User) {
                if(channel.MinimumRank > ctx.User.Rank) {
                    ctx.Session.ForceChannel();
                    throw new ChannelRankCommandException(channel);
                }

                if(channel.VerifyPassword(password)) {
                    ctx.Session.ForceChannel();
                    throw new ChannelPasswordCommandException(channel);
                }
            }

            ctx.Chat.SwitchChannel(ctx.Session, channel);

            return null;
        }
    }
}
