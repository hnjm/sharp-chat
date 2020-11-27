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
        public bool IsMatch(string name, IEnumerable<string> args)
            => name == @"pardonip" || name == @"unbanip";

        public IChatMessageEvent Dispatch(IChatCommandContext ctx) {
            if(!ctx.User.Can(ChatUserPermissions.BanUser | ChatUserPermissions.KickUser))
                throw new CommandException(LCR.COMMAND_NOT_ALLOWED, $@"/{ctx.Args.First()}");

            string ipAddress = ctx.Args.ElementAtOrDefault(1);
            BannedIPAddress banRecord = null;
            if(!IPAddress.TryParse(ipAddress, out IPAddress unbanIP)
                || (banRecord = ctx.Chat.Bans.GetIPAddress(unbanIP)) == null
                || banRecord.Expires <= DateTimeOffset.UtcNow)
                throw new CommandException(LCR.USER_NOT_BANNED, banRecord?.Address.ToString() ?? ipAddress ?? @"::");

            ctx.Chat.Bans.Remove(banRecord.Address);
            ctx.User.Send(new LegacyCommandResponse(LCR.USER_UNBANNED, false, banRecord));
            return null;
        }
    }
}
