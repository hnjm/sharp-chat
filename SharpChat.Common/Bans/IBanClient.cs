using System.Collections.Generic;

namespace SharpChat.Bans {
    public interface IBanClient {
        IEnumerable<IBanRecord> GetBanList();
    }
}
