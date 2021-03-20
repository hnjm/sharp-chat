using SharpChat.Channels;
using SharpChat.Events;
using System.Collections.Generic;

namespace SharpChat.Messages.Storage {
    public interface IMessageStorage : IEventHandler {
        IMessage GetMessage(long messageId);
        IEnumerable<IMessage> GetMessages(IChannel channel, int amount, int offset);
    }
}
