using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Events.Storage {
    public class MemoryChatEventStorage : IChatEventStorage {
        private object Lock { get; } = new object();
        private List<IChatEvent> Events { get; } = new List<IChatEvent>();

        public void AddEvent(IChatEvent evt) {
            lock(Lock)
                Events.Add(evt);
        }

        public bool RemoveEvent(IChatEvent evt) {
            lock(Lock)
                return Events.Remove(evt);
        }

        public IChatEvent GetEvent(long seqId) {
            lock(Lock)
                return Events.FirstOrDefault(e => e.SequenceId == seqId);
        }

        public IEnumerable<IChatEvent> GetEventsForTarget(IPacketTarget target, int amount = 20, int offset = 0) {
            lock(Lock) {
                IEnumerable<IChatEvent> subset = Events.Where(e => e.Target == target || e.Target == null);

                int start = subset.Count() - offset - amount;

                if(start < 0) {
                    amount += start;
                    start = 0;
                }

                return subset.Skip(start).Take(amount).ToList();
            }
        }

        private bool IsDisposed;

        ~MemoryChatEventStorage()
            => DoDispose();

        public void Dispose() {
            DoDispose();
            GC.SuppressFinalize(this);
        }

        private void DoDispose() {
            if(IsDisposed)
                return;
            IsDisposed = true;
            Events.Clear();
        }
    }
}
