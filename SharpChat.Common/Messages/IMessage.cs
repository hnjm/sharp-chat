using SharpChat.Channels;
using SharpChat.Users;
using System;

namespace SharpChat.Messages {
    public interface IMessage {
        long MessageId { get; }
        IChannel Channel { get; }
        IUser Sender { get; }
        string Text { get; }
        bool IsAction { get; }
        DateTimeOffset Created { get; }
        DateTimeOffset? Edited { get; }
        bool IsEdited { get; }
    }
}
