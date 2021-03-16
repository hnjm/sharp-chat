using System.Collections.Generic;

namespace SharpChat.Events {
    public interface IUpdateEvent {
        long TargetId { get; }
        Dictionary<string, object> GetUpdatedFields();
    }
}
