using System;
using System.Collections.Generic;

namespace SharpChat.Bans {
    public interface IBanClient {
        void GetBanList(Action<IEnumerable<IBanRecord>> onSuccess, Action<Exception> onFailure = null);
    }
}
