using SharpChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class SilenceUserCommand : ICommand {
        private IUser Sender { get; }

        public SilenceUserCommand(IUser sender) {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"silence";

        public bool DispatchCommand(ICommandContext ctx) {
            if(!ctx.User.Can(UserPermissions.SilenceUser))
                throw new CommandNotAllowedException(ctx.Args);

            string userName = ctx.Args.ElementAtOrDefault(1);
            IUser user;
            if(string.IsNullOrEmpty(userName) || (user = ctx.Chat.Users.GetUser(userName)) == null)
                throw new UserNotFoundCommandException(userName);

            if(user == ctx.User)
                throw new SelfSilenceCommandException();
            if(user.Rank >= user.Rank)
                throw new SilenceNotAllowedCommandException();
            //if(user.IsSilenced)
            //    throw new AlreadySilencedCommandException();

            string durationArg = ctx.Args.ElementAtOrDefault(2);

            if(!string.IsNullOrEmpty(durationArg)) {
                if(!double.TryParse(durationArg, out double durationRaw))
                    throw new CommandFormatException();
                //ctx.Chat.Users.Silence(user, TimeSpan.FromSeconds(durationRaw));
            } //else
                //ctx.Chat.Users.Silence(user);

            // UserManager
            //user.SendPacket(new SilenceNoticePacket(Sender));

            // Remain? Also UserManager?
            ctx.Session.SendPacket(new SilenceResponsePacket(Sender, user));
            return true;
        }
    }
}
