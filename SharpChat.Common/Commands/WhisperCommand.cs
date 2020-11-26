using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System.Linq;

namespace SharpChat.Commands {
    public class WhisperCommand : IChatCommand {
        public bool IsMatch(string name)
            => name == @"whisper" || name == @"msg";

        public IChatMessageEvent Dispatch(IChatCommandContext ctx) {
            if(ctx.Args.Count() < 3)
                throw new CommandException(LCR.COMMAND_FORMAT_ERROR);

            string whisperUserName = ctx.Args.ElementAtOrDefault(1);
            ChatUser whisperUser = ctx.Chat.Users.Get(whisperUserName);

            if(whisperUser == null)
                throw new CommandException(LCR.USER_NOT_FOUND, whisperUserName);

            if(whisperUser == ctx.User)
                return null;

            string whisperStr = string.Join(' ', ctx.Args.Skip(2));

            whisperUser.Send(new ChatMessageAddPacket(new ChatMessageEvent(ctx.User, whisperUser, whisperStr, ChatEventFlags.Private)));
            ctx.User.Send(new ChatMessageAddPacket(new ChatMessageEvent(ctx.User, ctx.User, $@"{whisperUser.DisplayName} {whisperStr}", ChatEventFlags.Private)));
            return null;
        }
    }
}
