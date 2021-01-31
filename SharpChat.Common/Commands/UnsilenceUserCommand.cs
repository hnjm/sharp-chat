using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class UnsilenceUserCommand : IChatCommand {
        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"unsilence";

        public IMessageEvent DispatchCommand(IChatCommandContext ctx) {
            if(!ctx.User.Can(UserPermissions.SilenceUser))
                throw new CommandException(LCR.COMMAND_NOT_ALLOWED, $@"/{ctx.Args.First()}");

            string userName = ctx.Args.ElementAtOrDefault(1);
            ChatUser user = null;
            if(string.IsNullOrEmpty(userName)
                || (user = ctx.Chat.Users.Get(userName)) == null)
                throw new CommandException(LCR.USER_NOT_FOUND, userName ?? @"User");

            if(user.Rank >= ctx.User.Rank)
                throw new CommandException(LCR.UNSILENCE_RANK);

            if(!user.IsSilenced)
                throw new CommandException(LCR.NOT_SILENCED);

            user.SilencedUntil = DateTimeOffset.MinValue;
            user.Send(new LegacyCommandResponse(LCR.UNSILENCED, false));
            ctx.User.Send(new LegacyCommandResponse(LCR.TARGET_UNSILENCED, false, user.DisplayName));
            return null;
        }
    }
}
