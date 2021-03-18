using SharpChat.Events;
using SharpChat.Users;
using System;
using System.Collections.Generic;

namespace SharpChat.Channels {
    public interface IChannel : IEventTarget, IEventHandler {
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

        bool HasUser(IUser user);
        void GetUsers(Action<IEnumerable<IUser>> callable);

        string Pack();
    }
}
