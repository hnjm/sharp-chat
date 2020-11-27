using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class SilenceUserCommand : IChatCommand {
        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"silence";

        public IChatMessageEvent DispatchCommand(IChatCommandContext ctx) {
            if(!ctx.User.Can(ChatUserPermissions.SilenceUser))
                throw new CommandException(LCR.COMMAND_NOT_ALLOWED, $@"/{ctx.Args.First()}");

            string userName = ctx.Args.ElementAtOrDefault(1);
            ChatUser user = null;
            if(string.IsNullOrEmpty(userName)
                || (user = ctx.Chat.Users.Get(userName)) == null)
                throw new CommandException(LCR.USER_NOT_FOUND, userName ?? @"User");

            if(user == ctx.User)
                throw new CommandException(LCR.SILENCE_SELF);
            if(user.Rank >= user.Rank)
                throw new CommandException(LCR.SILENCE_RANK);
            if(user.IsSilenced)
                throw new CommandException(LCR.SILENCE_ALREADY);

            string durationArg = ctx.Args.ElementAtOrDefault(2);
            DateTimeOffset duration = DateTimeOffset.MaxValue;

            if(string.IsNullOrEmpty(durationArg)) {
                if(!double.TryParse(durationArg, out double durationRaw))
                    throw new CommandException(LCR.COMMAND_FORMAT_ERROR);
                duration = DateTimeOffset.UtcNow.AddSeconds(durationRaw);
            }

            user.SilencedUntil = duration;
            user.Send(new LegacyCommandResponse(LCR.SILENCED, false));
            ctx.User.Send(new LegacyCommandResponse(LCR.TARGET_SILENCED, false, user.Username));
            return null;
        }
    }
}
