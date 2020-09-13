using System;
using System.Collections.Generic;

namespace SharpChat {
    public interface IChatCommandContext {
        IEnumerable<string> Args { get; }
        ChatChannel Channel { get; }
        ChatUser User { get; }
        ChatUserSession Session { get; }
    }

    public class ChatCommandContext : IChatCommandContext {
        public IEnumerable<string> Args { get; }
        public ChatChannel Channel { get; }
        public ChatUser User { get; }
        public ChatUserSession Session { get; }

        public ChatCommandContext(IEnumerable<string> args, ChatChannel channel, ChatUser user, ChatUserSession session) {
            Args = args ?? throw new ArgumentNullException(nameof(args));
            User = user ?? throw new ArgumentNullException(nameof(user));
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
            Session = session ?? throw new ArgumentNullException(nameof(session));
        }
    }
}
