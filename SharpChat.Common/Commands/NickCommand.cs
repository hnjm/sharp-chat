using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class NickCommand : IChatCommand {
        public bool IsMatch(string name, IEnumerable<string> args)
            => name == @"nick";

        public IChatMessageEvent Dispatch(IChatCommandContext ctx) {
            bool setOthersNick = ctx.User.Can(ChatUserPermissions.SetOthersNickname);

            if(!setOthersNick && !ctx.User.Can(ChatUserPermissions.SetOwnNickname))
                throw new CommandException(LCR.COMMAND_NOT_ALLOWED, @"/nick");

            ChatUser targetUser = null;
            int offset = 1;

            if(setOthersNick && long.TryParse(ctx.Args.ElementAtOrDefault(1), out long targetUserId) && targetUserId > 0) {
                targetUser = ctx.Chat.Users.Get(targetUserId);
                offset = 2;
            }

            if(targetUser == null)
                targetUser = ctx.User;

            if(ctx.Args.Count() < offset)
                throw new CommandException(LCR.COMMAND_FORMAT_ERROR);

            string nickStr = string.Join('_', ctx.Args.Skip(offset))
                .Replace(' ', '_')
                .Replace("\n", string.Empty)
                .Replace("\r", string.Empty)
                .Replace("\f", string.Empty)
                .Replace("\t", string.Empty)
                .Trim();

            if(nickStr == targetUser.Username)
                nickStr = null;
            else if(nickStr.Length > 15)
                nickStr = nickStr.Substring(0, 15);
            else if(string.IsNullOrEmpty(nickStr))
                nickStr = null;

            if(nickStr != null && ctx.Chat.Users.Get(nickStr) != null)
                throw new CommandException(LCR.NAME_IN_USE, nickStr);

            string previousName = targetUser == ctx.User ? (targetUser.Nickname ?? targetUser.Username) : null;
            targetUser.Nickname = nickStr;
            ctx.Channel.Send(new UserUpdatePacket(targetUser, previousName));
            return null;
        }
    }
}
