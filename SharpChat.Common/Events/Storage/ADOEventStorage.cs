using SharpChat.Database;
using System;

namespace SharpChat.Events.Storage {
    public partial class ADOEventStorage : IEventHandler {
        private DatabaseWrapper Wrapper { get; }

        public ADOEventStorage(DatabaseWrapper wrapper) {
            Wrapper = wrapper ?? throw new ArgumentNullException(nameof(wrapper));
            RunMigrations();
        }

        public void HandleEvent(object sender, IEvent evt) {
            if(sender == this)
                return;

            Wrapper.RunCommand(
                @"INSERT INTO `sqc_events` (`event_id`, `event_created`, `event_type`, `event_target`, `event_data`"
                + @", `event_sender`, `event_sender_name`, `event_sender_colour`, `event_sender_rank`, `event_sender_nick`, `event_sender_perms`)"
                + @" VALUES (@id, " + Wrapper.FromUnixTime(@"@created") + @", @type, @target, @data"
                + @", @sender, @sender_name, @sender_colour, @sender_rank, @sender_nick, @sender_perms)",
                Wrapper.CreateParam(@"id", evt.EventId),
                Wrapper.CreateParam(@"created", evt.DateTime.ToUnixTimeSeconds()),
                Wrapper.CreateParam(@"type", evt.Type),
                Wrapper.CreateParam(@"target", evt.Target),
                Wrapper.CreateParam(@"data", evt.EncodeAsJson()),
                Wrapper.CreateParam(@"sender", evt.Sender?.UserId < 1 ? null : (long?)evt.Sender.UserId),
                Wrapper.CreateParam(@"sender_name", evt.Sender?.UserName),
                Wrapper.CreateParam(@"sender_colour", evt.Sender?.Colour.Raw),
                Wrapper.CreateParam(@"sender_rank", evt.Sender?.Rank),
                Wrapper.CreateParam(@"sender_nick", evt.Sender?.NickName),
                Wrapper.CreateParam(@"sender_perms", evt.Sender?.Permissions)
            );
        }
    }
}
