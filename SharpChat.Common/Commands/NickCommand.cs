using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class NickCommand : ICommand {
        private const string NAME = @"nick";
        private IUser Sender { get; }

        public NickCommand(IUser sender) {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == NAME;

        public MessageCreateEvent DispatchCommand(ICommandContext ctx) {
            bool setOthersNick = ctx.User.Can(UserPermissions.SetOthersNickname);

            if(!setOthersNick && !ctx.User.Can(UserPermissions.SetOwnNickname))
                throw new CommandNotAllowedException(NAME);

            IUser targetUser = null;
            int offset = 1;

            if(setOthersNick && long.TryParse(ctx.Args.ElementAtOrDefault(1), out long targetUserId) && targetUserId > 0) {
                targetUser = ctx.Chat.Users.Get(targetUserId);
                offset = 2;
            }

            if(targetUser == null)
                targetUser = ctx.User;

            if(ctx.Args.Count() < offset)
                throw new CommandFormatException();

            string nickStr = string.Join('_', ctx.Args.Skip(offset))
                .Replace(' ', '_')
                .Replace("\n", string.Empty)
                .Replace("\r", string.Empty)
                .Replace("\f", string.Empty)
                .Replace("\t", string.Empty)
                .Trim();

            if(nickStr == targetUser.UserName)
                nickStr = null;
            else if(nickStr.Length > 15)
                nickStr = nickStr.Substring(0, 15);
            else if(string.IsNullOrEmpty(nickStr))
                nickStr = null;

            if(nickStr != null && ctx.Chat.Users.Get(nickStr) != null)
                throw new NickNameInUseCommandException(nickStr);

            string previousName = targetUser == ctx.User ? (targetUser.NickName ?? targetUser.UserName) : null;
            ctx.Chat.Users.Update(targetUser, nickName: nickStr);
            ctx.Channel.SendPacket(new UserNickChangePacket(Sender, previousName, targetUser.DisplayName));
            ctx.Channel.SendPacket(new UserUpdatePacket(targetUser));
            return null;
        }
    }
}
