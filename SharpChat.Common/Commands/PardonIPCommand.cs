using SharpChat.Bans;
using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SharpChat.Commands {
    public class PardonIPCommand : IChatCommand {
        private IUser Sender { get; }

        public PardonIPCommand(IUser sender) {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"pardonip" || name == @"unbanip";

        public IMessageEvent DispatchCommand(IChatCommandContext ctx) {
            if(!ctx.User.Can(UserPermissions.BanUser | UserPermissions.KickUser))
                throw new CommandNotAllowedException(ctx.Args);

            string ipAddress = ctx.Args.ElementAtOrDefault(1);
            BannedIPAddress banRecord = null;
            if(!IPAddress.TryParse(ipAddress, out IPAddress unbanIP)
                || (banRecord = ctx.Chat.Bans.GetIPAddress(unbanIP)) == null
                || banRecord.Expires <= DateTimeOffset.Now)
                throw new NotBannedCommandException(banRecord?.Address.ToString() ?? ipAddress ?? @"::");

            ctx.Chat.Bans.Remove(banRecord.Address);
            ctx.Session.Send(new PardonResponsePacket(Sender, banRecord));
            return null;
        }
    }
}
