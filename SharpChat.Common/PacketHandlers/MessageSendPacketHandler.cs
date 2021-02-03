using SharpChat.Channels;
using SharpChat.Commands;
using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Sessions;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.PacketHandlers {
    public class MessageSendPacketHandler : IPacketHandler {
        public ClientPacket PacketId => ClientPacket.MessageSend;

        public ChatContext Context { get; }
        public IEnumerable<IChatCommand> Commands { get; }

        public MessageSendPacketHandler(ChatContext context, IEnumerable<IChatCommand> commands) {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public void HandlePacket(IPacketHandlerContext ctx) {
            if(ctx.Args.Count() < 3 || !ctx.HasUser || !ctx.User.Can(UserPermissions.SendMessage))
                return;

            if(!long.TryParse(ctx.Args.ElementAtOrDefault(1), out long userId) || ctx.User.UserId != userId)
                return;

            // No longer concats everything after index 1 with \t, no previous implementation did that either
            string text = ctx.Args.ElementAtOrDefault(2);
            if(string.IsNullOrWhiteSpace(text))
                return;

            Channel channel;
            string channelName = ctx.Args.ElementAtOrDefault(3)?.ToLowerInvariant();
            if(string.IsNullOrWhiteSpace(channelName))
                channel = ctx.Session.LastChannel;
            else
                channel = ctx.User.GetChannels().FirstOrDefault(c => c.Name.ToLowerInvariant() == channelName);

            if(channel == null
                || !ctx.User.InChannel(channel)
                || (ctx.User.IsSilenced && !ctx.User.Can(UserPermissions.SilenceUser)))
                return;

            ctx.Session.LastChannel = channel;

            if(ctx.User.Status != UserStatus.Online) {
                ctx.User.Status = UserStatus.Online;
                channel.Send(new UserUpdatePacket(ctx.User));
            }

            // there's a very miniscule chance that this will return a different value on second read
            int maxLength = Context.MessageTextMaxLength;
            if(text.Length > maxLength)
                text = text.Substring(0, maxLength);

            text = text.Trim();

#if DEBUG
            Logger.Write($@"<{ctx.Session.Id} {ctx.User.UserName}> {text}");
#endif

            IMessageEvent message = null;

            if(text[0] == '/') {
                try {
                    message = HandleCommand(text, ctx.Chat, ctx.User, channel, ctx.Session);
                } catch(CommandException ex) {
                    ctx.User.Send(ex.ToPacket(Context.Bot));
                }

                if(message == null)
                    return;
            }

            if(message == null)
                message = new ChatMessageEvent(ctx.User, channel, text);

            ctx.Chat.Events.AddEvent(message);
            channel.Send(new ChatMessageAddPacket(message));
        }

        public IMessageEvent HandleCommand(string message, ChatContext context, ChatUser user, Channel channel, Session session) {
            string[] parts = message[1..].Split(' ');
            string commandName = parts[0].Replace(@".", string.Empty).ToLowerInvariant();

            for(int i = 1; i < parts.Length; i++)
                parts[i] = parts[i].Replace(@"<", @"&lt;")
                                   .Replace(@">", @"&gt;")
                                   .Replace("\n", @" <br/> ");

            IChatCommand command = Commands.FirstOrDefault(x => x.IsCommandMatch(commandName, parts));
            if(command == null)
                throw new CommandNotFoundException(commandName);
            return command.DispatchCommand(new ChatCommandContext(parts, user, channel, context, session));
        }
    }
}
