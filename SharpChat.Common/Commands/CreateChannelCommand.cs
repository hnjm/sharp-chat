﻿using SharpChat.Channels;
using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class CreateChannelCommand : ICommand {
        private const string NAME = @"create";

        private IUser Sender { get; }

        public CreateChannelCommand(IUser sender) {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == NAME;

        public IMessageEvent DispatchCommand(ICommandContext ctx) {
            if(!ctx.User.Can(UserPermissions.CreateChannel))
                throw new CommandNotAllowedException(NAME);

            bool hasRank;
            if(ctx.Args.Count() < 2 || (hasRank = ctx.Args.ElementAtOrDefault(1)?.All(char.IsDigit) == true && ctx.Args.Count() < 3))
                throw new CommandFormatException();

            int rank = 0;
            if(hasRank && !int.TryParse(ctx.Args.ElementAtOrDefault(1), out rank) && rank < 0)
                rank = 0;

            if(rank > ctx.User.Rank)
                throw new InsufficientRankForChangeCommandException();

            string createChanName = string.Join('_', ctx.Args.Skip(hasRank ? 2 : 1));
            Channel createChan;

            try {
                createChan = ctx.Chat.Channels.Create(
                    ctx.User,
                    createChanName,
                    !ctx.User.Can(UserPermissions.SetChannelPermanent),
                    rank
                );
            } catch(ChannelExistException) {
                throw new ChannelExistsCommandException(createChanName);
            } catch(ChannelInvalidNameException) {
                throw new ChannelNameInvalidCommandException();
            }

            ctx.Chat.SwitchChannel(ctx.Session, createChan);
            ctx.Session.SendPacket(new ChannelCreateResponsePacket(Sender, createChan));
            return null;
        }
    }
}
