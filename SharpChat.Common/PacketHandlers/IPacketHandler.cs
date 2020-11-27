namespace SharpChat.PacketHandlers {
    public interface IPacketHandler {
        SockChatClientPacket PacketId { get; }
        void HandlePacket(IPacketHandlerContext ctx);
    }
}
