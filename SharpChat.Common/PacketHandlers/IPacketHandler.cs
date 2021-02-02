namespace SharpChat.PacketHandlers {
    public interface IPacketHandler {
        ClientPacket PacketId { get; }
        void HandlePacket(IPacketHandlerContext ctx);
    }
}
