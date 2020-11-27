using SharpChat.Events;
using System.Collections.Generic;

namespace SharpChat.Commands {
    public interface IChatCommand {
        bool IsMatch(string name, IEnumerable<string> args);
        IChatMessageEvent Dispatch(IChatCommandContext ctx);
    }
}
