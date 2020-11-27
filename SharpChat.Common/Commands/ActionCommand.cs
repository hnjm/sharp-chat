using SharpChat.Events;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class ActionCommand : IChatCommand {
        public bool IsMatch(string name, IEnumerable<string> args)
            => name == @"action" || name == @"me";

        public IChatMessageEvent Dispatch(IChatCommandContext ctx) {
            if(ctx.Args.Count() < 2)
                return null;

            return new ChatMessageEvent(ctx.User, ctx.Channel, string.Join(' ', ctx.Args.Skip(1)), ChatEventFlags.Action);
        }
    }
}
