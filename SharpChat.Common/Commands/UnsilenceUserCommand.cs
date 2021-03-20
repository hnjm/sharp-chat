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

        public bool DispatchCommand(ICommandContext ctx) {
            if(!ctx.User.Can(UserPermissions.SilenceUser))
                throw new CommandNotAllowedException(ctx.Args);

            string userName = ctx.Args.ElementAtOrDefault(1);
            IUser user;
            if(string.IsNullOrEmpty(userName) || (user = ctx.Chat.Users.GetUser(userName)) == null)
                throw new UserNotFoundCommandException(userName);

            if(user.Rank >= ctx.User.Rank)
                throw new RevokeSilenceNotAllowedCommandException();

            //if(!user.IsSilenced)
            //    throw new NotSilencedCommandException();

            //ctx.Chat.Users.RevokeSilence(user);

            user.SendPacket(new SilenceRevokeNoticePacket(Sender));
            ctx.Session.SendPacket(new SilenceRevokeResponsePacket(Sender, user));
            return true;
        }
    }
}
