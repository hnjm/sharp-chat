using SharpChat.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.PacketHandlers {
    public class CapabilitiesPacketHandler : IPacketHandler {
        public ClientPacketId PacketId => ClientPacketId.Capabilities;

        private SessionManager Sessions { get; }

        public CapabilitiesPacketHandler(SessionManager sessions) {
            Sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
        }

        public void HandlePacket(IPacketHandlerContext ctx) {
            if(!ctx.HasSession)
                return;

            ClientCapability caps = 0;

            IEnumerable<string> capStrs = ctx.Args.ElementAtOrDefault(1)?.Split(' ');
            if(capStrs != null && capStrs.Any())
                foreach(string capStr in capStrs)
                    if(Enum.TryParse(typeof(ClientCapability), capStr.ToUpperInvariant(), out object cap))
                        caps |= (ClientCapability)cap;

            Sessions.SetCapabilities(ctx.Session, caps);
        }
    }
}
