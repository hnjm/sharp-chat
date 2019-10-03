CREATE TABLE `sharp_log` (
    `log_id`       INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
    `log_target`   VARCHAR(255)     NOT NULL                 COLLATE 'utf8mb4_bin',
    `user_id`      INT(10) UNSIGNED NOT NULL,
    `user_name`    VARCHAR(255)     NOT NULL                 COLLATE 'utf8mb4_bin',
    `user_colour`  INT(10) UNSIGNED NOT NULL,
    `log_datetime` DATETIME         NOT NULL,
    `log_text`     TEXT             NOT NULL DEFAULT ''      COLLATE 'utf8mb4_bin',
    `log_flags`    INT(10) UNSIGNED NOT NULL,
    PRIMARY KEY (`log_id`),
             INDEX `sharp_log_user_id`  (`user_id`),
             INDEX `sharp_log_target`   (`log_target`),
             INDEX `sharp_log_datetime` (`log_datetime`),
    FULLTEXT INDEX `sharp_log_text`     (`log_text`),
    FULLTEXT INDEX `sharp_log_username` (`user_name`)
) COLLATE='utf8mb4_bin' ENGINE=InnoDB;
