using SharpChat.Channels;
using SharpChat.Users;
using System;
using System.Collections.Generic;

namespace SharpChat {
    public interface IChatCommandContext {
        IEnumerable<string> Args { get; }
        ChatUser User { get; }
        ChatChannel Channel { get; }
    }

    public class ChatCommandContext : IChatCommandContext {
        public IEnumerable<string> Args { get; }
        public ChatUser User { get; }
        public ChatChannel Channel { get; }

        public ChatCommandContext(IEnumerable<string> args, ChatUser user, ChatChannel channel) {
            Args = args ?? throw new ArgumentNullException(nameof(args));
            User = user ?? throw new ArgumentNullException(nameof(user));
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }
    }
}
