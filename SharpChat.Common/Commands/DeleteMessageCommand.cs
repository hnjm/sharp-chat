using SharpChat.Events;
using SharpChat.Events.Storage;
using SharpChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class DeleteMessageCommand : ICommand {
        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"delmsg" || (name == @"delete" && args.ElementAtOrDefault(1)?.All(char.IsDigit) == true);

        private IEventStorage Storage { get; }

        public DeleteMessageCommand(IEventStorage storage) {
            Storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public MessageCreateEvent DispatchCommand(ICommandContext ctx) {
            bool deleteAnyMessage = ctx.User.Can(UserPermissions.DeleteAnyMessage);

            if(!deleteAnyMessage && !ctx.User.Can(UserPermissions.DeleteOwnMessage))
                throw new CommandNotAllowedException(ctx.Args);

            if(!long.TryParse(ctx.Args.ElementAtOrDefault(1), out long sequenceId))
                throw new CommandFormatException();

            IEvent delEvent = Storage.GetEvent(sequenceId);

            if(delEvent is not MessageCreateEvent || delEvent.Sender.Rank > ctx.User.Rank
                || (!deleteAnyMessage && delEvent.Sender.UserId != ctx.User.UserId))
                throw new MessageNotFoundCommandException();

            if(Storage.RemoveEvent(delEvent))
                ctx.Chat.SendPacket(new ChatMessageDeletePacket(delEvent));

            return null;
        }
    }
}
