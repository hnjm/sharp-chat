using SharpChat.Channels;
using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class WhoCommand : ICommand {
        private IUser Sender { get; }

        public WhoCommand(IUser sender) {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"who";

        private static void WhoServer(IUser sender, ICommandContext ctx) {
            ctx.Session.SendPacket(new UserListResponsePacket(sender, ctx.User, ctx.Chat.Users.All()));
        }

        private static void WhoChannel(IUser sender, ICommandContext ctx, string channelName) {
            IChannel whoChan = ctx.Chat.Channels.Get(channelName);

            if(whoChan == null)
                throw new ChannelNotFoundCommandException(channelName);

            if(whoChan.MinimumRank > ctx.User.Rank || (whoChan.HasPassword && !ctx.User.Can(UserPermissions.JoinAnyChannel)))
                throw new UserListChannelNotFoundCommandException(channelName);

            whoChan.GetUsers(
                users => ctx.Session.SendPacket(new UserListResponsePacket(
                    sender,
                    whoChan,
                    ctx.User,
                    users.OrderByDescending(u => u.Rank)
                ))
            );
        }

        public MessageCreateEvent DispatchCommand(ICommandContext ctx) {
            string channelName = ctx.Args.ElementAtOrDefault(1) ?? string.Empty;

            if(string.IsNullOrEmpty(channelName))
                WhoServer(Sender, ctx);
            else
                WhoChannel(Sender, ctx, channelName);

            return null;
        }
    }
}
