using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class WhisperCommand : IChatCommand {
        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"whisper" || name == @"msg";

        public IMessageEvent DispatchCommand(IChatCommandContext ctx) {
            if(ctx.Args.Count() < 3)
                throw new CommandException(LCR.COMMAND_FORMAT_ERROR);

            string whisperUserName = ctx.Args.ElementAtOrDefault(1);
            ChatUser whisperUser = ctx.Chat.Users.Get(whisperUserName);

            if(whisperUser == null)
                throw new CommandException(LCR.USER_NOT_FOUND, whisperUserName);

            if(whisperUser == ctx.User)
                return null;

            string whisperStr = string.Join(' ', ctx.Args.Skip(2));

            whisperUser.Send(new ChatMessageAddPacket(new ChatMessageEvent(ctx.User, whisperUser, whisperStr, EventFlags.Private)));
            ctx.User.Send(new ChatMessageAddPacket(new ChatMessageEvent(ctx.User, ctx.User, $@"{whisperUser.DisplayName} {whisperStr}", EventFlags.Private)));
            return null;
        }
    }
}
