using System;

// to be destroyed

namespace SharpChat.Events.Storage {
    public partial class ADOEventStorage {
        private const string CREATE_EVENTS = @"create_events_table";
        private const string ENSURE_COLLATION = @"ensure_collation";
        private const string GENERALISE_EVENTS = @"update_events_20210313";

        public void RunMigrations() {
            Wrapper.RunCommand(
                @"CREATE TABLE IF NOT EXISTS `sqc_migrations` ("
                + @"`migration_name` " + Wrapper.VarCharType(255) + @" PRIMARY KEY,"
                + @"`migration_completed` " + Wrapper.TimestampType + @" NOT NULL DEFAULT 0"
                + @");"
            );
            Wrapper.RunCommand(@"CREATE INDEX IF NOT EXISTS `sqc_migrations_completed_index` ON `sqc_migrations` (`migration_completed`);");

            DoMigration(CREATE_EVENTS, CreateEventsTable);
            DoMigration(ENSURE_COLLATION, EnsureCollation);
            DoMigration(GENERALISE_EVENTS, GeneraliseEvents);
        }

        private bool CheckMigration(string name) {
            return Wrapper.RunQueryValue(
                @"SELECT `migration_completed` IS NOT NULL FROM `sqc_migrations` WHERE `migration_name` = @name LIMIT 1",
                Wrapper.CreateParam(@"name", name)
            ) is not null;
        }

        private void DoMigration(string name, Action action) {
            if(!CheckMigration(name)) {
                Logger.Write($@"Running migration '{name}'...");
                action();
                Wrapper.RunCommand(
                    @"INSERT INTO `sqc_migrations` (`migration_name`, `migration_completed`) VALUES (@name, " + Wrapper.DateTimeNow() + @")",
                    Wrapper.CreateParam(@"name", name)
                );
            }
        }

        private void CreateEventsTable() {
            Wrapper.RunCommand(
                @"CREATE TABLE `sqc_events` ("
                + @"`event_id` " + Wrapper.BigIntType(20) + @" PRIMARY KEY,"
                + @"`event_sender` " + Wrapper.BigUIntType(20) + @" NULL DEFAULT NULL,"
                + @"`event_sender_name` " + Wrapper.VarCharType(255) + @" NULL DEFAULT NULL,"
                + @"`event_sender_colour` " + Wrapper.IntType(11) + @" NULL DEFAULT NULL,"
                + @"`event_sender_rank` " + Wrapper.IntType(11) + @" NULL DEFAULT NULL,"
                + @"`event_sender_nick` " + Wrapper.VarCharType(255) + @" NULL DEFAULT NULL,"
                + @"`event_sender_perms` " + Wrapper.IntType(11) + @" NULL DEFAULT NULL,"
                + @"`event_created` " + Wrapper.TimestampType + @" NOT NULL DEFAULT 0,"
                + @"`event_deleted` " + Wrapper.TimestampType + @" NULL DEFAULT NULL,"
                + @"`event_type` " + Wrapper.VarBinaryType(255) + @" NOT NULL,"
                + @"`event_target` " + Wrapper.VarBinaryType(255) + @" NOT NULL,"
                + @"`event_flags` " + Wrapper.TinyUIntType(3) + @" NOT NULL,"
                + @"`event_data` " + Wrapper.BlobType + @" NULL DEFAULT NULL"
                + @");"
            );
            CreateEventsTableIndices();
        }

        private void CreateEventsTableIndices() {
            Wrapper.RunCommand(@"CREATE INDEX `sqc_events_target_index` ON `sqc_events` (`event_target`);");
            Wrapper.RunCommand(@"CREATE INDEX `sqc_events_type_index` ON `sqc_events` (`event_type`);");
            Wrapper.RunCommand(@"CREATE INDEX `sqc_events_sender_index` ON `sqc_events` (`event_sender`);");
            Wrapper.RunCommand(@"CREATE INDEX `sqc_events_created_index` ON `sqc_events` (`event_created`);");
            Wrapper.RunCommand(@"CREATE INDEX `sqc_events_deleted_index` ON `sqc_events` (`event_deleted`);");
        }

