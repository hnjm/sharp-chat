using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System.Linq;

namespace SharpChat.Commands {
    public class AFKCommand : IChatCommand {
        private const string DEFAULT = @"AFK";
        private const int MAX_LENGTH = 5;

        public bool IsMatch(string name) {
            return name == @"afk";
        }

        public IChatMessage Dispatch(IChatCommandContext context) {
            string statusText = context.Args.ElementAtOrDefault(1);
            if(string.IsNullOrWhiteSpace(statusText))
                statusText = DEFAULT;
            else {
                statusText = statusText.Trim();
                if(statusText.Length > MAX_LENGTH)
                    statusText = statusText.Substring(0, MAX_LENGTH).Trim();
            }

            context.User.Status = ChatUserStatus.Away;
            context.User.StatusMessage = statusText;
            context.Channel.Send(new UserUpdatePacket(context.User));

            return null;
        }
    }
}
