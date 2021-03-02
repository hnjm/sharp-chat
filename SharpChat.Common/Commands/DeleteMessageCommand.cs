using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class DeleteMessageCommand : IChatCommand {
        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"delmsg" || (name == @"delete" && args.ElementAtOrDefault(1)?.All(char.IsDigit) == true);

        public IMessageEvent DispatchCommand(IChatCommandContext ctx) {
            bool deleteAnyMessage = ctx.User.Can(UserPermissions.DeleteAnyMessage);

            if(!deleteAnyMessage && !ctx.User.Can(UserPermissions.DeleteOwnMessage))
                throw new CommandNotAllowedException(ctx.Args);

            if(!long.TryParse(ctx.Args.ElementAtOrDefault(1), out long sequenceId))
                throw new CommandFormatException();

            IEvent delEvent = ctx.Chat.Events.GetEvent(sequenceId);

            if(delEvent is not IMessageEvent || delEvent.Sender.Rank > ctx.User.Rank
                || (!deleteAnyMessage && delEvent.Sender.UserId != ctx.User.UserId))
                throw new MessageNotFoundCommandException();

            if(ctx.Chat.Events.RemoveEvent(delEvent))
                ctx.Chat.SendPacket(new ChatMessageDeletePacket(delEvent));

            return null;
        }
    }
}
