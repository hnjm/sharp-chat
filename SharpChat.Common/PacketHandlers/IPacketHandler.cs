namespace SharpChat.PacketHandlers {
    public interface IPacketHandler {
        ClientPacketId PacketId { get; }
        void HandlePacket(IPacketHandlerContext ctx);
    }
}
