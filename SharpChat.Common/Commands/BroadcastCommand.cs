using SharpChat.Messages;
using SharpChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class BroadcastCommand : ICommand {
        private const string NAME = @"say";

        private ChatContext Context { get; }

        public BroadcastCommand(ChatContext context) {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == NAME;

        public bool DispatchCommand(ICommandContext ctx) {
            if(!ctx.User.Can(UserPermissions.Broadcast))
                throw new CommandNotAllowedException(NAME);

            Context.BroadcastMessage(string.Join(' ', ctx.Args.Skip(1)));
            return true;
        }
    }
}
