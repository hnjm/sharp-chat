using SharpChat.Database;
using SharpChat.Events;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SharpChat {
    public static partial class DB {
        private static DatabaseWrapper Wrapper { get; set; }

        public static bool HasDatabase
            => Wrapper != null && !Wrapper.IsNullBackend;

        public static void Init(DatabaseWrapper wrapper) {
            Wrapper = wrapper;
            RunMigrations();
        }

        public static void Deinit() {
            Wrapper = null;
        }

        private const long ID_EPOCH = 1588377600000;
        private static int IdCounter = 0;

        public static long GenerateId() {
            if (IdCounter > 200)
                IdCounter = 0;

            long id = 0;
            id |= (DateTimeOffset.Now.ToUnixTimeMilliseconds() - ID_EPOCH) << 8;
            id |= (ushort)(++IdCounter);
            return id;
        }

        public static void LogEvent(IChatEvent evt) {
            if(evt.SequenceId < 1)
                evt.SequenceId = GenerateId();

            Wrapper.RunCommand(
                @"INSERT INTO `sqc_events` (`event_id`, `event_created`, `event_type`, `event_target`, `event_flags`, `event_data`"
                + @", `event_sender`, `event_sender_name`, `event_sender_colour`, `event_sender_rank`, `event_sender_nick`, `event_sender_perms`)"
                + @" VALUES (@id, FROM_UNIXTIME(@created), @type, @target, @flags, @data"
                + @", @sender, @sender_name, @sender_colour, @sender_rank, @sender_nick, @sender_perms)",
                Wrapper.CreateParam(@"id", evt.SequenceId),
                Wrapper.CreateParam(@"created", evt.DateTime.ToUnixTimeSeconds()),
                Wrapper.CreateParam(@"type", evt.GetType().FullName),
                Wrapper.CreateParam(@"target", evt.Target.TargetName),
                Wrapper.CreateParam(@"flags", (byte)evt.Flags),
                Wrapper.CreateParam(@"data", JsonSerializer.SerializeToUtf8Bytes(evt, evt.GetType())),
                Wrapper.CreateParam(@"sender", evt.Sender?.UserId < 1 ? null : (long?)evt.Sender.UserId),
                Wrapper.CreateParam(@"sender_name", evt.Sender?.Username),
                Wrapper.CreateParam(@"sender_colour", evt.Sender?.Colour.Raw),
                Wrapper.CreateParam(@"sender_rank", evt.Sender?.Rank),
                Wrapper.CreateParam(@"sender_nick", evt.Sender?.Nickname),
                Wrapper.CreateParam(@"sender_perms", evt.Sender?.Permissions)
            );
        }

        public static void DeleteEvent(IChatEvent evt) {
            Wrapper.RunCommand(
                @"UPDATE IGNORE `sqc_events` SET `event_deleted` = NOW() WHERE `event_id` = @id AND `event_deleted` IS NULL",
                Wrapper.CreateParam(@"id", evt.SequenceId)
            );
        }

        private static IChatEvent ReadEvent(IDatabaseReader reader, IPacketTarget target = null) {
            Type evtType = Type.GetType(reader.ReadString(@"event_type"));
            IChatEvent evt = JsonSerializer.Deserialize(reader.ReadString(@"event_data"), evtType) as IChatEvent;
            evt.SequenceId = reader.ReadI64(@"event_id");
            evt.Target = target;
            evt.TargetName = target?.TargetName ?? reader.ReadString(@"event_target");
            evt.Flags = (ChatMessageFlags)reader.ReadU8(@"event_flags");
            evt.DateTime = DateTimeOffset.FromUnixTimeSeconds(reader.ReadI32(@"event_created"));

            if (!reader.IsNull(@"event_sender")) {
                evt.Sender = new BasicUser {
                    UserId = reader.ReadI64(@"event_sender"),
                    Username = reader.ReadString(@"event_sender_name"),
                    Colour = new ChatColour(reader.ReadI32(@"event_sender_colour")),
                    Rank = reader.ReadI32(@"event_sender_rank"),
                    Nickname = reader.IsNull(@"event_sender_nick") ? null : reader.ReadString(@"event_sender_nick"),
                    Permissions = (ChatUserPermissions)reader.ReadI32(@"event_sender_perms")
                };
            }

            return evt;
        }

        public static IEnumerable<IChatEvent> GetEvents(IPacketTarget target, int amount, int offset) {
            List<IChatEvent> events = new List<IChatEvent>();

            Wrapper.RunQuery(
                @"SELECT `event_id`, `event_type`, `event_flags`, `event_data`"
                + @", `event_sender`, `event_sender_name`, `event_sender_colour`, `event_sender_rank`, `event_sender_nick`, `event_sender_perms`"
                + @", UNIX_TIMESTAMP(`event_created`) AS `event_created`"
                + @" FROM `sqc_events`"
                + @" WHERE `event_deleted` IS NULL AND `event_target` = @target"
                + @" ORDER BY `event_id` DESC"
                + @" LIMIT @amount OFFSET @offset",
                reader => {
                    while(reader.Next()) {
                        IChatEvent evt = ReadEvent(reader, target);
                        if(evt != null)
                            events.Add(evt);
                    }
                },
                Wrapper.CreateParam(@"target", target.TargetName),
                Wrapper.CreateParam(@"amount", amount),
                Wrapper.CreateParam(@"offset", offset)
            );

            return events;
        }

        public static IChatEvent GetEvent(long seqId) {
            IChatEvent evt = null;

            Wrapper.RunQuery(
                @"SELECT `event_id`, `event_type`, `event_flags`, `event_data`"
                + @", `event_sender`, `event_sender_name`, `event_sender_colour`, `event_sender_rank`, `event_sender_nick`, `event_sender_perms`"
                + @", UNIX_TIMESTAMP(`event_created`) AS `event_created`"
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
    }
}
