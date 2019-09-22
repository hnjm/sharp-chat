namespace SharpChat
{
    public interface IPacketTarget
    {
        string TargetName { get; }
        void Send(IServerPacket packet);
    }
}
