using SharpChat.Bans;
using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class PardonUserCommand : IChatCommand {
        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"pardon" || name == @"unban";

        public IChatMessageEvent DispatchCommand(IChatCommandContext ctx) {
            if(!ctx.User.Can(ChatUserPermissions.BanUser | ChatUserPermissions.KickUser))
                throw new CommandException(LCR.COMMAND_NOT_ALLOWED, $@"/{ctx.Args.First()}");

            string userName = ctx.Args.ElementAtOrDefault(1);
            BannedUser banRecord = null;

            if(string.IsNullOrEmpty(userName)
                || (banRecord = ctx.Chat.Bans.GetUser(userName)) == null
                || banRecord.Expires <= DateTimeOffset.Now)
                throw new CommandException(LCR.USER_NOT_BANNED, banRecord?.Username ?? userName ?? @"User");

            ctx.Chat.Bans.Remove(banRecord);
            ctx.User.Send(new LegacyCommandResponse(LCR.USER_UNBANNED, false, banRecord));
            return null;
        }
    }
}
