using System.Collections.Generic;

namespace SharpChat.Commands {
    public class LeaveCommand : ICommand {
        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"leave";

        public bool DispatchCommand(ICommandContext ctx) {
            if(!ctx.Session.HasCapability(ClientCapabilities.MCHAN))
                throw new CommandNotFoundException(@"leave");

            // figure out the channel leaving logic
            // should i postpone this implementation till i have the event based shit in place?

            return true;
        }
    }
}
