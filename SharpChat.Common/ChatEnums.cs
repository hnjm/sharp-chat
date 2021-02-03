using System;

namespace SharpChat {
    public enum ClientPacket {
        // Version 1
        Ping = 0,
        Authenticate = 1,
        MessageSend = 2,

        // Version 2
        Typing = 3,
        Capabilities = 4,
    }

    public enum ServerPacket {
        // Version 1
        Pong = 0,
        UserConnect = 1,
        MessageAdd = 2,
        UserDisconnect = 3,
        ChannelEvent = 4,
        UserSwitch = 5,
        MessageDelete = 6,
        ContextPopulate = 7,
        ContextClear = 8,
        BAKA = 9,
        UserUpdate = 10,

        // Version 2
        Typing = 11,
    }

    public enum ServerChannelPacket {
        Create = 0,
        Update = 1,
        Delete = 2,
    }

    public enum ServerMovePacket {
        UserJoined = 0,
        UserLeft = 1,
        ForcedMove = 2,
    }

    public enum ServerContextPacket {
        Users = 0,
        Message = 1,
        Channels = 2,
    }

    [Flags]
    public enum ClientCapabilities : int {
        /// <summary>
        /// Supports the typing event.
        /// </summary>
        TYPING = 0x01,

        /// <summary>
        /// Supports being in multiple channels at once.
        /// </summary>
        MCHAN = 0x02,
    }
}
