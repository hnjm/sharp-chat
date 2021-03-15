using SharpChat.Events;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class ActionCommand : ICommand {
        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"action" || name == @"me";

        public IMessageEvent DispatchCommand(ICommandContext ctx) {
            if(ctx.Args.Count() < 2)
                return null;

            return new MessageCreateEvent(ctx.Channel, ctx.User, string.Join(' ', ctx.Args.Skip(1)), true);
        }
    }
}
