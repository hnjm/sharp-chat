namespace SharpChat {
    public interface IPacketTarget {
        void Send(IServerPacket packet);
    }
}
