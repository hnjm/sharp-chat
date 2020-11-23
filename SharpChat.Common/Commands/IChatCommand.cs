using SharpChat.Events;

namespace SharpChat.Commands {
    public interface IChatCommand {
        bool IsMatch(string name);
        IChatMessage Dispatch(IChatCommandContext context);
    }
}
