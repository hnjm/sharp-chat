using System.Collections.Generic;
using System.Threading;

namespace SharpChat {
    public interface IServerPacket {
        int SequenceId { get; }
        IEnumerable<string> Pack(int version);
    }

    public abstract class ServerPacket : IServerPacket {
        private static int SequenceIdCounter = 0;

        public int SequenceId { get; }

        public ServerPacket(int sequenceId = 0) {
            // Allow sequence id to be manually set for potential message repeats
            SequenceId = sequenceId > 0 ? sequenceId : Interlocked.Increment(ref SequenceIdCounter);
        }

        public abstract IEnumerable<string> Pack(int version);
    }
}
