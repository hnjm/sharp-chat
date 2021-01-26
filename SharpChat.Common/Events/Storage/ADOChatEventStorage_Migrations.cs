using System;

namespace SharpChat.Events.Storage {
    public partial class ADOChatEventStorage {
        public void RunMigrations() {
            Wrapper.RunCommand(
                @"CREATE TABLE IF NOT EXISTS `sqc_migrations` ("
                + @"`migration_name` " + Wrapper.VarCharType(255) + @" PRIMARY KEY,"
                + @"`migration_completed` " + Wrapper.TimestampType + @" NOT NULL DEFAULT 0"
                + @");"
            );
            Wrapper.RunCommand(@"CREATE INDEX IF NOT EXISTS `sqc_migrations_completed_index` ON `sqc_migrations` (`migration_completed`);");

            DoMigration(@"create_events_table", CreateEventsTable);
            DoMigration(@"ensure_collation", EnsureCollation);
        }

        private void DoMigration(string name, Action action) {
            bool done = (long)Wrapper.RunQueryValue(
                @"SELECT COUNT(*) FROM `sqc_migrations` WHERE `migration_name` = @name",
                Wrapper.CreateParam(@"name", name)
            ) > 0;
            if(!done) {
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
                        + @"CHANGE COLUMN `event_target` `event_target` " + Wrapper.VarCharType(255) + @" NOT NULL COLLATE " + Wrapper.AsciiCollation + @";"
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
    }
}
