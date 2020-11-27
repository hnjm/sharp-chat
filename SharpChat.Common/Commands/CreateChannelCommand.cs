using SharpChat.Channels;
using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Commands {
    public class CreateChannelCommand : IChatCommand {
        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"create";

        public IChatMessageEvent DispatchCommand(IChatCommandContext ctx) {
            if(!ctx.User.Can(ChatUserPermissions.CreateChannel))
                throw new CommandException(LCR.COMMAND_NOT_ALLOWED, @"/create");

            bool hasRank;
            if(ctx.Args.Count() < 2 || (hasRank = ctx.Args.ElementAtOrDefault(1)?.All(char.IsDigit) == true && ctx.Args.Count() < 3))
                throw new CommandException(LCR.COMMAND_FORMAT_ERROR);

            int rank = 0;
            if(hasRank && !int.TryParse(ctx.Args.ElementAtOrDefault(1), out rank) && rank < 0)
                rank = 0;

            if(rank > ctx.User.Rank)
                throw new CommandException(LCR.INSUFFICIENT_RANK);

            string createChanName = string.Join('_', ctx.Args.Skip(hasRank ? 2 : 1));
            ChatChannel createChan = new ChatChannel {
                Name = createChanName,
                IsTemporary = !ctx.User.Can(ChatUserPermissions.SetChannelPermanent),
                Rank = rank,
                Owner = ctx.User,
            };

            try {
                ctx.Chat.Channels.Add(createChan);
            } catch(ChannelExistException) {
                throw new CommandException(LCR.CHANNEL_ALREADY_EXISTS, createChan.Name);
            } catch(ChannelInvalidNameException) {
                throw new CommandException(LCR.CHANNEL_NAME_INVALID);
            }

            ctx.Chat.SwitchChannel(ctx.User, createChan, createChan.Password);
            ctx.User.Send(new LegacyCommandResponse(LCR.CHANNEL_CREATED, false, createChan.Name));
            return null;
        }
    }
}
