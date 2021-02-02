namespace SharpChat {
    public interface IServerPacketTarget {
        void Send(IServerPacket packet);
    }
}
