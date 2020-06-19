using MySql.Data.MySqlClient;
using System;

namespace SharpChat {
    public static partial class Database {
        private static void DoMigration(string name, Action action) {
            bool done = (long)RunQueryValue(
                @"SELECT COUNT(*) FROM `sqc_migrations` WHERE `migration_name` = @name",
                new MySqlParameter(@"name", name)
            ) > 0;
            if (!done) {
                Logger.Write($@"Running migration '{name}'...");
                action();
                RunCommand(
                    @"INSERT INTO `sqc_migrations` (`migration_name`) VALUES (@name)",
                    new MySqlParameter(@"name", name)
                );
            }
        }

        public static void RunMigrations() {
            RunCommand(
                @"CREATE TABLE IF NOT EXISTS `sqc_migrations` ("
                + @"`migration_name` VARCHAR(255) NOT NULL,"
                + @"`migration_completed` TIMESTAMP NOT NULL DEFAULT current_timestamp(),"
                + @"UNIQUE INDEX `migration_name` (`migration_name`),"
                + @"INDEX `migration_completed` (`migration_completed`)"
                + @") COLLATE='utf8mb4_unicode_ci' ENGINE=InnoDB;"
            );

            DoMigration(@"create_events_table", CreateEventsTable);
        }

        private static void CreateEventsTable() {
            RunCommand(
                @"CREATE TABLE `sqc_events` ("
                + @"`event_id` BIGINT(20) NOT NULL,"
                + @"`event_sender` BIGINT(20) UNSIGNED NULL DEFAULT NULL,"
                + @"`event_sender_name` VARCHAR(255) NULL DEFAULT NULL,"
                + @"`event_sender_colour` INT(11) NULL DEFAULT NULL,"
                + @"`event_sender_rank` INT(11) NULL DEFAULT NULL,"
                + @"`event_sender_nick` VARCHAR(255) NULL DEFAULT NULL,"
                + @"`event_sender_perms` INT(11) NULL DEFAULT NULL,"
                + @"`event_created` TIMESTAMP NOT NULL DEFAULT current_timestamp(),"
                + @"`event_deleted` TIMESTAMP NULL DEFAULT NULL,"
                + @"`event_type` VARBINARY(255) NOT NULL,"
                + @"`event_target` VARBINARY(255) NOT NULL,"
                + @"`event_flags` TINYINT(3) UNSIGNED NOT NULL,"
                + @"`event_data` BLOB NULL DEFAULT NULL,"
                + @"PRIMARY KEY (`event_id`),"
                + @"INDEX `event_target` (`event_target`),"
                + @"INDEX `event_type` (`event_type`),"
                + @"INDEX `event_sender` (`event_sender`),"
                + @"INDEX `event_datetime` (`event_created`),"
                + @"INDEX `event_deleted` (`event_deleted`)"
                + @") COLLATE='utf8mb4_unicode_ci' ENGINE=InnoDB;"
            );
        }
    }
}
