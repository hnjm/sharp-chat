﻿using SharpChat.Channels;
using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class WhoCommand : IChatCommand {
        private IUser Sender { get; }

        public WhoCommand(IUser sender) {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"who";

        private static void WhoServer(IUser sender, IChatCommandContext ctx) {
            ctx.User.Send(new UserListResponsePacket(sender, ctx.User, ctx.Chat.Users.All()));
        }

        private static void WhoChannel(IUser sender, IChatCommandContext ctx, string channelName) {
            Channel whoChan = ctx.Chat.Channels.Get(channelName);

            if(whoChan == null)
                throw new ChannelNotFoundCommandException(channelName);

            if(whoChan.MinimumRank > ctx.User.Rank || (whoChan.HasPassword && !ctx.User.Can(UserPermissions.JoinAnyChannel)))
                throw new UserListChannelNotFoundCommandException(channelName);

            ctx.User.Send(new UserListResponsePacket(sender, whoChan, ctx.User, whoChan.GetUsers()));
        }

        public IMessageEvent DispatchCommand(IChatCommandContext ctx) {
            string channelName = ctx.Args.ElementAtOrDefault(1) ?? string.Empty;

            if(string.IsNullOrEmpty(channelName))
                WhoServer(Sender, ctx);
            else
                WhoChannel(Sender, ctx, channelName);

            return null;
        }
    }
}
