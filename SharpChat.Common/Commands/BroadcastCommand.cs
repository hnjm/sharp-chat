using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class BroadcastCommand : IChatCommand {
        private const string NAME = @"say";

        private IUser Sender { get; }

        public BroadcastCommand(IUser sender) {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == NAME;

        public IMessageEvent DispatchCommand(IChatCommandContext ctx) {
            if(!ctx.User.Can(UserPermissions.Broadcast))
                throw new CommandNotAllowedException(NAME);

            ctx.Chat.Send(new BroadcastMessagePacket(Sender, string.Join(' ', ctx.Args.Skip(1))));
            return null;
        }
    }
}
