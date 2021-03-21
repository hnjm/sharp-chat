using SharpChat.Messages;
using SharpChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class BroadcastCommand : ICommand {
        private const string NAME = @"say";

        private IUser Sender { get; }
        private MessageManager Messages { get; }

        public BroadcastCommand(MessageManager messages, IUser sender) {
            Messages = messages ?? throw new ArgumentNullException(nameof(messages));
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == NAME;

        public bool DispatchCommand(ICommandContext ctx) {
            if(!ctx.User.Can(UserPermissions.Broadcast))
                throw new CommandNotAllowedException(NAME);

            ctx.Chat.SendPacket(new BroadcastMessagePacket(Sender, string.Join(' ', ctx.Args.Skip(1))));
            return true;
        }
    }
}
