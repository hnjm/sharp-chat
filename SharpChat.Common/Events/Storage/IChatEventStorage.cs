using System;
using System.Collections.Generic;

namespace SharpChat.Events.Storage {
    public interface IChatEventStorage : IDisposable {
        void AddEvent(IChatEvent evt);
        bool RemoveEvent(IChatEvent evt);
        IChatEvent GetEvent(long seqId);
        IEnumerable<IChatEvent> GetEventsForTarget(IPacketTarget target, int amount = 20, int offset = 0);
    }
}
