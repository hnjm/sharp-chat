using SharpChat.Channels;
using SharpChat.Events;
using System;
using System.Collections.Generic;

namespace SharpChat.Messages.Storage {
    public interface IMessageStorage : IEventHandler {
        IMessage GetMessage(long messageId);
        void GetMessages(IChannel channel, Action<IEnumerable<IMessage>> callback, int amount, int offset);
    }
}
