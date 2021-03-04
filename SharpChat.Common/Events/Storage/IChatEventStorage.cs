using SharpChat.Channels;
using System.Collections.Generic;

namespace SharpChat.Events.Storage {
    public interface IChatEventStorage : IEventHandler {
        bool RemoveEvent(IEvent evt);
        IEvent GetEvent(long seqId);
        IEnumerable<IEvent> GetEventsForTarget(Channel target, int amount = 20, int offset = 0);
    }
}
