using SharpChat.Bans;
using System;
using System.Net;

namespace SharpChat.DataProvider.Null {
    public class NullBanRecord : IBanRecord {
        public long UserId => -1;
        public IPAddress UserIP => IPAddress.None;
        public DateTimeOffset Expires => DateTimeOffset.MinValue;
        public bool IsPermanent => false;
        public string Username => null;
    }
}
