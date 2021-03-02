using System;
using System.Collections.Generic;
using System.Net;

namespace SharpChat.Bans {
    public interface IBanClient {
        void GetBanList(Action<IEnumerable<IBanRecord>> onSuccess, Action<Exception> onFailure = null);
        void CheckBan(long userId, IPAddress ipAddress, Action<IBanRecord> onSuccess, Action<Exception> onFailure = null);
        void CreateBan(long userId, long modId, bool perma, TimeSpan duration, string reason, Action<bool> onSuccess = null, Action<Exception> onFailure = null);
        void RemoveBan(string userName, Action<bool> onSuccess, Action<Exception> onFailure = null);
        void RemoveBan(IPAddress ipAddress, Action<bool> onSuccess, Action<Exception> onFailure = null);
    }
}
