using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;

namespace SharpChat.Commands {
    public class BanListCommand : IChatCommand {
        private IUser Sender { get; }

        public BanListCommand(IUser sender) {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"bans" || name == @"banned";

        public IMessageEvent DispatchCommand(IChatCommandContext ctx) {
            if(!ctx.User.Can(UserPermissions.BanUser | UserPermissions.KickUser))
                throw new CommandNotAllowedException(ctx.Args);
            ctx.Session.Send(new BanListPacket(Sender, ctx.Chat.Bans.All()));
            return null;
        }
    }
}
