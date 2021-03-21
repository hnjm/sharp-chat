using SharpChat.Channels;
using SharpChat.Commands;
using SharpChat.Events;
using SharpChat.Messages;
using SharpChat.Packets;
using SharpChat.Sessions;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.PacketHandlers {
    public class MessageSendPacketHandler : IPacketHandler {
        public ClientPacket PacketId => ClientPacket.MessageSend;

        private IEventDispatcher Dispatcher { get; }
        private MessageManager Messages { get; }
        private UserManager Users { get; }
        private ChannelManager Channels { get; }
        private ChatBot Bot { get; }
        private IEnumerable<ICommand> Commands { get; }

        public MessageSendPacketHandler(
            IEventDispatcher dispatcher,
            UserManager users,
            ChannelManager channels,
            MessageManager messages,
            ChatBot bot,
            IEnumerable<ICommand> commands
        ) {
            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            Users = users ?? throw new ArgumentNullException(nameof(users));
            Channels = channels ?? throw new ArgumentNullException(nameof(channels));
            Messages = messages ?? throw new ArgumentNullException(nameof(messages));
            Bot = bot ?? throw new ArgumentNullException(nameof(bot));
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

            IChannel channel;
            string channelName = ctx.Args.ElementAtOrDefault(3)?.ToLowerInvariant();
            if(string.IsNullOrWhiteSpace(channelName))
                channel = ctx.Session.LastChannel;
            else
                channel = ctx.Chat.Channels.GetChannel(channelName);

            if(channel == null
                || !ctx.Chat.ChannelUsers.HasUser(channel, ctx.User)
            //  || (ctx.User.IsSilenced && !ctx.User.Can(UserPermissions.SilenceUser)) TODO: readd silencing
            ) return;

            ctx.Session.LastChannel = channel;

            if(ctx.User.Status != UserStatus.Online) {
                ctx.Chat.Users.Update(ctx.User, status: UserStatus.Online);
                // ChannelUsers?
                //channel.SendPacket(new UserUpdatePacket(ctx.User));
            }

            // there's a very miniscule chance that this will return a different value on second read
            int maxLength = Messages.TextMaxLength;
            if(text.Length > maxLength)
                text = text.Substring(0, maxLength);

            text = text.Trim();

#if DEBUG
            Logger.Write($@"<{ctx.Session.Id} {ctx.User.UserName}> {text}");
#endif

            bool handled = false;

            if(text[0] == '/')
                try {
                    handled = HandleCommand(text, ctx.Chat, ctx.User, channel, ctx.Session);
                } catch(CommandException ex) {
                    ctx.Session.SendPacket(ex.ToPacket(Bot));
                }

            if(!handled)
                Messages.Create(ctx.User, channel, text);
        }

        public bool HandleCommand(string message, ChatContext context, IUser user, IChannel channel, Session session) {
            string[] parts = message[1..].Split(' ');
            string commandName = parts[0].Replace(@".", string.Empty).ToLowerInvariant();

            for(int i = 1; i < parts.Length; i++)
                parts[i] = parts[i].Replace(@"<", @"&lt;")
                                   .Replace(@">", @"&gt;")
                                   .Replace("\n", @" <br/> ");

            ICommand command = Commands.FirstOrDefault(x => x.IsCommandMatch(commandName, parts));
            if(command == null)
                throw new CommandNotFoundException(commandName);
            return command.DispatchCommand(new CommandContext(parts, user, channel, context, session));
        }
    }
}
