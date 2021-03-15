using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Events.Storage {
    public class MemoryChatEventStorage : IChatEventStorage {
        private object Lock { get; } = new object();
        private List<IEvent> Events { get; } = new List<IEvent>();

        public void HandleEvent(IEvent evt) {
            lock(Lock)
                Events.Add(evt);
        }

        public bool RemoveEvent(IEvent evt) {
            lock(Lock)
                return Events.Remove(evt);
        }

        public IEvent GetEvent(long seqId) {
            lock(Lock)
                return Events.FirstOrDefault(e => e.EventId == seqId);
        }

        public IEnumerable<IEvent> GetEventsForTarget(IEventTarget target, int amount = 20, int offset = 0) {
            lock(Lock) {
                IEnumerable<IEvent> subset = Events.Where(e => e.Target == target.TargetName || e.Target == null);

                int start = subset.Count() - offset - amount;

                if(start < 0) {
                    amount += start;
                    start = 0;
                }

                return subset.Skip(start).Take(amount).ToList();
            }
        }

        public void RegisterConstructor(string type, IEvent.DecodeFromJson construct) {
            // No need to register, events are kept in memory
        }
    }
}
