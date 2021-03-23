using SharpChat.DataProvider;
using SharpChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class PardonUserCommand : ICommand {
        private IDataProvider DataProvider { get; }
        private IUser Sender { get; }

        public PardonUserCommand(IDataProvider dataProvider, IUser sender) {
            DataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"pardon" || name == @"unban";

        public bool DispatchCommand(ICommandContext ctx) {
            if(!ctx.User.Can(UserPermissions.BanUser | UserPermissions.KickUser))
                throw new CommandNotAllowedException(ctx.Args);

            string userName = ctx.Args.ElementAtOrDefault(1);
            if(string.IsNullOrEmpty(userName))
                throw new NotBannedCommandException(userName ?? @"User");

            DataProvider.BanClient.RemoveBan(userName, success => {
                if(success)
                    ctx.Session.SendPacket(new PardonResponsePacket(Sender, userName));
                else
                    ctx.Session.SendPacket(new NotBannedCommandException(userName).ToPacket(Sender));
            }, ex => ctx.Session.SendPacket(new CommandGenericException().ToPacket(Sender)));

            return true;
        }
    }
}
