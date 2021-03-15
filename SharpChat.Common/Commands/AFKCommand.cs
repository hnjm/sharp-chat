using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class AFKCommand : ICommand {
        private const string DEFAULT = @"AFK";
        private const int MAX_LENGTH = 5;

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"afk";

        public IMessageEvent DispatchCommand(ICommandContext ctx) {
            string statusText = ctx.Args.ElementAtOrDefault(1);
            if(string.IsNullOrWhiteSpace(statusText))
                statusText = DEFAULT;
            else {
                statusText = statusText.Trim();
                if(statusText.Length > MAX_LENGTH)
                    statusText = statusText.Substring(0, MAX_LENGTH).Trim();
            }

            ctx.User.Status = UserStatus.Away;
            ctx.User.StatusMessage = statusText;
            ctx.Channel.SendPacket(new UserUpdatePacket(ctx.User));
            return null;
        }
    }
}
