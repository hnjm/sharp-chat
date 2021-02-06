using SharpChat.Events;
using System.Collections.Generic;

namespace SharpChat.Commands {
    public class LeaveCommand : IChatCommand {
        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"leave";

        public IMessageEvent DispatchCommand(IChatCommandContext ctx) {
            if(!ctx.Session.HasCapability(ClientCapabilities.MCHAN))
                throw new CommandNotFoundException(@"leave");

            // figure out the channel leaving logic
            // should i postpone this implementation till i have the event based shit in place?

            return null;
        }
    }
}
