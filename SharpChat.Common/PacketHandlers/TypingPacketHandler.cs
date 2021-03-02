using SharpChat.Channels;
using SharpChat.Packets;
using System.Linq;

namespace SharpChat.PacketHandlers {
    public class TypingPacketHandler : IPacketHandler {
        public ClientPacket PacketId => ClientPacket.Typing;

        public void HandlePacket(IPacketHandlerContext ctx) {
            if(!ctx.HasUser)
                return;

            if(!long.TryParse(ctx.Args.ElementAtOrDefault(1), out long userId) || ctx.User.UserId != userId)
                return;

            string channelName = ctx.Args.ElementAtOrDefault(2)?.ToLowerInvariant();
            if(!string.IsNullOrWhiteSpace(channelName))
                return;

            Channel channel = ctx.User.GetChannels().FirstOrDefault(c => c.Name.ToLowerInvariant() == channelName);
            if(channel == null || !channel.CanType(ctx.User))
                return;

            ctx.Session.LastChannel = channel;

            ChannelTyping info = channel.RegisterTyping(ctx.User);
            if(info == null)
                return;

            channel.SendPacket(new TypingPacket(channel, info));
        }
    }
}
