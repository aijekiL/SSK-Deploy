USE `aqjc_manage_system`;

CREATE TABLE IF NOT EXISTS `__efmigrationshistory` (
  `MigrationId` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProductVersion` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

SET @column_exists := (
  SELECT COUNT(*)
  FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'sm_staffinfo'
    AND COLUMN_NAME = 'WatchImei'
);
SET @sql := IF(
  @column_exists = 0,
  'ALTER TABLE `sm_staffinfo` ADD COLUMN `WatchImei` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL AFTER `Device1`',
  'SELECT ''Column sm_staffinfo.WatchImei already exists'' AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

CREATE TABLE IF NOT EXISTS `sm_watchhealthreport` (
  `ObjectId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `WatchImei` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `StaffId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `ProjectId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `HeartRate` int DEFAULT NULL,
  `Spo2` int DEFAULT NULL,
  `Systolic` int DEFAULT NULL,
  `Diastolic` int DEFAULT NULL,
  `ReportTime` datetime(6) NOT NULL,
  `RawHex` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CreateID` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `CreateDate` datetime(6) NOT NULL,
  `ModifyID` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `ModifyDate` datetime(6) DEFAULT NULL,
  `Deleted` tinyint(1) NOT NULL,
  `DeleteId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `DeleteDate` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`ObjectId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

SET @index_exists := (
  SELECT COUNT(*)
  FROM information_schema.STATISTICS
  WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'sm_staffinfo'
    AND INDEX_NAME = 'IX_SM_StaffInfo_WatchImei'
);
SET @sql := IF(
  @index_exists = 0,
  'CREATE INDEX `IX_SM_StaffInfo_WatchImei` ON `sm_staffinfo` (`WatchImei`)',
  'SELECT ''Index IX_SM_StaffInfo_WatchImei already exists'' AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @index_exists := (
  SELECT COUNT(*)
  FROM information_schema.STATISTICS
  WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'sm_watchhealthreport'
    AND INDEX_NAME = 'IX_SM_WatchHealthReport_WatchImei_ReportTime'
);
SET @sql := IF(
  @index_exists = 0,
  'CREATE INDEX `IX_SM_WatchHealthReport_WatchImei_ReportTime` ON `sm_watchhealthreport` (`WatchImei`, `ReportTime`)',
  'SELECT ''Index IX_SM_WatchHealthReport_WatchImei_ReportTime already exists'' AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @index_exists := (
  SELECT COUNT(*)
  FROM information_schema.STATISTICS
  WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'sm_watchhealthreport'
    AND INDEX_NAME = 'IX_SM_WatchHealthReport_StaffId_ReportTime'
);
SET @sql := IF(
  @index_exists = 0,
  'CREATE INDEX `IX_SM_WatchHealthReport_StaffId_ReportTime` ON `sm_watchhealthreport` (`StaffId`, `ReportTime`)',
  'SELECT ''Index IX_SM_WatchHealthReport_StaffId_ReportTime already exists'' AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

INSERT INTO `__efmigrationshistory` (`MigrationId`, `ProductVersion`)
SELECT '20260701000100_AddWatchHealthSupport', '6.0.36'
WHERE NOT EXISTS (
  SELECT 1
  FROM `__efmigrationshistory`
  WHERE `MigrationId` = '20260701000100_AddWatchHealthSupport'
);
