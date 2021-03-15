using System.Collections.Generic;

namespace SharpChat.Events.Storage {
    public interface IEventStorage : IEventHandler {
        bool RemoveEvent(IEvent evt);
        IEvent GetEvent(long seqId);
        IEnumerable<IEvent> GetEventsForTarget(IEventTarget target, int amount = 20, int offset = 0);
        void RegisterConstructor(string type, IEvent.DecodeFromJson construct);
    }
}
