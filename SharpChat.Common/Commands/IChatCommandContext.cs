using SharpChat.Channels;
using SharpChat.Sessions;
using SharpChat.Users;
using System;
using System.Collections.Generic;

namespace SharpChat.Commands {
    public interface IChatCommandContext {
        IEnumerable<string> Args { get; }
        ChatUser User { get; }
        Channel Channel { get; }
        ChatContext Chat { get; }
        Session Session { get; }
    }

    public class ChatCommandContext : IChatCommandContext {
        public IEnumerable<string> Args { get; }
        public ChatUser User { get; }
        public Channel Channel { get; }
        public ChatContext Chat { get; }
        public Session Session { get; }

        public ChatCommandContext(IEnumerable<string> args, ChatUser user, Channel channel, ChatContext context, Session session) {
            Args = args ?? throw new ArgumentNullException(nameof(args));
            User = user ?? throw new ArgumentNullException(nameof(user));
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
            Chat = context ?? throw new ArgumentNullException(nameof(context));
            Session = session ?? throw new ArgumentNullException(nameof(session));
        }
    }
}
