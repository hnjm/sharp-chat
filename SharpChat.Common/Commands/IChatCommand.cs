using SharpChat.Events;
using System.Collections.Generic;

namespace SharpChat.Commands {
    public interface IChatCommand {
        bool IsCommandMatch(string name, IEnumerable<string> args);
        IMessageEvent DispatchCommand(IChatCommandContext ctx);
    }
}
