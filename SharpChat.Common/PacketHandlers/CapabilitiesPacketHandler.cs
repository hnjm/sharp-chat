using SharpChat.Packets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.PacketHandlers {
    public class CapabilitiesPacketHandler : IPacketHandler {
        public ClientPacket PacketId => ClientPacket.Capabilities;

        public void HandlePacket(IPacketHandlerContext ctx) {
            if(!ctx.HasSession)
                return;

            ClientCapabilities caps = 0;

            IEnumerable<string> capStrs = ctx.Args.ElementAtOrDefault(1)?.Split(' ');
            if(capStrs != null && capStrs.Any())
                foreach(string capStr in capStrs) {
                    Logger.Debug(capStr);
                    if(Enum.TryParse(typeof(ClientCapabilities), capStr.ToUpperInvariant(), out object cap))
                        caps |= (ClientCapabilities)cap;
                }

            Logger.Debug(caps);

            ctx.Session.Capabilities = caps;
            ctx.Session.SendPacket(new CapabilityConfirmationPacket(caps));
        }
    }
}
