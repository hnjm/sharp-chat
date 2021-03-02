using SharpChat.Packets;
using System.Linq;

namespace SharpChat.PacketHandlers {
    public class PingPacketHandler : IPacketHandler {
        public ClientPacket PacketId => ClientPacket.Ping;

        public void HandlePacket(IPacketHandlerContext ctx) {
            if(!long.TryParse(ctx.Args.ElementAtOrDefault(1), out long userId)
                && ctx.Session.User.UserId != userId)
                return;
            //if(!int.TryParse(ctx.Args.ElementAtOrDefault(2), out int timestamp))
            //    timestamp = -1;

            ctx.Session.BumpPing();
            ctx.Session.SendPacket(new PongPacket(ctx.Session.LastPing));
        }
    }
}
