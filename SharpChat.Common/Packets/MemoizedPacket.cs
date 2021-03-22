using System;

namespace SharpChat.Packets {
    /// <summary>
    /// Essentially makes it so <see cref="IServerPacket.Pack"/> is only called once rather than individually for each session.
    /// </summary>
    public class MemoizedPacket : IServerPacket {
        private string Packed { get; }

        public MemoizedPacket(IServerPacket packet) {
            Packed = (packet ?? throw new ArgumentNullException(nameof(packet))).Pack();
        }

        public string Pack()
            => Packed;
    }
}
