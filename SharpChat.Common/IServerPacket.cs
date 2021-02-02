using System.Collections.Generic;
using System.Threading;

namespace SharpChat {
    public interface IServerPacket {
        long SequenceId { get; }
        IEnumerable<string> Pack();
    }

    public abstract class ServerPacketBase : IServerPacket {
        private static long SequenceIdCounter = 0;

        public long SequenceId { get; }

        public ServerPacketBase(long sequenceId = 0) {
            // Allow sequence id to be manually set for potential message repeats
            SequenceId = sequenceId > 0 ? sequenceId : Interlocked.Increment(ref SequenceIdCounter);
        }

        public abstract IEnumerable<string> Pack();
    }
}
