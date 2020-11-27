using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class DeleteMessageCommand : IChatCommand {
        public bool IsMatch(string name, IEnumerable<string> args)
            => name == @"delmsg" || (name == @"delete" && args.ElementAtOrDefault(1)?.All(char.IsDigit) == true);

        public IChatMessageEvent Dispatch(IChatCommandContext ctx) {
            bool deleteAnyMessage = ctx.User.Can(ChatUserPermissions.DeleteAnyMessage);

            if(!deleteAnyMessage && !ctx.User.Can(ChatUserPermissions.DeleteOwnMessage))
                throw new CommandException(LCR.COMMAND_NOT_ALLOWED, $@"/{ctx.Args.ElementAt(0)}");

            if(!long.TryParse(ctx.Args.ElementAtOrDefault(1), out long sequenceId))
                throw new CommandException(LCR.COMMAND_FORMAT_ERROR);

            IChatEvent delEvent = ctx.Chat.Events.GetEvent(sequenceId);

            if(delEvent is not IChatMessageEvent || delEvent.Sender.Rank > ctx.User.Rank
                || (!deleteAnyMessage && delEvent.Sender.UserId != ctx.User.UserId))
                throw new CommandException(LCR.MESSAGE_DELETE_ERROR);

            if(ctx.Chat.Events.RemoveEvent(delEvent))
                ctx.Chat.Send(new ChatMessageDeletePacket(delEvent.SequenceId));

            return null;
        }
    }
}
