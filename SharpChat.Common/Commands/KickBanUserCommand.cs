using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class KickBanUserCommand : IChatCommand {
        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"kick" || name == @"ban";

        public IMessageEvent DispatchCommand(IChatCommandContext ctx) {
            string commandName = ctx.Args.First();
            bool isBan = commandName == @"ban";

            if(!ctx.User.Can(isBan ? UserPermissions.BanUser : UserPermissions.KickUser))
                throw new CommandException(LCR.COMMAND_NOT_ALLOWED, $@"/{commandName}");

            string userName = ctx.Args.ElementAtOrDefault(1);
            ChatUser user;
            if(userName == null || (user = ctx.Chat.Users.Get(userName)) == null)
                throw new CommandException(LCR.USER_NOT_FOUND, userName ?? @"User");

            if(user == ctx.User || user.Rank >= ctx.User.Rank || ctx.Chat.Bans.Check(user) > DateTimeOffset.Now)
                throw new CommandException(LCR.KICK_NOT_ALLOWED, user.UserName);

            string durationArg = ctx.Args.ElementAtOrDefault(2);
            DateTimeOffset? duration = isBan ? (DateTimeOffset?)DateTimeOffset.MaxValue : null;

            if(!string.IsNullOrEmpty(durationArg)) {
                if(!double.TryParse(durationArg, out double durationRaw))
                    throw new CommandException(LCR.COMMAND_FORMAT_ERROR);
                duration = DateTimeOffset.Now.AddSeconds(durationRaw);
            }

            ctx.Chat.BanUser(user, duration, isBan);
            return null;
        }
    }
}
