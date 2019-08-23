namespace SharpChat
{
    public enum SockChatClientPacket
    {
        // Version 1
        Ping = 0,
        Authenticate = 1,
        MessageSend = 2,

        // Version 2
        Upgrade = 3,
    }

    public enum SockChatServerPacket
    {
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
        UpgradeAck = 11,
    }
}
