-- MySQL dump 10.13  Distrib 8.0.41, for Win64 (x86_64)
--
-- Host: localhost    Database: aqjc_manage_system
-- ------------------------------------------------------
-- Server version	8.0.41

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `__efmigrationshistory`
--

CREATE DATABASE IF NOT EXISTS `aqjc_manage_system`
-- 2. 指定字符集（utf8mb4兼容emoji，utf8mb4_general_ci是通用排序规则）
DEFAULT CHARACTER SET utf8mb4
-- 3. 指定排序规则（避免中文乱码/排序异常）
DEFAULT COLLATE utf8mb4_general_ci;

-- =============================================
-- 第二步：切换到aqjc_manage_system数据库（后续表操作都基于此库）
-- =============================================
USE `aqjc_manage_system`;

DROP TABLE IF EXISTS `__efmigrationshistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `__efmigrationshistory` (
  `MigrationId` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProductVersion` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `__efmigrationshistory`
--

LOCK TABLES `__efmigrationshistory` WRITE;
/*!40000 ALTER TABLE `__efmigrationshistory` DISABLE KEYS */;
INSERT INTO `__efmigrationshistory` VALUES ('20250421065747_InitialCreate','6.0.36'),('20250421071458_AddNewTable','6.0.36'),('20250421074416_UpdateSMProjectInfo','6.0.36'),('20250422024100_AddTable','6.0.36'),('20250423024037_AddTable04231040','6.0.36'),('20250423024629_AddTable04231046','6.0.36'),('20250423024813_AddTable04231048','6.0.36'),('20250506103359_add_StfaaSetp_20250506','6.0.36'),('20250507061022_Device1ToNull202505071410','6.0.36'),('20250507090551_AddIsCurrent202505071705','6.0.36'),('20250607075638_Add_SM_EmployeeProfile','6.0.36'),('20250618031106_SM_DeviceInfoTable','6.0.36'),('20250701093000_MakeEmployeeProfileProjectIdNullable','6.0.36');
/*!40000 ALTER TABLE `__efmigrationshistory` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `sm_config`
--

DROP TABLE IF EXISTS `sm_config`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sm_config` (
  `ObjectId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `Code` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Value` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`ObjectId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `sm_config`
--

LOCK TABLES `sm_config` WRITE;
/*!40000 ALTER TABLE `sm_config` DISABLE KEYS */;
/*!40000 ALTER TABLE `sm_config` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `sm_deviceinfo`
--

DROP TABLE IF EXISTS `sm_deviceinfo`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `sm_deviceinfo`
--

LOCK TABLES `sm_deviceinfo` WRITE;
/*!40000 ALTER TABLE `sm_deviceinfo` DISABLE KEYS */;
/*!40000 ALTER TABLE `sm_deviceinfo` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `sm_employeeprofile`
--

DROP TABLE IF EXISTS `sm_employeeprofile`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sm_employeeprofile` (
  `ObjectId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `name` longtext NOT NULL COMMENT '姓名',
  `gender` longtext NOT NULL COMMENT '性别',
  `id` longtext NOT NULL COMMENT '身份证号',
  `phone` longtext NOT NULL COMMENT '联系方式',
  `company` longtext NOT NULL COMMENT '工作单位',
  `department` longtext NOT NULL COMMENT '工作部门',
  `ProjectId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `CreateID` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `CreateDate` datetime(6) NOT NULL,
  `ModifyID` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `ModifyDate` datetime(6) DEFAULT NULL,
  `Deleted` tinyint(1) NOT NULL,
  `DeleteId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `DeleteDate` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`ObjectId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `sm_employeeprofile`
--

LOCK TABLES `sm_employeeprofile` WRITE;
/*!40000 ALTER TABLE `sm_employeeprofile` DISABLE KEYS */;
INSERT INTO `sm_employeeprofile` VALUES ('04571c1c-1802-432e-8a52-2a217b0ce24e','雷欧','男','420116199907295118','18971195697','中国电力工程顾问集团中南电力设计院有限公司','zs','00000000-0000-0000-0000-000000000000','78ef95ad-1e45-47f2-a8d2-bf584569486c','2025-12-19 09:01:54.772731',NULL,NULL,0,NULL,NULL),('0cbe7458-4489-413a-8f71-27adb1d3f5a7','老李','男','420106196600000000','15522222222','中南电力设计院','数智科技公司',NULL,'78ef95ad-1e45-47f2-a8d2-bf584569486c','2025-09-17 15:34:26.470324',NULL,NULL,0,NULL,NULL),('18b1c9ce-1ed4-4d04-bd4a-a0eb1f6ebc63','小王','男','420106200000000000','15511111111','中南电力设计院','数智科技公司',NULL,'78ef95ad-1e45-47f2-a8d2-bf584569486c','2025-09-17 15:32:53.510874',NULL,NULL,0,NULL,NULL),('3dda05f9-abd3-411c-904f-2e4280f2127d','雷鸣','男','420115199907295118','18971195697','中国电力工程顾问集团中南电力设计院有限公司','zs','00000000-0000-0000-0000-000000000000','78ef95ad-1e45-47f2-a8d2-bf584569486c','2025-12-19 09:01:19.426546',NULL,NULL,0,NULL,NULL),('47b89a9d-7ec7-4d59-8ea1-d478f630c675','雷欧','男','420116199907425118','18971195697','中国电力工程顾问集团中南电力设计院有限公司','zs','00000000-0000-0000-0000-000000000000','78ef95ad-1e45-47f2-a8d2-bf584569486c','2026-01-12 17:06:06.918903',NULL,NULL,0,NULL,NULL),('67cf3bf8-c619-4c75-8063-b3da46083e9b','雷峰','男','420116199907295110','18971165555','中国电力工程顾问集团中南电力设计院有限公司','szhj',NULL,'78ef95ad-1e45-47f2-a8d2-bf584569486c','2026-01-13 08:50:01.436489',NULL,NULL,0,NULL,NULL),('717abfc9-16af-4a48-9cb2-5cbc31b6240d','李四','男','420106199500000000','15544444444','中南电力设计院','数智科技公司',NULL,'78ef95ad-1e45-47f2-a8d2-bf584569486c','2025-09-17 15:51:31.109897',NULL,'2025-09-17 17:11:22.237827',0,NULL,NULL),('8428cc34-2f3d-4910-800f-5db4aca9c0fa','雷8','男','420111199907895118','18971195697','中国电力工程顾问集团中南电力设计院有限公司','sz',NULL,'78ef95ad-1e45-47f2-a8d2-bf584569486c','2026-01-13 09:55:15.458428',NULL,NULL,0,NULL,NULL),('8a9f105f-4c66-4fc3-aa3b-749e9a7d8163','雷鸣','男','420115199907795118','18971195697','中国电力工程顾问集团中南电力设计院有限公司','sz',NULL,'78ef95ad-1e45-47f2-a8d2-bf584569486c','2026-01-13 09:49:59.356620',NULL,NULL,0,NULL,NULL),('a9a0ffa5-3757-472b-89d0-e238ffaf193f','leifeng','男','420111199907295119','18971195697','ez','中国电力工程顾问集团中南电力设计院有限公司',NULL,'78ef95ad-1e45-47f2-a8d2-bf584569486c','2025-12-12 14:19:13.189624',NULL,NULL,0,NULL,NULL),('aeb285d2-158f-43cd-82d8-90d87fb37ba4','雷7','男','420111199907795118','18971195697','中国电力工程顾问集团中南电力设计院有限公司','sz',NULL,'78ef95ad-1e45-47f2-a8d2-bf584569486c','2026-01-13 09:51:25.729010',NULL,NULL,0,NULL,NULL),('bd863f83-b7e8-4d14-abf9-b5b741d9833e','老郑','男','420106197000000000','15533333333','中南电力设计院','数智科技公司',NULL,'78ef95ad-1e45-47f2-a8d2-bf584569486c','2025-09-17 15:35:25.540241',NULL,NULL,0,NULL,NULL);
/*!40000 ALTER TABLE `sm_employeeprofile` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `sm_projectinfo`
--

DROP TABLE IF EXISTS `sm_projectinfo`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
  PRIMARY KEY (`ObjectId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `sm_projectinfo`
--

LOCK TABLES `sm_projectinfo` WRITE;
/*!40000 ALTER TABLE `sm_projectinfo` DISABLE KEYS */;
INSERT INTO `sm_projectinfo` VALUES ('0667ea2d-87e5-45bc-a421-fbbfa1ebfd8d','武昌中南','张子晗','17812345678','2025-08-27 00:00:00.000000',0,'78ef95ad-1e45-47f2-a8d2-bf584569486c','2025-08-27 15:08:20.376773',NULL,NULL,0,NULL,NULL,0),('8e73031d-156f-4300-b0a2-f51c858bb798','鸣','雷','18971195697','2026-01-01 00:00:00.000000',1,'78ef95ad-1e45-47f2-a8d2-bf584569486c','2026-01-13 08:49:34.309020',NULL,NULL,0,NULL,NULL,0),('d513566c-95d5-4dc3-95f7-64333fb131d7','测试','雷鸣','18971195697','2026-01-01 00:00:00.000000',0,'78ef95ad-1e45-47f2-a8d2-bf584569486c','2026-01-13 08:47:13.804646',NULL,NULL,0,NULL,NULL,0);
/*!40000 ALTER TABLE `sm_projectinfo` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `sm_staffinfo`
--

DROP TABLE IF EXISTS `sm_staffinfo`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sm_staffinfo` (
  `ObjectId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `ProjectId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `name` longtext NOT NULL COMMENT '姓名',
  `gender` longtext NOT NULL COMMENT '性别',
  `id` longtext NOT NULL COMMENT '身份证号',
  `phone` longtext NOT NULL COMMENT '联系方式',
  `company` longtext NOT NULL COMMENT '工作单位',
  `department` longtext NOT NULL COMMENT '工作部门',
  `Status` int NOT NULL,
  `Device1` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `CreateID` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `CreateDate` datetime(6) NOT NULL,
  `ModifyID` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `ModifyDate` datetime(6) DEFAULT NULL,
  `Deleted` tinyint(1) NOT NULL,
  `DeleteId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `DeleteDate` datetime(6) DEFAULT NULL,
  `CurrentStep` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`ObjectId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `sm_staffinfo`
--

LOCK TABLES `sm_staffinfo` WRITE;
/*!40000 ALTER TABLE `sm_staffinfo` DISABLE KEYS */;
INSERT INTO `sm_staffinfo` VALUES ('0999eede-de9e-47c2-976e-d43561828ab2','0667ea2d-87e5-45bc-a421-fbbfa1ebfd8d','雷鸣','','420117199907295119','18971195697','ez','中国电力工程顾问集团中南电力设计院有限公司',1,'未绑定','00000000-0000-0000-0000-000000000000','2025-12-12 14:18:41.027022',NULL,NULL,0,NULL,NULL,3),('0e41a4d9-404b-47fe-8ee5-53656cafa2de','0667ea2d-87e5-45bc-a421-fbbfa1ebfd8d','雷7','男','420111199907795118','18971195697','中国电力工程顾问集团中南电力设计院有限公司','sz',1,'未绑定','00000000-0000-0000-0000-000000000000','2026-01-13 09:51:25.675248',NULL,NULL,0,NULL,NULL,1),('28cbb1f1-41d7-4b6d-9bca-0e10b754fdeb','0667ea2d-87e5-45bc-a421-fbbfa1ebfd8d','雷鸣','男','420115199907295118','18971195697','中国电力工程顾问集团中南电力设计院有限公司','zs',2,'12314','00000000-0000-0000-0000-000000000000','2025-12-19 09:01:19.373298',NULL,NULL,0,NULL,NULL,4),('2ed80f7a-a7ac-4872-94db-0e3dd6daa26a','0667ea2d-87e5-45bc-a421-fbbfa1ebfd8d','雷8','男','420111199907895118','18971195697','中国电力工程顾问集团中南电力设计院有限公司','sz',2,'1236','00000000-0000-0000-0000-000000000000','2026-01-13 09:55:15.372864',NULL,NULL,0,NULL,NULL,4),('54f391db-99b5-4b67-9111-992dae86013d','0667ea2d-87e5-45bc-a421-fbbfa1ebfd8d','雷鸣','男','420115199907295118','18971195697','ez','中国电力工程顾问集团中南电力设计院有限公司',1,'未绑定','00000000-0000-0000-0000-000000000000','2025-12-12 11:20:20.809406',NULL,NULL,0,NULL,NULL,2),('558353c3-73f5-4bd0-8873-4ed9bb854434','8e73031d-156f-4300-b0a2-f51c858bb798','雷峰','男','420116199907295110','18971165555','中国电力工程顾问集团中南电力设计院有限公司','szhj',0,'未绑定','00000000-0000-0000-0000-000000000000','2026-01-13 08:50:01.403496',NULL,NULL,0,NULL,NULL,4);
/*!40000 ALTER TABLE `sm_staffinfo` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `sm_stafftool`
--

DROP TABLE IF EXISTS `sm_stafftool`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `sm_stafftool`
--

LOCK TABLES `sm_stafftool` WRITE;
/*!40000 ALTER TABLE `sm_stafftool` DISABLE KEYS */;
/*!40000 ALTER TABLE `sm_stafftool` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `sys_attachment`
--

DROP TABLE IF EXISTS `sys_attachment`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `sys_attachment`
--

LOCK TABLES `sys_attachment` WRITE;
/*!40000 ALTER TABLE `sys_attachment` DISABLE KEYS */;
INSERT INTO `sys_attachment` VALUES ('02cb46cf-a7e8-4caa-850d-4a14815154bd','558353c3-73f5-4bd0-8873-4ed9bb854434','SM_StaffInfo','djz','DSC_0138-01-01.jpeg','image/jpeg',45577,'.jpeg','6964b963d559cee2771365ec','78ef95ad-1e45-47f2-a8d2-bf584569486c','2026-01-13 08:49:58.993339',NULL,NULL,0,NULL,NULL),('085dd15e-24ca-4d17-b4a3-766d540a6406','98758de1-a079-4d03-a48e-9b12d97fe772','SM_StaffInfo','rczp','培训合同书.jpg','image/jpeg',89026,'.jpg','693b938d9f2cef8576a2ef75','78ef95ad-1e45-47f2-a8d2-bf584569486c','2025-12-12 12:01:17.473280',NULL,NULL,0,NULL,NULL),('0ded5980-113f-4e05-8a02-eeec3304cd49','','SM_StaffInfo','djz','image (2).png','image/jpeg',20883,'.png','6944a0b22c6aed9dbca651e8','78ef95ad-1e45-47f2-a8d2-bf584569486c','2025-12-19 08:55:04.516948',NULL,NULL,0,NULL,NULL),('120c46d7-1966-45d8-9b20-534b3b75e397','d8d05a19-0d7b-4fb3-a3ea-84dfdeb0a32a','SM_StaffInfo','rczp','DSC_0138-01-01.jpeg','image/jpeg',45577,'.jpeg','6964b963d559cee2771365ec','78ef95ad-1e45-47f2-a8d2-bf584569486c','2026-01-13 09:50:11.351400',NULL,NULL,0,NULL,NULL),('128fc6fd-e2e8-43ae-9720-575786c2d1fc','','SM_StaffInfo','djz','0b7fb25ffd84764eb778cc8f8faa025e.jpg','image/jpeg',157763,'.jpg','693bb3bf6b445b28cb15cc25','78ef95ad-1e45-47f2-a8d2-bf584569486c','2025-12-17 08:42:40.732610',NULL,NULL,0,NULL,NULL),('153c51d2-4253-4d88-9320-91f2323ef495','98758de1-a079-4d03-a48e-9b12d97fe772','SM_StaffInfo','djz','60ac50a5e6d9276a3a05f12561d32771_origin.jpg','image/jpeg',104327,'.jpg','693b93809f2cef8576a2ef73','78ef95ad-1e45-47f2-a8d2-bf584569486c','2025-12-12 12:01:04.720786',NULL,NULL,0,NULL,NULL),('2cc9997e-994b-4164-bfba-bafe3fc221d9','','SM_StaffInfo','djz','image (2).png','image/jpeg',20883,'.png','6944a0b22c6aed9dbca651e8','78ef95ad-1e45-47f2-a8d2-bf584569486c','2025-12-19 08:52:40.634351',NULL,NULL,0,NULL,NULL),('30f49d56-edff-4ae6-9305-e59ca16fb4fe','28cbb1f1-41d7-4b6d-9bca-0e10b754fdeb','SM_StaffInfo','rczp','image (2).png','image/jpeg',20883,'.png','6944a0b22c6aed9dbca651e8','78ef95ad-1e45-47f2-a8d2-bf584569486c','2025-12-19 09:01:27.123016',NULL,NULL,0,NULL,NULL),('38dc4364-2faf-4736-b9ec-3577db2183d8','87960d4d-1773-4510-be7b-f28876c85ff3','SM_StaffInfo','rczp','logo2.png','image/png',76627,'.png','693b93619f2cef8576a2ef71','78ef95ad-1e45-47f2-a8d2-bf584569486c','2026-01-13 08:48:24.924422',NULL,NULL,0,NULL,NULL),('4132acc4-96b0-44bf-86e6-f9677e60f2a4','83a9ce56-91f0-426e-b942-e4ca1df6c774','SM_StaffInfo','djz','logo2.png','image/png',76627,'.png','693b93619f2cef8576a2ef71','78ef95ad-1e45-47f2-a8d2-bf584569486c','2025-12-12 12:00:33.478853',NULL,NULL,0,NULL,NULL),('4b333e09-a2f5-46e3-ad96-bc6c9346cdb4','87960d4d-1773-4510-be7b-f28876c85ff3','SM_StaffInfo','djz','logo2.png','image/png',76627,'.png','693b93619f2cef8576a2ef71','78ef95ad-1e45-47f2-a8d2-bf584569486c','2026-01-13 08:48:08.176969',NULL,NULL,0,NULL,NULL),('61378419-c520-4361-ae56-cdcb98c205f7','','SM_StaffInfo','djz','0b7fb25ffd84764eb778cc8f8faa025e.jpg','image/jpeg',157763,'.jpg','693bb3bf6b445b28cb15cc25','78ef95ad-1e45-47f2-a8d2-bf584569486c','2025-12-17 18:28:57.323653',NULL,NULL,0,NULL,NULL),('65f1838f-8a83-4b69-a5e7-9c96c6f463e4','e6521e05-5d7f-46cc-b5ea-ef31fc78b99d','SM_StaffInfo','djz','DSC_0138-01-01.jpeg','image/jpeg',45577,'.jpeg','6964b963d559cee2771365ec','78ef95ad-1e45-47f2-a8d2-bf584569486c','2026-01-12 17:05:39.796324',NULL,NULL,0,NULL,NULL),('66aed305-b1d4-468a-bb2b-1ee5401dea2c','d8d05a19-0d7b-4fb3-a3ea-84dfdeb0a32a','SM_StaffInfo','djz','logo2.png','image/png',76627,'.png','693b93619f2cef8576a2ef71','78ef95ad-1e45-47f2-a8d2-bf584569486c','2026-01-13 09:49:58.397513',NULL,NULL,0,NULL,NULL),('74c4c2c9-36a2-4ed8-8b22-8f788957dc15','0999eede-de9e-47c2-976e-d43561828ab2','SM_StaffInfo','rczp','0b7fb25ffd84764eb778cc8f8faa025e.jpg','image/jpeg',157763,'.jpg','693bb3bf6b445b28cb15cc25','78ef95ad-1e45-47f2-a8d2-bf584569486c','2025-12-12 14:18:55.226647',NULL,NULL,0,NULL,NULL),('7eeec495-7fc6-4a09-9b40-15158f857585','78e8235b-a046-4dbd-8629-0df20b905626','SM_StaffInfo','djz','38acc44435318d3b487888d9fb06b58.png','image/jpeg',54181,'.png','693b93989f2cef8576a2ef77','78ef95ad-1e45-47f2-a8d2-bf584569486c','2025-12-12 12:01:28.509576',NULL,NULL,0,NULL,NULL),('822486ec-b76d-461c-9478-069a784d02ab','','SM_StaffInfo','djz','logo2.png','image/png',76627,'.png','693b93619f2cef8576a2ef71','78ef95ad-1e45-47f2-a8d2-bf584569486c','2026-01-13 09:50:38.997587',NULL,NULL,0,NULL,NULL),('87044abb-4a73-4eca-a586-1f4c543eef91','bd863f83-b7e8-4d14-abf9-b5b741d9833e','SM_EmployeeProfile','djz','生成登记照 (4).png','image/jpeg',58938,'.png','68ca64bad07d58d70a6414cd','78ef95ad-1e45-47f2-a8d2-bf584569486c','2025-09-17 15:35:22.685889',NULL,NULL,0,NULL,NULL),('8a5c3c72-02f7-4937-910b-cf5c5812ee0d','2ed80f7a-a7ac-4872-94db-0e3dd6daa26a','SM_StaffInfo','djz','logo2.png','image/png',76627,'.png','693b93619f2cef8576a2ef71','78ef95ad-1e45-47f2-a8d2-bf584569486c','2026-01-13 09:54:59.953442',NULL,NULL,0,NULL,NULL),('93021dce-ddb7-4fc7-b6d1-cda37c335540','','SM_StaffInfo','djz','logo2.png','image/png',76627,'.png','693b93619f2cef8576a2ef71','78ef95ad-1e45-47f2-a8d2-bf584569486c','2026-01-13 09:51:55.297296',NULL,NULL,0,NULL,NULL),('96ade963-e615-4ade-9c69-f0071158a3f4','e6521e05-5d7f-46cc-b5ea-ef31fc78b99d','SM_StaffInfo','rczp','DSC_0138-01-01.jpeg','image/jpeg',45577,'.jpeg','6964b963d559cee2771365ec','78ef95ad-1e45-47f2-a8d2-bf584569486c','2026-01-12 17:06:15.704344',NULL,NULL,0,NULL,NULL),('a36f9413-b4cb-4274-9509-5b9a05723735','','SM_StaffInfo','djz','0b7fb25ffd84764eb778cc8f8faa025e.jpg','image/jpeg',157763,'.jpg','693bb3bf6b445b28cb15cc25','78ef95ad-1e45-47f2-a8d2-bf584569486c','2025-12-18 10:12:29.530639',NULL,NULL,0,NULL,NULL),('a422c81a-34b3-4271-85b3-46193835458c','2ed80f7a-a7ac-4872-94db-0e3dd6daa26a','SM_StaffInfo','rczp','logo2.png','image/png',76627,'.png','693b93619f2cef8576a2ef71','78ef95ad-1e45-47f2-a8d2-bf584569486c','2026-01-13 09:55:22.096038',NULL,NULL,0,NULL,NULL),('a9fe5450-bfa5-4616-8158-07ad04e1573a','717abfc9-16af-4a48-9cb2-5cbc31b6240d','SM_EmployeeProfile','djz','testdjz.png','image/jpeg',36262,'.png','68ca683cd07d58d70a6414cf','78ef95ad-1e45-47f2-a8d2-bf584569486c','2025-09-17 17:11:21.356313',NULL,NULL,0,NULL,NULL),('aa6134cb-7e47-473b-b6fc-89fc487e0b38','','SM_StaffInfo','djz','image (2).png','image/jpeg',20883,'.png','6944a0b22c6aed9dbca651e8','78ef95ad-1e45-47f2-a8d2-bf584569486c','2025-12-19 08:47:46.122433',NULL,NULL,0,NULL,NULL),('ac09d7cd-05ea-4acc-b285-9aed5f104d53','78e8235b-a046-4dbd-8629-0df20b905626','SM_StaffInfo','rczp','图片1.png','image/png',148143,'.png','693b93a29f2cef8576a2ef79','78ef95ad-1e45-47f2-a8d2-bf584569486c','2025-12-12 12:01:38.856344',NULL,NULL,0,NULL,NULL),('b4b94370-480b-431b-a853-52cb975c1d20','','SM_StaffInfo','djz','logo2.png','image/png',76627,'.png','693b93619f2cef8576a2ef71','78ef95ad-1e45-47f2-a8d2-bf584569486c','2026-01-13 09:35:44.774568',NULL,NULL,0,NULL,NULL),('bbbfd5b7-0b70-4271-8232-35dd942af23d','558353c3-73f5-4bd0-8873-4ed9bb854434','SM_StaffInfo','rczp','DSC_0138-01-01.jpeg','image/jpeg',45577,'.jpeg','6964b963d559cee2771365ec','78ef95ad-1e45-47f2-a8d2-bf584569486c','2026-01-13 08:50:08.070557',NULL,NULL,0,NULL,NULL),('cf08338c-cc4a-43e9-ac60-71aadbcf7da0','18b1c9ce-1ed4-4d04-bd4a-a0eb1f6ebc63','SM_EmployeeProfile','djz','生成登记照 (10).png','image/jpeg',58725,'.png','68ca63ffd07d58d70a6414c9','78ef95ad-1e45-47f2-a8d2-bf584569486c','2025-09-17 15:32:15.652014',NULL,NULL,0,NULL,NULL),('d1cb18c4-53ef-41f5-b769-e57ec4363660','5f79654c-d22d-4cd2-867b-cbc669aebd78','SM_StaffInfo','djz','image.png','image/jpeg',44420,'.png','6944a4012c6aed9dbca651ea','78ef95ad-1e45-47f2-a8d2-bf584569486c','2025-12-19 09:01:53.510403',NULL,NULL,0,NULL,NULL),('d319bddd-5611-4015-a9b8-1aeb659093b2','28cbb1f1-41d7-4b6d-9bca-0e10b754fdeb','SM_StaffInfo','djz','image (2).png','image/jpeg',20883,'.png','6944a0b22c6aed9dbca651e8','78ef95ad-1e45-47f2-a8d2-bf584569486c','2025-12-19 09:01:03.784951',NULL,NULL,0,NULL,NULL),('d9f3e4b3-753c-426a-9eea-148b0f46dad1','f93b862b-9614-4ccc-913a-d30cc4c5d845','SM_StaffInfo','djz','DSC_0138-01-01.jpeg','image/jpeg',45577,'.jpeg','6964b963d559cee2771365ec','78ef95ad-1e45-47f2-a8d2-bf584569486c','2026-01-13 09:24:20.276776',NULL,NULL,0,NULL,NULL),('e0a64360-50a2-464d-8fe4-246651ceadf5','f93b862b-9614-4ccc-913a-d30cc4c5d845','SM_StaffInfo','rczp','DSC_0138-01-01.jpeg','image/jpeg',45577,'.jpeg','6964b963d559cee2771365ec','78ef95ad-1e45-47f2-a8d2-bf584569486c','2026-01-13 09:24:36.177996',NULL,NULL,0,NULL,NULL),('eb016638-4db2-43b0-a50b-0e24a2c5de05','0cbe7458-4489-413a-8f71-27adb1d3f5a7','SM_EmployeeProfile','djz','生成登记照 (5).png','image/jpeg',63805,'.png','68ca6480d07d58d70a6414cb','78ef95ad-1e45-47f2-a8d2-bf584569486c','2025-09-17 15:34:24.545801',NULL,NULL,0,NULL,NULL),('ec8d578e-ecb0-48b6-8a3a-d5e0e6665b72','597198ff-7bee-4e9b-95f7-31d7cc3a4330','SM_StaffInfo','djz','0b7fb25ffd84764eb778cc8f8faa025e.jpg','image/jpeg',157763,'.jpg','693bb3bf6b445b28cb15cc25','78ef95ad-1e45-47f2-a8d2-bf584569486c','2025-12-12 14:18:39.082886',NULL,NULL,0,NULL,NULL),('f93ace4c-fcad-4ae2-acd1-4c77341527c2','0e41a4d9-404b-47fe-8ee5-53656cafa2de','SM_StaffInfo','djz','logo2.png','image/png',76627,'.png','693b93619f2cef8576a2ef71','78ef95ad-1e45-47f2-a8d2-bf584569486c','2026-01-13 09:51:23.350260',NULL,NULL,0,NULL,NULL),('f9f039ca-9930-49f5-b48c-baf139a42984','54f391db-99b5-4b67-9111-992dae86013d','SM_StaffInfo','djz','中南数智logo.png','image/png',49976,'.png','693b89f1f40bd18727216e91','78ef95ad-1e45-47f2-a8d2-bf584569486c','2025-12-12 11:20:17.966450',NULL,NULL,0,NULL,NULL);
/*!40000 ALTER TABLE `sys_attachment` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `sys_user`
--

DROP TABLE IF EXISTS `sys_user`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `sys_user`
--

LOCK TABLES `sys_user` WRITE;
/*!40000 ALTER TABLE `sys_user` DISABLE KEYS */;
INSERT INTO `sys_user` VALUES ('78ef95ad-1e45-47f2-a8d2-bf584569486c','admin','管理员','9gIyodhj~YNxWZeHkEPihQ==',1,0,'huqixing@huwork.cn',0,'2026-01-13 08:57:40.173700',100,'a2213f45-e0c2-11ec-ae74-001132c4ec80','2025-08-27 09:09:06.815558','a2214c10-e0c2-11ec-ae74-001132c4ec80','2025-08-27 09:09:06.815594',0,NULL,NULL);
/*!40000 ALTER TABLE `sys_user` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping routines for database 'aqjc_manage_system'
--

-- Dump completed on 2026-01-13 18:53:30
