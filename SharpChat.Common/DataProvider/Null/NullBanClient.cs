using SharpChat.Bans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SharpChat.DataProvider.Null {
    public class NullBanClient : IBanClient {
        public void CheckBan(long userId, IPAddress ipAddress, Action<IBanRecord> onSuccess, Action<Exception> onFailure = null) {
            onSuccess.Invoke(new NullBanRecord());
        }

        public void CreateBan(long userId, long modId, bool perma, TimeSpan duration, string reason, Action<bool> onSuccess = null, Action<Exception> onFailure = null) {
            onSuccess?.Invoke(true);
        }

        public void GetBanList(Action<IEnumerable<IBanRecord>> onSuccess, Action<Exception> onFailure = null) {
            onSuccess.Invoke(Enumerable.Empty<IBanRecord>());
        }

        public void RemoveBan(string userName, Action<bool> onSuccess, Action<Exception> onFailure = null) {
            onSuccess.Invoke(false);
        }

        public void RemoveBan(IPAddress ipAddress, Action<bool> onSuccess, Action<Exception> onFailure = null) {
            onSuccess.Invoke(false);
        }
    }
}
