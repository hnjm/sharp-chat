using SharpChat.Channels;
using SharpChat.Sessions;
using SharpChat.Users;
using System;
using System.Collections.Generic;

namespace SharpChat.Commands {
    public interface ICommandContext {
        IEnumerable<string> Args { get; }
        IUser User { get; }
        IChannel Channel { get; }
        ChatContext Chat { get; }
        Session Session { get; }
    }

    public class CommandContext : ICommandContext {
        public IEnumerable<string> Args { get; }
        public IUser User { get; }
        public IChannel Channel { get; }
        public ChatContext Chat { get; }
        public Session Session { get; }

        public CommandContext(IEnumerable<string> args, IUser user, IChannel channel, ChatContext context, Session session) {
            Args = args ?? throw new ArgumentNullException(nameof(args));
            User = user ?? throw new ArgumentNullException(nameof(user));
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
            Chat = context ?? throw new ArgumentNullException(nameof(context));
            Session = session ?? throw new ArgumentNullException(nameof(session));
        }
    }
}
