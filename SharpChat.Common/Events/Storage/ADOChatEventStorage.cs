using SharpChat.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace SharpChat.Events.Storage {
    public partial class ADOChatEventStorage : IChatEventStorage {
        private DatabaseWrapper Wrapper { get; }

        public ADOChatEventStorage(DatabaseWrapper wrapper) {
            Wrapper = wrapper ?? throw new ArgumentNullException(nameof(wrapper));
            RunMigrations();
        }

        public void AddEvent(IEvent evt) {
            Wrapper.RunCommand(
                @"INSERT INTO `sqc_events` (`event_id`, `event_created`, `event_type`, `event_target`, `event_flags`, `event_data`"
                + @", `event_sender`, `event_sender_name`, `event_sender_colour`, `event_sender_rank`, `event_sender_nick`, `event_sender_perms`)"
                + @" VALUES (@id, " + Wrapper.FromUnixTime(@"@created") + @", @type, @target, @flags, @data"
                + @", @sender, @sender_name, @sender_colour, @sender_rank, @sender_nick, @sender_perms)",
                Wrapper.CreateParam(@"id", evt.SequenceId),
                Wrapper.CreateParam(@"created", evt.DateTime.ToUnixTimeSeconds()),
                Wrapper.CreateParam(@"type", evt.GetType().FullName),
                Wrapper.CreateParam(@"target", evt.Target.TargetName),
                Wrapper.CreateParam(@"flags", (byte)evt.Flags),
                Wrapper.CreateParam(@"data", JsonSerializer.SerializeToUtf8Bytes(evt, evt.GetType())),
                Wrapper.CreateParam(@"sender", evt.Sender?.UserId < 1 ? null : (long?)evt.Sender.UserId),
                Wrapper.CreateParam(@"sender_name", evt.Sender?.UserName),
                Wrapper.CreateParam(@"sender_colour", evt.Sender?.Colour.Raw),
                Wrapper.CreateParam(@"sender_rank", evt.Sender?.Rank),
                Wrapper.CreateParam(@"sender_nick", evt.Sender?.NickName),
                Wrapper.CreateParam(@"sender_perms", evt.Sender?.Permissions)
            );
        }

        public bool RemoveEvent(IEvent evt) {
            return Wrapper.RunCommand(
                @"UPDATE IGNORE `sqc_events` SET `event_deleted` = " + Wrapper.DateTimeNow() + @" WHERE `event_id` = @id AND `event_deleted` IS NULL",
                Wrapper.CreateParam(@"id", evt.SequenceId)
            ) > 0;
        }

        public IEvent GetEvent(long seqId) {
            IEvent evt = null;

            Wrapper.RunQuery(
                @"SELECT `event_id`, `event_type`, `event_flags`, `event_data`, `event_target`"
                + @", `event_sender`, `event_sender_name`, `event_sender_colour`, `event_sender_rank`, `event_sender_nick`, `event_sender_perms`"
                + @", " + Wrapper.ToUnixTime(@"`event_created`") + @" AS `event_created`"
                + @" FROM `sqc_events`"
                + @" WHERE `event_id` = @id",
                reader => {
                    if(reader.Next())
                        evt = ReadEvent(reader);
                },
                Wrapper.CreateParam(@"id", seqId)
            );

            return evt;
        }

        public IEnumerable<IEvent> GetEventsForTarget(IPacketTarget target, int amount = 20, int offset = 0) {
            List<IEvent> events = new List<IEvent>();

            Wrapper.RunQuery(
                @"SELECT `event_id`, `event_type`, `event_flags`, `event_data`, `event_target`"
                + @", `event_sender`, `event_sender_name`, `event_sender_colour`, `event_sender_rank`, `event_sender_nick`, `event_sender_perms`"
                + @", " + Wrapper.ToUnixTime(@"`event_created`") + @" AS `event_created`"
                + @" FROM `sqc_events`"
                + @" WHERE `event_deleted` IS NULL AND `event_target` = @target"
                + @" ORDER BY `event_id` DESC"
                + @" LIMIT @amount OFFSET @offset",
                reader => {
                    while(reader.Next()) {
                        IEvent evt = ReadEvent(reader, target);
                        if(evt != null)
                            events.Add(evt);
                    }
                },
                Wrapper.CreateParam(@"target", target.TargetName),
                Wrapper.CreateParam(@"amount", amount),
                Wrapper.CreateParam(@"offset", offset)
            );

            events.Reverse();
            return events;
        }

        private static readonly Type[] constPropTypes = new[] { typeof(IEvent), typeof(JsonElement), };

        private static IEvent ReadEvent(IDatabaseReader reader, IPacketTarget target = null) {
            Type evtType = Type.GetType(reader.ReadString(@"event_type"));
            ConstructorInfo evtConst = evtType.GetConstructors().FirstOrDefault(ci => ci.GetParameters().Select(pi => pi.ParameterType).SequenceEqual(constPropTypes));
            JsonDocument jsonDoc = JsonDocument.Parse(reader.ReadString(@"event_data"));
            return (IEvent)evtConst.Invoke(new object[] { new ADOEventReader(reader, target), jsonDoc.RootElement });
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }
    }
}
