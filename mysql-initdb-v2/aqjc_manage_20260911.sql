CREATE DATABASE IF NOT EXISTS `aqjc_manage_system`
DEFAULT CHARACTER SET utf8mb4
DEFAULT COLLATE utf8mb4_general_ci;

USE `aqjc_manage_system`;

SET NAMES utf8mb4;

DROP TABLE IF EXISTS `__efmigrationshistory`;
CREATE TABLE `__efmigrationshistory` (
  `MigrationId` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProductVersion` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

INSERT INTO `__efmigrationshistory` VALUES
('20250421065747_InitialCreate','6.0.36'),
('20250421071458_AddNewTable','6.0.36'),
('20250421074416_UpdateSMProjectInfo','6.0.36'),
('20250422024100_AddTable','6.0.36'),
('20250423024037_AddTable04231040','6.0.36'),
('20250423024629_AddTable04231046','6.0.36'),
('20250423024813_AddTable04231048','6.0.36'),
('20250506103359_add_StfaaSetp_20250506','6.0.36'),
('20250507061022_Device1ToNull202505071410','6.0.36'),
('20250507090551_AddIsCurrent202505071705','6.0.36'),
('20250607075638_Add_SM_EmployeeProfile','6.0.36'),
('20250618031106_SM_DeviceInfoTable','6.0.36'),
('20250701093000_MakeEmployeeProfileProjectIdNullable','6.0.36'),
('20260701000100_AddWatchHealthSupport','6.0.36'),
('20260722000100_AddEmployeeProfileFaceSync','6.0.36'),
('20260915090000_AddStaffInOutLog','8.0.10'),
('20260915100000_AddVideoClip','8.0.10');

DROP TABLE IF EXISTS `sm_config`;
CREATE TABLE `sm_config` (
  `ObjectId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `Code` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Value` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`ObjectId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

DROP TABLE IF EXISTS `sm_deviceinfo`;
CREATE TABLE `sm_deviceinfo` (
  `DeviceId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `DeviceName` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `DeviceNumber` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `DeviceType` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `BelongUserId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `CreateID` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `CreateDate` datetime(6) NOT NULL,
  `ModifyID` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `ModifyDate` datetime(6) DEFAULT NULL,
  `Deleted` tinyint(1) NOT NULL,
  `DeleteId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `DeleteDate` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`DeviceId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

DROP TABLE IF EXISTS `sm_employeeprofile`;
CREATE TABLE `sm_employeeprofile` (
  `ObjectId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `name` longtext NOT NULL COMMENT '姓名',
  `gender` longtext NOT NULL COMMENT '性别',
  `id` longtext NOT NULL COMMENT '身份证号',
  `phone` longtext NOT NULL COMMENT '联系方式',
  `company` longtext NOT NULL COMMENT '工作单位',
  `department` longtext NOT NULL COMMENT '工作部门',
  `ProjectId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `Device1` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT '设备Id（智能安全帽）',
  `WatchImei` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT '手表IMEI',
  `FaceImageId` varchar(100) DEFAULT NULL COMMENT 'CompreFace中的当前人脸图片ID',
  `FaceSyncStatus` int NOT NULL DEFAULT '0' COMMENT '人脸同步状态：0=未同步，1=成功，2=失败',
  `FaceSyncMessage` varchar(500) DEFAULT NULL COMMENT '最近一次人脸同步结果',
  `CreateID` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `CreateDate` datetime(6) NOT NULL,
  `ModifyID` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `ModifyDate` datetime(6) DEFAULT NULL,
  `Deleted` tinyint(1) NOT NULL,
  `DeleteId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `DeleteDate` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`ObjectId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

DROP TABLE IF EXISTS `sm_projectinfo`;
CREATE TABLE `sm_projectinfo` (
  `ObjectId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `ProjectName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ManagerName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ManagerPhone` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `StartWorkDate` datetime(6) NOT NULL,
  `Status` int NOT NULL,
  `CreateID` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `CreateDate` datetime(6) NOT NULL,
  `ModifyID` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `ModifyDate` datetime(6) DEFAULT NULL,
  `Deleted` tinyint(1) NOT NULL,
  `DeleteId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `DeleteDate` datetime(6) DEFAULT NULL,
  `IsCurrent` tinyint(1) NOT NULL DEFAULT '0',
  `GuardianName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `AdministratorName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `UnitName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  PRIMARY KEY (`ObjectId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

INSERT INTO `sm_projectinfo` VALUES
('0667ea2d-87e5-45bc-a421-fbbfa1ebfd8d','武昌中南','张子晗','17812345678','2025-08-27 00:00:00.000000',0,'78ef95ad-1e45-47f2-a8d2-bf584569486c','2025-08-27 15:08:20.376773',NULL,NULL,0,NULL,NULL,0,NULL,NULL,NULL),
('8e73031d-156f-4300-b0a2-f51c858bb798','鸣','雷','18971195697','2026-01-01 00:00:00.000000',1,'78ef95ad-1e45-47f2-a8d2-bf584569486c','2026-01-13 08:49:34.309020',NULL,NULL,0,NULL,NULL,1,NULL,NULL,NULL),
('d513566c-95d5-4dc3-95f7-64333fb131d7','测试','雷鸣','18971195697','2026-01-01 00:00:00.000000',0,'78ef95ad-1e45-47f2-a8d2-bf584569486c','2026-01-13 08:47:13.804646',NULL,NULL,0,NULL,NULL,0,NULL,NULL,NULL);

DROP TABLE IF EXISTS `sm_staffinfo`;
CREATE TABLE `sm_staffinfo` (
  `ObjectId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `ProjectId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `EmployeeProfileId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL COMMENT '人员信息台账ID-SM_EmployeeProfile-ObjectId',
  `name` longtext NOT NULL COMMENT '姓名',
  `gender` longtext NOT NULL COMMENT '性别',
  `id` longtext NOT NULL COMMENT '身份证号',
  `phone` longtext NOT NULL COMMENT '联系方式',
  `company` longtext NOT NULL COMMENT '工作单位',
  `department` longtext NOT NULL COMMENT '工作部门',
  `Status` int NOT NULL COMMENT '在场状态：0=离场 1=登记中 2=在场',
  `Device1` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT '设备1',
  `WatchImei` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT '手表IMEI',
  `CurrentStep` int NOT NULL DEFAULT '0' COMMENT '当前步骤',
  `CreateID` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `CreateDate` datetime(6) NOT NULL,
  `ModifyID` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `ModifyDate` datetime(6) DEFAULT NULL,
  `Deleted` tinyint(1) NOT NULL,
  `DeleteId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `DeleteDate` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`ObjectId`),
  KEY `IX_SM_StaffInfo_WatchImei` (`WatchImei`),
  KEY `IX_SM_StaffInfo_EmployeeProfileId` (`EmployeeProfileId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

DROP TABLE IF EXISTS `sm_stafftool`;
CREATE TABLE `sm_stafftool` (
  `ObjectId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `ParentId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `ToolName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ToolNumber` int NOT NULL,
  `Status` int NOT NULL,
  `CreateID` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `CreateDate` datetime(6) NOT NULL,
  `ModifyID` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `ModifyDate` datetime(6) DEFAULT NULL,
  `Deleted` tinyint(1) NOT NULL,
  `DeleteId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `DeleteDate` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`ObjectId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

DROP TABLE IF EXISTS `sm_watchhealthreport`;
CREATE TABLE `sm_watchhealthreport` (
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
  PRIMARY KEY (`ObjectId`),
  KEY `IX_SM_WatchHealthReport_WatchImei_ReportTime` (`WatchImei`,`ReportTime`),
  KEY `IX_SM_WatchHealthReport_StaffId_ReportTime` (`StaffId`,`ReportTime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

DROP TABLE IF EXISTS `sm_staffinoutlog`;
CREATE TABLE `sm_staffinoutlog` (
  `ObjectId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `ProjectId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `StaffInfoId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `Name` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '姓名',
  `Phone` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '联系电话',
  `Company` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '工作单位',
  `Department` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '工作部门',
  `Tools` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '携带工器具及数量汇总',
  `Device1` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT '绑定的智能安全帽设备ID（入场时）',
  `EnterTime` datetime(6) NOT NULL COMMENT '入场时间',
  `ExitTime` datetime(6) DEFAULT NULL COMMENT '退场时间',
  `CreateID` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `CreateDate` datetime(6) NOT NULL,
  `ModifyID` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `ModifyDate` datetime(6) DEFAULT NULL,
  `Deleted` tinyint(1) NOT NULL,
  `DeleteId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `DeleteDate` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`ObjectId`),
  KEY `IX_SM_StaffInOutLog_ProjectId` (`ProjectId`),
  KEY `IX_SM_StaffInOutLog_StaffInfoId` (`StaffInfoId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='人员进出登记日志（作业台账）';

DROP TABLE IF EXISTS `sm_videoclip`;
CREATE TABLE `sm_videoclip` (
  `ObjectId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `DeviceId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '设备ID（ZLM streamId）',
  `App` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'ZLM应用名',
  `RelPath` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '相对ZLM www根目录的文件路径',
  `FileName` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '文件名',
  `StartTime` datetime(6) NOT NULL COMMENT '片段开始时间',
  `EndTime` datetime(6) NOT NULL COMMENT '片段结束时间',
  `DurationSeconds` int NOT NULL COMMENT '时长（秒）',
  `FileSizeBytes` bigint NOT NULL COMMENT '文件大小（字节）',
  `ProjectId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL COMMENT '关联工程ID',
  `StaffInfoId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL COMMENT '关联人员入场登记ID',
  `Name` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '人员姓名（快照）',
  `No` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '人员证件号/工号（快照）',
  `Phone` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '联系电话（快照）',
  `Company` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '工作单位（快照）',
  `Department` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '工作部门（快照）',
  `CreateID` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `CreateDate` datetime(6) NOT NULL,
  `ModifyID` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `ModifyDate` datetime(6) DEFAULT NULL,
  `Deleted` tinyint(1) NOT NULL,
  `DeleteId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `DeleteDate` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`ObjectId`),
  KEY `IX_SM_VideoClip_DeviceId_StartTime` (`DeviceId`,`StartTime`),
  KEY `IX_SM_VideoClip_StartTime` (`StartTime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='视频回放片段';

DROP TABLE IF EXISTS `sys_attachment`;
CREATE TABLE `sys_attachment` (
  `ObjectId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `BizObjectId` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `BizObjectTable` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `DataField` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `FileName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ContentType` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ContentLength` bigint NOT NULL,
  `FileExt` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `MongoId` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CreateID` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `CreateDate` datetime(6) NOT NULL,
  `ModifyID` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `ModifyDate` datetime(6) DEFAULT NULL,
  `Deleted` tinyint(1) NOT NULL,
  `DeleteId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `DeleteDate` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`ObjectId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

DROP TABLE IF EXISTS `sys_user`;
CREATE TABLE `sys_user` (
  `UserId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `loginName` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Password` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Sex` int NOT NULL,
  `Number` int NOT NULL,
  `Email` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `SystemUser` tinyint(1) NOT NULL,
  `LastLoginDate` datetime(6) NOT NULL,
  `RecordStatus` int NOT NULL,
  `CreateID` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `CreateDate` datetime(6) NOT NULL,
  `ModifyID` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `ModifyDate` datetime(6) DEFAULT NULL,
  `Deleted` tinyint(1) NOT NULL,
  `DeleteId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `DeleteDate` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`UserId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

INSERT INTO `sys_user` VALUES ('78ef95ad-1e45-47f2-a8d2-bf584569486c','admin','管理员','9gIyodhj~YNxWZeHkEPihQ==',1,0,'huqixing@huwork.cn',0,'2026-01-13 08:57:40.173700',100,'a2213f45-e0c2-11ec-ae74-001132c4ec80','2025-08-27 09:09:06.815558','a2214c10-e0c2-11ec-ae74-001132c4ec80','2025-08-27 09:09:06.815594',0,NULL,NULL);
