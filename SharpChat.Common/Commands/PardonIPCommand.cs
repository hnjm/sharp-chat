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
            if(!IPAddress.TryParse(ipAddress, out IPAddress ipAddr))
                throw new NotBannedCommandException(ipAddr?.ToString() ?? @"::");

            ctx.Chat.DataProvider.BanClient.RemoveBan(ipAddr, success => {
                if(success)
                    ctx.Session.SendPacket(new PardonResponsePacket(Sender, ipAddr));
                else
                    ctx.Session.SendPacket(new NotBannedCommandException(ipAddr.ToString()).ToPacket(Sender));
            }, ex => ctx.Session.SendPacket(new CommandGenericException().ToPacket(Sender)));

            return null;
        }
    }
}
