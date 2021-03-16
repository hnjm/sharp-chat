using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;

namespace SharpChat.Commands {
    public class BanListCommand : ICommand {
        private IUser Sender { get; }

        public BanListCommand(IUser sender) {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"bans" || name == @"banned";

        public MessageCreateEvent DispatchCommand(ICommandContext ctx) {
            if(!ctx.User.Can(UserPermissions.BanUser | UserPermissions.KickUser))
                throw new CommandNotAllowedException(ctx.Args);

            ctx.Chat.DataProvider.BanClient.GetBanList(b => {
                ctx.Session.SendPacket(new BanListPacket(Sender, b));
            }, ex => {
                Logger.Write(@"Error during ban list retrieval.");
                Logger.Write(ex);
                ctx.Session.SendPacket(new CommandGenericException().ToPacket(Sender));
            });

            return null;
        }
    }
}
