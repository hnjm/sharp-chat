using System;
using System.Net;

namespace SharpChat.Bans {
    public interface IBanRecord {
        long UserId { get; }
        IPAddress UserIP { get; }
        DateTimeOffset Expires { get; }
        bool IsPermanent { get; }
        string Username { get; }
    }
}
