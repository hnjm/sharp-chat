using System;

namespace SharpChat.Events {
    [Flags]
    public enum EventFlags {
        None = 0,
        Action = 1,
        Broadcast = 1 << 1,
        Log = 1 << 2,
        Private = 1 << 3,
    }
}
