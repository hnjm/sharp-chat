using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class ChannelPasswordCommand : ICommand {
        private IUser Sender { get; }

        public ChannelPasswordCommand(IUser sender) {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"password" || name == @"pwd";

        public MessageCreateEvent DispatchCommand(ICommandContext ctx) {
            if(!ctx.User.Can(UserPermissions.SetChannelPassword) || ctx.Channel.Owner != ctx.User)
                throw new CommandNotAllowedException(ctx.Args);

            string password = string.Join(' ', ctx.Args.Skip(1)).Trim();

            if(string.IsNullOrWhiteSpace(password))
                password = string.Empty;

            ctx.Chat.Channels.Update(ctx.Channel, password: password);
            ctx.Session.SendPacket(new ChannelPasswordResponsePacket(Sender));
            return null;
        }
    }
}
