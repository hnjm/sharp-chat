namespace SharpChat {
    public interface IServerPacketTarget {
        void SendPacket(IServerPacket packet);
    }
}
