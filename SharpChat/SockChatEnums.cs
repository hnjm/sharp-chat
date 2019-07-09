namespace SharpChat
{
    public enum SockChatServerMessage
    {
        Ping = 0,
        Authenticate = 1,
        MessageSend = 2,
    }

    public enum SockChatClientMessage
    {
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
    }

    enum SockChatChannelEventType
    {
        Create = 0,
        Update = 1,
        Delete = 2,
    }

    // Force is only ever sent directly to a user, the rest is broadcast to the channel
    enum SockChatUserSwitchType
    {
        Join = 0,
        Leave = 1,
        Force = 2,
    }

    enum SockChatContextPopulateType
    {
        Users = 0,
        Message = 1,
        Channels = 2,
    }

    enum SockChatContextClearType
    {
        Messages = 0,
        Users = 1,
        Channels = 2,
        MessagesUsers = 3,
        All = 4,
    }
}
