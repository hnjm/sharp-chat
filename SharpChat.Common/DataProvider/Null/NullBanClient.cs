using SharpChat.Bans;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.DataProvider.Null {
    public class NullBanClient : IBanClient {
        public IEnumerable<IBanRecord> GetBanList() {
            return Enumerable.Empty<IBanRecord>();
        }
    }
}
