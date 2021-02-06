using SharpChat.Bans;
using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class PardonUserCommand : IChatCommand {
        private IUser Sender { get; }

        public PardonUserCommand(IUser sender) {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"pardon" || name == @"unban";

        public IMessageEvent DispatchCommand(IChatCommandContext ctx) {
            if(!ctx.User.Can(UserPermissions.BanUser | UserPermissions.KickUser))
                throw new CommandNotAllowedException(ctx.Args);

            string userName = ctx.Args.ElementAtOrDefault(1);
            BannedUser banRecord = null;

            if(string.IsNullOrEmpty(userName)
                || (banRecord = ctx.Chat.Bans.GetUser(userName)) == null
                || banRecord.Expires <= DateTimeOffset.Now)
                throw new NotBannedCommandException(banRecord?.Username ?? userName ?? @"User");

            ctx.Chat.Bans.Remove(banRecord);
            ctx.Session.Send(new PardonResponsePacket(Sender, banRecord));
            return null;
        }
    }
}
