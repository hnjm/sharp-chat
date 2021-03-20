using System;

namespace SharpChat.Messages.Storage {
    public partial class ADOMessageStorage {
        private const string CREATE_MESSAGES_TABLE = @"create_msgs_table";

        public void RunMigrations() {
            Wrapper.RunCommand(
                @"CREATE TABLE IF NOT EXISTS `sqc_migrations` ("
                + @"`migration_name` " + Wrapper.VarCharType(255) + @" PRIMARY KEY,"
                + @"`migration_completed` " + Wrapper.TimestampType + @" NOT NULL DEFAULT 0"
                + @");"
            );
            Wrapper.RunCommand(@"CREATE INDEX IF NOT EXISTS `sqc_migrations_completed_index` ON `sqc_migrations` (`migration_completed`);");

            DoMigration(CREATE_MESSAGES_TABLE, CreateMessagesTable);
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

        private void CreateMessagesTable() {
            Wrapper.RunCommand(
                @"CREATE TABLE `sqc_messages` ("
                + @"`msg_id` " + Wrapper.BigIntType(20) + @" PRIMARY KEY,"
                + @"`msg_channel_name` " + Wrapper.VarCharType(255) + @" NOT NULL COLLATE " + Wrapper.AsciiCollation + @","
                + @"`msg_sender_id` " + Wrapper.BigUIntType(20) + @" NOT NULL,"
                + @"`msg_sender_name` " + Wrapper.VarCharType(255) + @" NOT NULL COLLATE " + Wrapper.UnicodeCollation + @","
                + @"`msg_sender_colour` " + Wrapper.IntType(11) + @" NOT NULL,"
                + @"`msg_sender_rank` " + Wrapper.IntType(11) + @" NOT NULL,"
                + @"`msg_sender_nick` " + Wrapper.VarCharType(255) + @" NULL DEFAULT NULL COLLATE " + Wrapper.UnicodeCollation + @","
                + @"`msg_sender_perms` " + Wrapper.IntType(11) + @" NOT NULL,"
                + @"`msg_text` " + Wrapper.TextType + @" NOT NULL COLLATE " + Wrapper.UnicodeCollation + @","
                + @"`msg_flags` " + Wrapper.TinyUIntType(3) + @" NOT NULL,"
                + @"`msg_created` " + Wrapper.TimestampType + @" NOT NULL DEFAULT 0,"
                + @"`msg_edited` " + Wrapper.TimestampType + @" NULL DEFAULT NULL,"
                + @"`msg_deleted` " + Wrapper.TimestampType + @" NULL DEFAULT NULL"
                + @");"
            );
            Wrapper.RunCommand(@"CREATE INDEX `sqc_messages_channel_index` ON `sqc_messages` (`msg_channel_name`);");
            Wrapper.RunCommand(@"CREATE INDEX `sqc_messages_sender_index` ON `sqc_events` (`msg_sender_id`);");
            Wrapper.RunCommand(@"CREATE INDEX `sqc_messages_flags_index` ON `sqc_events` (`msg_flags`);");
            Wrapper.RunCommand(@"CREATE INDEX `sqc_messages_created_index` ON `sqc_events` (`msg_created`);");
            Wrapper.RunCommand(@"CREATE INDEX `sqc_messages_edited_index` ON `sqc_events` (`msg_edited`);");
            Wrapper.RunCommand(@"CREATE INDEX `sqc_messages_deleted_index` ON `sqc_events` (`msg_deleted`);");

            // TODO: convert messages events from sqc_events
            //       can use CheckMigration(@"create_events_table"); to check if it exists 
        }
    }
}
