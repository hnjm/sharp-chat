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
}
