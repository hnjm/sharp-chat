using SharpChat.Bans;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.DataProvider.Null {
    public class NullBanClient : IBanClient {
        public void GetBanList(Action<IEnumerable<IBanRecord>> onSuccess, Action<Exception> onFailure = null) {
            onSuccess.Invoke(Enumerable.Empty<IBanRecord>());
        }
    }
}
