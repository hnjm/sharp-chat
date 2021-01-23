using SharpChat.Channels;
using SharpChat.Packets;

namespace SharpChat.PacketHandlers {
    public class TypingPacketHandler : IPacketHandler {
        public SockChatClientPacket PacketId => SockChatClientPacket.Typing;

        public void HandlePacket(IPacketHandlerContext ctx) {
            if(!ctx.HasUser)
                return;

            ChatChannel tChannel = ctx.User.CurrentChannel;
            if(tChannel == null || !tChannel.CanType(ctx.User))
                return;

            ChatChannelTyping tInfo = tChannel.RegisterTyping(ctx.User);
            if(tInfo == null)
                return;

            tChannel.Send(new TypingPacket(tChannel, tInfo));
        }
    }
}