        private void EnsureCollation() {
            if(Wrapper.SupportsAlterTableCollate) {
                Wrapper.RunCommand(
                    @"ALTER TABLE `sqc_events`"
                        + @"CHANGE COLUMN `event_sender_name` `event_sender_name` " + Wrapper.VarCharType(255) + @" NULL DEFAULT NULL COLLATE " + Wrapper.UnicodeCollation + @","
                        + @"CHANGE COLUMN `event_sender_nick` `event_sender_nick` " + Wrapper.VarCharType(255) + @" NULL DEFAULT NULL COLLATE " + Wrapper.UnicodeCollation + @","
                        + @"CHANGE COLUMN `event_target` `event_target` " + Wrapper.VarCharType(255) + @" NOT NULL COLLATE " + Wrapper.AsciiCollation + @";", 1800
                );
            } else {
                Wrapper.RunCommand(@"ALTER TABLE `sqc_events` RENAME TO `sqc_events_old`");
                Wrapper.RunCommand(
                    @"CREATE TABLE `sqc_events` ("
                    + @"`event_id` " + Wrapper.BigIntType(20) + @" PRIMARY KEY,"
                    + @"`event_sender` " + Wrapper.BigUIntType(20) + @" NULL DEFAULT NULL,"
                    + @"`event_sender_name` " + Wrapper.VarCharType(255) + @" NULL DEFAULT NULL COLLATE " + Wrapper.UnicodeCollation + @","
                    + @"`event_sender_colour` " + Wrapper.IntType(11) + @" NULL DEFAULT NULL,"
                    + @"`event_sender_rank` " + Wrapper.IntType(11) + @" NULL DEFAULT NULL,"
                    + @"`event_sender_nick` " + Wrapper.VarCharType(255) + @" NULL DEFAULT NULL COLLATE " + Wrapper.UnicodeCollation + @","
                    + @"`event_sender_perms` " + Wrapper.IntType(11) + @" NULL DEFAULT NULL,"
                    + @"`event_created` " + Wrapper.TimestampType + @" NOT NULL DEFAULT 0,"
                    + @"`event_deleted` " + Wrapper.TimestampType + @" NULL DEFAULT NULL,"
                    + @"`event_type` " + Wrapper.VarBinaryType(255) + @" NOT NULL,"
                    + @"`event_target` " + Wrapper.VarCharType(255) + @" NOT NULL COLLATE " + Wrapper.AsciiCollation + @","
                    + @"`event_flags` " + Wrapper.TinyUIntType(3) + @" NOT NULL,"
                    + @"`event_data` " + Wrapper.BlobType + @" NULL DEFAULT NULL"
                    + @");"
                );
                Wrapper.RunCommand(@"INSERT INTO `sqc_events` SELECT * FROM `sqc_events_old`;");
                Wrapper.RunCommand(@"DROP TABLE `sqc_events_old`;");
                CreateEventsTableIndices();
            }
        }

        private void GeneraliseEvents() {
            if(Wrapper.SupportsJson) {
                Wrapper.RunCommand(@"DELETE FROM `sqc_events` WHERE `event_type` NOT IN ('SharpChat.Events.UserChannelJoinEvent', 'SharpChat.Events.UserChannelLeaveEvent', 'SharpChat.Events.UserConnectEvent', 'SharpChat.Events.ChatMessage', 'SharpChat.Events.ChatMessageEvent');", 1800);
                Wrapper.RunCommand(@"UPDATE `sqc_events` SET `event_type` = 'channel:join' WHERE `event_type` = 'SharpChat.Events.UserChannelJoinEvent';", 1800);
                Wrapper.RunCommand(@"UPDATE `sqc_events` SET `event_type` = 'channel:leave' WHERE `event_type` = 'SharpChat.Events.UserChannelLeaveEvent';", 1800);
                Wrapper.RunCommand(@"UPDATE `sqc_events` SET `event_type` = 'user:connect' WHERE `event_type` = 'SharpChat.Events.UserConnectEvent';", 1800);
                Wrapper.RunCommand(@"UPDATE `sqc_events` SET `event_type` = 'user:disconnect' WHERE `event_type` = 'SharpChat.Events.UserDisconnectEvent';", 1800);
                Wrapper.RunCommand(@"UPDATE `sqc_events` SET `event_type` = 'message:create' WHERE `event_type` IN ('SharpChat.Events.ChatMessage', 'SharpChat.Events.ChatMessageEvent');", 1800);
                Wrapper.RunCommand(@"UPDATE `sqc_events` SET `event_target` = " + Wrapper.ToLower(@"`event_target`") + @";", 1800);
                Wrapper.RunCommand(@"UPDATE `sqc_events` SET `event_data` = " + Wrapper.JsonSet(@"`event_data`", @"$.action", @"true") + @" WHERE `event_type` = 'message:create' AND `event_flags` & 1;", 1800);
            } else {
                Wrapper.RunCommand(@"TRUNCATE `sqc_events`;", 1800);
            }
            Wrapper.RunCommand(@"ALTER TABLE `sqc_events` DROP COLUMN `event_flags`;", 1800);
        }
    }
}
