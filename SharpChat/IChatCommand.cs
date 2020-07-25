using SharpChat.Events;

namespace SharpChat {
    public interface IChatCommand {
        bool IsMatch(string name);
        IChatMessage Dispatch(IChatCommandContext context);
    }
}
