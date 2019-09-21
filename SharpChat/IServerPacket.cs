﻿using System.Collections.Generic;
using System.Threading;

namespace SharpChat
{
    public interface IServerPacket
    {
        int SequenceId { get; }
        IEnumerable<string> Pack(int version);
    }

    public abstract class ServerPacket : IServerPacket
    {
        private static int SequenceIdCounter = 0;

        public int SequenceId { get; }

        public ServerPacket()
        {
            SequenceId = Interlocked.Increment(ref SequenceIdCounter);
        }

        [System.Obsolete(@"Provided for shit that hasn't been moved into its own packet class yet.")]
        public static int NextSequenceId()
        {
            return Interlocked.Increment(ref SequenceIdCounter);
        }

        public abstract IEnumerable<string> Pack(int version);
    }
}