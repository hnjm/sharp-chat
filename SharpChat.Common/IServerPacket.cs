using System;
using System.Threading;

namespace SharpChat {
    public interface IServerPacket {
        public const char SEPARATOR = '\t';

        [Obsolete(@"Should be represented by an Event's ID")]
        long SequenceId { get; }

        string Pack();
    }

    public abstract class ServerPacketBase : IServerPacket {
        private static long SequenceIdCounter = 0;

        [Obsolete(@"Should be represented by an Event's ID")]
        public long SequenceId { get; }

        public ServerPacketBase(long sequenceId = 0) {
            // Allow sequence id to be manually set for potential message repeats
            SequenceId = sequenceId > 0 ? sequenceId : Interlocked.Increment(ref SequenceIdCounter);
        }

        public abstract string Pack();
    }
}
