using SharpChat.Events;
using SharpChat.Users;
using System;

namespace SharpChat.Channels {
    public interface IChannel : IEventTarget, IEquatable<IChannel> {
        string Name { get; }
        bool IsTemporary { get; }
        int MinimumRank { get; }
        bool AutoJoin { get; }
        uint MaxCapacity { get; }
        IUser Owner { get; }

        string Password { get; }
        bool HasPassword { get; }

        bool HasMaxCapacity { get; }

        bool VerifyPassword(string password);
    }
}
