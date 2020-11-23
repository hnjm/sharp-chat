using System;

namespace SharpChat.Users {
    [Flags]
    public enum ChatUserPermissions : int {
        KickUser            = 0x00000001,
        BanUser             = 0x00000002,
        SilenceUser         = 0x00000004,
        Broadcast           = 0x00000008,
        SetOwnNickname      = 0x00000010,
        SetOthersNickname   = 0x00000020,
        CreateChannel       = 0x00000040,
        DeleteChannel       = 0x00010000,
        SetChannelPermanent = 0x00000080,
        SetChannelPassword  = 0x00000100,
        SetChannelHierarchy = 0x00000200,
        JoinAnyChannel      = 0x00020000,
        SendMessage         = 0x00000400,
        DeleteOwnMessage    = 0x00000800,
        DeleteAnyMessage    = 0x00001000,
        EditOwnMessage      = 0x00002000,
        EditAnyMessage      = 0x00004000,
        SeeIPAddress        = 0x00008000,
    }
}
