using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SharpChat.Commands {
    public class WhoIsUserCommand : IChatCommand {
        private IUser Sender { get; }

        public WhoIsUserCommand(IUser sender) {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"ip" || name == @"whois";

        public IMessageEvent DispatchCommand(IChatCommandContext ctx) {
            if(!ctx.User.Can(UserPermissions.SeeIPAddress))
                throw new CommandNotAllowedException(ctx.Args);

            string userName = ctx.Args.ElementAtOrDefault(1);
            ChatUser user = null;
            if(string.IsNullOrEmpty(userName) || (user = ctx.Chat.Users.Get(userName)) == null)
                throw new UserNotFoundCommandException(user?.UserName ?? userName);

            foreach(IPAddress ip in user.RemoteAddresses)
                ctx.Session.Send(new WhoIsResponsePacket(Sender, user, ip));

            return null;
        }
    }
}
