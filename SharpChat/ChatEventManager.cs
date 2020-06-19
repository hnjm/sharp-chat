using SharpChat.Events;
using SharpChat.Packet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat {
    public class ChatEventManager : IDisposable {
        private readonly List<IChatEvent> Events = null;

        public readonly ChatContext Context;

        public bool IsDisposed { get; private set; }

        public ChatEventManager(ChatContext context) {
            Context = context;

            if (Database.HasDatabase)
                Events = new List<IChatEvent>();
        }

        public void Add(IChatEvent evt) {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            if(Events != null)
                lock(Events)
                    Events.Add(evt);

            if(Database.HasDatabase)
                Database.LogEvent(evt);
        }

        public void Remove(IChatEvent evt) {
            if (evt == null)
                return;

            if (Events != null)
                lock (Events)
                    Events.Remove(evt);

            if (Database.HasDatabase)
                Database.DeleteEvent(evt);

            Context.Send(new ChatMessageDeletePacket(evt.SequenceId));
        }

        public IChatEvent Get(long seqId) {
            if (seqId < 1)
                return null;

            if (Database.HasDatabase)
                return Database.GetEvent(seqId);

            if (Events != null)
                lock (Events)
                    return Events.FirstOrDefault(e => e.SequenceId == seqId);

            return null;
        }

        public IEnumerable<IChatEvent> GetTargetLog(IPacketTarget target, int amount = 20, int offset = 0) {
            if (Database.HasDatabase)
                return Database.GetEvents(target, amount, offset).Reverse();

            if (Events != null)
                lock (Events) {
                    IEnumerable<IChatEvent> subset = Events.Where(e => e.Target == target || e.Target == null);

                    int start = subset.Count() - offset - amount;

                    if(start < 0) {
                        amount += start;
                        start = 0;
                    }

                    return subset.Skip(start).Take(amount).ToList();
                }

            return Enumerable.Empty<IChatEvent>();
        }

        ~ChatEventManager()
            => Dispose(false);

        public void Dispose()
            => Dispose(true);

        private void Dispose(bool disposing) {
            if (IsDisposed)
                return;
            IsDisposed = true;

            Events.Clear();

            if (disposing)
                GC.SuppressFinalize(this);
        }
    }
}
