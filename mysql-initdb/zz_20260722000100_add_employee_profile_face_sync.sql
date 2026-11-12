USE `aqjc_manage_system`;

SET @column_exists := (
  SELECT COUNT(*) FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'sm_staffinfo' AND COLUMN_NAME = 'EmployeeProfileId'
);
SET @sql := IF(
  @column_exists = 0,
  'ALTER TABLE `sm_staffinfo` ADD COLUMN `EmployeeProfileId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL',
  'SELECT ''Column sm_staffinfo.EmployeeProfileId already exists'' AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @column_exists := (
  SELECT COUNT(*) FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'sm_employeeprofile' AND COLUMN_NAME = 'FaceImageId'
);
SET @sql := IF(
  @column_exists = 0,
  'ALTER TABLE `sm_employeeprofile` ADD COLUMN `FaceImageId` varchar(100) DEFAULT NULL',
  'SELECT ''Column sm_employeeprofile.FaceImageId already exists'' AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @column_exists := (
  SELECT COUNT(*) FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'sm_employeeprofile' AND COLUMN_NAME = 'FaceSyncStatus'
);
SET @sql := IF(
  @column_exists = 0,
  'ALTER TABLE `sm_employeeprofile` ADD COLUMN `FaceSyncStatus` int NOT NULL DEFAULT 0',
  'SELECT ''Column sm_employeeprofile.FaceSyncStatus already exists'' AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @column_exists := (
  SELECT COUNT(*) FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'sm_employeeprofile' AND COLUMN_NAME = 'FaceSyncMessage'
);
SET @sql := IF(
  @column_exists = 0,
  'ALTER TABLE `sm_employeeprofile` ADD COLUMN `FaceSyncMessage` varchar(500) DEFAULT NULL',
  'SELECT ''Column sm_employeeprofile.FaceSyncMessage already exists'' AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @index_exists := (
  SELECT COUNT(*) FROM information_schema.STATISTICS
  WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'sm_staffinfo' AND INDEX_NAME = 'IX_SM_StaffInfo_EmployeeProfileId'
);
SET @sql := IF(
  @index_exists = 0,
  'CREATE INDEX `IX_SM_StaffInfo_EmployeeProfileId` ON `sm_staffinfo` (`EmployeeProfileId`)',
  'SELECT ''Index IX_SM_StaffInfo_EmployeeProfileId already exists'' AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

INSERT INTO `__efmigrationshistory` (`MigrationId`, `ProductVersion`)
SELECT '20260722000100_AddEmployeeProfileFaceSync', '6.0.36'
WHERE NOT EXISTS (
  SELECT 1 FROM `__efmigrationshistory`
  WHERE `MigrationId` = '20260722000100_AddEmployeeProfileFaceSync'
);
