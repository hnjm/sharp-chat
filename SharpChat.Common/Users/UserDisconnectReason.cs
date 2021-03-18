namespace SharpChat.Users {
    public enum UserDisconnectReason : int {
        Unknown = 0,
        Leave = 1,
        TimeOut = 2,
        Kicked = 3,
        Flood = 4,
    }
}
