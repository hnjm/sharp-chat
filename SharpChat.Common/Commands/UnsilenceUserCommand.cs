using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class UnsilenceUserCommand : ICommand {
        private IUser Sender { get; }

        public UnsilenceUserCommand(IUser sender) {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"unsilence";

        public MessageCreateEvent DispatchCommand(ICommandContext ctx) {
            if(!ctx.User.Can(UserPermissions.SilenceUser))
                throw new CommandNotAllowedException(ctx.Args);

            string userName = ctx.Args.ElementAtOrDefault(1);
            ChatUser user;
            if(string.IsNullOrEmpty(userName) || (user = ctx.Chat.Users.Get(userName)) == null)
                throw new UserNotFoundCommandException(userName);

            if(user.Rank >= ctx.User.Rank)
                throw new RevokeSilenceNotAllowedCommandException();

            if(!user.IsSilenced)
                throw new NotSilencedCommandException();

            user.SilencedUntil = DateTimeOffset.MinValue;
            user.SendPacket(new SilenceRevokeNoticePacket(Sender));
            ctx.Session.SendPacket(new SilenceRevokeResponsePacket(Sender, user));
            return null;
        }
    }
}
