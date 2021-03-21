using SharpChat.Channels;
using SharpChat.Users;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class JoinCommand : ICommand {
        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"join";

        public bool DispatchCommand(ICommandContext ctx) {
            string channelName = ctx.Args.ElementAtOrDefault(1);

            // no error, apparently
            if(string.IsNullOrWhiteSpace(channelName))
                return false;

            IChannel channel = ctx.Chat.Channels.GetChannel(channelName);

            // the original server sends ForceChannel before sending the error message, but this order probably makes more sense.

            if(channel == null) {
                ctx.Session.ForceChannel();
                throw new ChannelNotFoundCommandException(channelName);
            }

            if(ctx.Chat.ChannelUsers.HasUser(channel, ctx.User)) {
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

            if(ctx.Session.LastChannel != null)
                ctx.Chat.ChannelUsers.LeaveChannel(ctx.Session.LastChannel, ctx.User, UserDisconnectReason.Leave);
            ctx.Chat.ChannelUsers.JoinChannel(channel, ctx.User);
            return true;
        }
    }
}
