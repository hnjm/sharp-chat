using SharpChat.Channels;
using SharpChat.Commands;
using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.PacketHandlers {
    public class MessageSendPacketHandler : IPacketHandler {
        public SockChatClientPacket PacketId => SockChatClientPacket.MessageSend;

        public ChatContext Context { get; }
        public IEnumerable<IChatCommand> Commands { get; }

        public MessageSendPacketHandler(ChatContext context, IEnumerable<IChatCommand> commands) {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public void HandlePacket(IPacketHandlerContext ctx) {
            if(ctx.Args.Count() < 3 || !ctx.HasUser || !ctx.User.Can(ChatUserPermissions.SendMessage))
                return;

            if(!long.TryParse(ctx.Args.ElementAtOrDefault(1), out long userId) || ctx.User.UserId != userId)
                return;

            // No longer concats everything after index 1 with \t, no previous implementation did that either
            string text = ctx.Args.ElementAtOrDefault(2);
            if(string.IsNullOrWhiteSpace(text))
                return;

            ChatChannel channel = ctx.User.CurrentChannel;
            if(channel == null
                || !ctx.User.InChannel(channel)
                || (ctx.User.IsSilenced && !ctx.User.Can(ChatUserPermissions.SilenceUser)))
                return;

            if(ctx.User.Status != ChatUserStatus.Online) {
                ctx.User.Status = ChatUserStatus.Online;
                channel.Send(new UserUpdatePacket(ctx.User));
            }

            // there's a very miniscule chance that this will return a different value on second read
            int maxLength = Context.MessageTextMaxLength;
            if(text.Length > maxLength)
                text = text.Substring(0, maxLength);

            text = text.Trim();

#if DEBUG
            Logger.Write($@"<{ctx.Session.Id} {ctx.User.Username}> {text}");
#endif

            IChatMessageEvent message = null;

            if(text[0] == '/') {
                message = HandleCommand(text, ctx.Chat, ctx.User, channel);
                if(message == null)
                    return;
            }

            if(message == null)
                message = new ChatMessageEvent(ctx.User, channel, text);

            ctx.Chat.Events.AddEvent(message);
            channel.Send(new ChatMessageAddPacket(message));
        }

        public IChatMessageEvent HandleCommand(string message, ChatContext context, ChatUser user, ChatChannel channel) {
            string[] parts = message[1..].Split(' ');
            string commandName = parts[0].Replace(@".", string.Empty).ToLowerInvariant();

            for(int i = 1; i < parts.Length; i++)
                parts[i] = parts[i].Replace(@"<", @"&lt;")
                                   .Replace(@">", @"&gt;")
                                   .Replace("\n", @" <br/> ");

            IChatCommand command = Commands.FirstOrDefault(x => x.IsCommandMatch(commandName, parts));
            if(command == null) {
                user.Send(new LegacyCommandResponse(LCR.COMMAND_NOT_FOUND, true, commandName));
            } else {
                try {
                    return command.DispatchCommand(new ChatCommandContext(parts, user, channel, context));
                } catch(CommandException ex) {
                    user.Send(ex.ToPacket());
                }
            }

            return null;
        }
    }
}
