-- 达梦数据库初始化脚本
-- 1. 创建数据库（如果使用管理工具已创建，可跳过）
CREATE DATABASE "YXKJ_CAPS";

-- 2. 创建业务用户并授权
CREATE USER "CAPS_USER" IDENTIFIED BY "CAPS_PASSWORD";
GRANT DBA TO "CAPS_USER";

-- 3. 连接到数据库（在客户端中执行时，可先执行下行示例）
-- USE "YXKJ_CAPS";
SET SCHEMA "CAPS_USER";

-- 4. 删除现有表（如果表存在）
DROP TABLE IF EXISTS sys_user_role;
DROP TABLE IF EXISTS sys_user_post;
DROP TABLE IF EXISTS sys_role_menu;
DROP TABLE IF EXISTS sys_role_dept;
DROP TABLE IF EXISTS sys_user;
DROP TABLE IF EXISTS sys_role;
DROP TABLE IF EXISTS sys_post;
DROP TABLE IF EXISTS sys_oper_log;
DROP TABLE IF EXISTS sys_notice;
DROP TABLE IF EXISTS sys_menu;
DROP TABLE IF EXISTS sys_logininfor;
DROP TABLE IF EXISTS sys_job_log;
DROP TABLE IF EXISTS sys_job;
DROP TABLE IF EXISTS sys_dict_type;
DROP TABLE IF EXISTS sys_dict_data;
DROP TABLE IF EXISTS sys_dept;
DROP TABLE IF EXISTS sys_config;
DROP TABLE IF EXISTS qrtz_simprop_triggers;
DROP TABLE IF EXISTS qrtz_cron_triggers;
DROP TABLE IF EXISTS qrtz_blob_triggers;
DROP TABLE IF EXISTS qrtz_triggers;
DROP TABLE IF EXISTS qrtz_job_details;
DROP TABLE IF EXISTS qrtz_simple_triggers;
DROP TABLE IF EXISTS qrtz_scheduler_state;
DROP TABLE IF EXISTS qrtz_paused_trigger_grps;
DROP TABLE IF EXISTS qrtz_locks;
DROP TABLE IF EXISTS qrtz_calendars;
DROP TABLE IF EXISTS qrtz_fired_triggers;
DROP TABLE IF EXISTS gen_table_column;
DROP TABLE IF EXISTS gen_table;
DROP TABLE IF EXISTS caps_device_report;
DROP TABLE IF EXISTS caps_device_alarm;
DROP TABLE IF EXISTS caps_device;
DROP TABLE IF EXISTS audible_voice_alarm;

-- 5. 创建 audible_voice_alarm 表
CREATE TABLE audible_voice_alarm (
  id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
  device_code VARCHAR(125) NOT NULL,
  person_id VARCHAR(125) NOT NULL,
  create_by VARCHAR(64) DEFAULT '',
  create_time DATETIME,
  update_by VARCHAR(64) DEFAULT '',
  update_time DATETIME,
  remark VARCHAR(255)
);

COMMENT ON COLUMN audible_voice_alarm.id IS 'ID';
COMMENT ON COLUMN audible_voice_alarm.device_code IS '设备编码';
COMMENT ON COLUMN audible_voice_alarm.person_id IS '客户ID';
COMMENT ON COLUMN audible_voice_alarm.create_by IS '创建人';
COMMENT ON COLUMN audible_voice_alarm.create_time IS '创建时间';
COMMENT ON COLUMN audible_voice_alarm.update_by IS '更新人';
COMMENT ON COLUMN audible_voice_alarm.update_time IS '更新时间';
COMMENT ON COLUMN audible_voice_alarm.remark IS '备注';
COMMENT ON TABLE audible_voice_alarm IS '声光报警器设备信息表';

-- 6. 创建 caps_device 表
CREATE TABLE caps_device (
  id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
  device_id VARCHAR(125) NOT NULL,
  person_id VARCHAR(125) NOT NULL,
  user_id VARCHAR(125) NOT NULL,
  user_name VARCHAR(125),
  mobile VARCHAR(125),
  create_by VARCHAR(64) DEFAULT '',
  create_time DATETIME,
  update_by VARCHAR(64) DEFAULT '',
  update_time DATETIME,
  remark VARCHAR(255)
);

COMMENT ON COLUMN caps_device.id IS 'ID';
COMMENT ON COLUMN caps_device.device_id IS '设备ID';
COMMENT ON COLUMN caps_device.person_id IS '客户ID';
COMMENT ON COLUMN caps_device.user_id IS '用户ID';
COMMENT ON COLUMN caps_device.user_name IS '用户名';
COMMENT ON COLUMN caps_device.mobile IS '手机号';
COMMENT ON COLUMN caps_device.create_by IS '创建人';
COMMENT ON COLUMN caps_device.create_time IS '创建时间';
COMMENT ON COLUMN caps_device.update_by IS '更新人';
COMMENT ON COLUMN caps_device.update_time IS '更新时间';
COMMENT ON COLUMN caps_device.remark IS '备注';
COMMENT ON TABLE caps_device IS '安全帽设备信息表';

-- 7. 插入初始数据（仅示例，可按需调整）
INSERT INTO caps_device (id, device_id, person_id, user_id, user_name, mobile, create_by, create_time, update_by, update_time, remark)
VALUES (1, '866652022038138', 'P-002', 'U-001', 'tester-01', '13556263578', '', NULL, '', NULL, NULL);
INSERT INTO caps_device (id, device_id, person_id, user_id, user_name, mobile, create_by, create_time, update_by, update_time, remark)
VALUES (2, '866652022426580', 'P-002', 'U-002', 'sensor-02', '13026327533', '', NULL, '', NULL, NULL);
INSERT INTO caps_device (id, device_id, person_id, user_id, user_name, mobile, create_by, create_time, update_by, update_time, remark)
VALUES (3, '866652022496187', 'P-003', 'U-003', 'helmet-03', '13026324515', '', NULL, '', NULL, NULL);
INSERT INTO caps_device (id, device_id, person_id, user_id, user_name, mobile, create_by, create_time, update_by, update_time, remark)
VALUES (4, '866652022399050', '11111111-1111-1111-1111-111111111111', 'U-004', 'cm-electric', '15556234555', '', NULL, '', NULL, NULL);
INSERT INTO caps_device (id, device_id, person_id, user_id, user_name, mobile, create_by, create_time, update_by, update_time, remark)
VALUES (5, '866652022498464', '11111111-1111-1111-1111-111111111111', 'U-005', 'safety-gas-05', '15556234555', '', NULL, '', NULL, NULL);
INSERT INTO caps_device (id, device_id, person_id, user_id, user_name, mobile, create_by, create_time, update_by, update_time, remark)
VALUES (6, '866652022498399', '11111111-1111-1111-1111-111111111111', 'U-006', 'safety-gas-06', '15556234555', '', NULL, '', NULL, NULL);
INSERT INTO caps_device (id, device_id, person_id, user_id, user_name, mobile, create_by, create_time, update_by, update_time, remark)
VALUES (7, '866652022498449', '11111111-1111-1111-1111-111111111111', 'U-007', 'yxkj_002_01', NULL, '', NULL, '', NULL, NULL);
INSERT INTO caps_device (id, device_id, person_id, user_id, user_name, mobile, create_by, create_time, update_by, update_time, remark)
VALUES (8, '866652022498407', '11111111-1111-1111-1111-111111111111', 'U-008', 'yxkj_002_02', NULL, '', NULL, '', NULL, NULL);
INSERT INTO caps_device (id, device_id, person_id, user_id, user_name, mobile, create_by, create_time, update_by, update_time, remark)
VALUES (9, '866652022498423', '11111111-1111-1111-1111-111111111111', 'U-009', 'yxkj_002_03', NULL, '', NULL, '', NULL, NULL);
INSERT INTO caps_device (id, device_id, person_id, user_id, user_name, mobile, create_by, create_time, update_by, update_time, remark)
VALUES (10, '866652022498415', '11111111-1111-1111-1111-111111111111', 'U-010', 'yxkj_002_04', NULL, '', NULL, '', NULL, NULL);
INSERT INTO caps_device (id, device_id, person_id, user_id, user_name, mobile, create_by, create_time, update_by, update_time, remark)
VALUES (11, '866652022498332', '11111111-1111-1111-1111-111111111111', 'U-011', 'yxkj_002_05', NULL, '', NULL, '', NULL, NULL);
INSERT INTO caps_device (id, device_id, person_id, user_id, user_name, mobile, create_by, create_time, update_by, update_time, remark)
VALUES (12, '866652022498431', '11111111-1111-1111-1111-111111111111', 'U-012', 'yxkj_002_06', NULL, '', NULL, '', NULL, NULL);
INSERT INTO caps_device (id, device_id, person_id, user_id, user_name, mobile, create_by, create_time, update_by, update_time, remark)
VALUES (13, '866652022589726', '11111111-1111-1111-1111-111111111111', 'U-013', 'yxkj_002_07', NULL, '', NULL, '', NULL, NULL);
INSERT INTO caps_device (id, device_id, person_id, user_id, user_name, mobile, create_by, create_time, update_by, update_time, remark)
VALUES (14, '866652022498340', '11111111-1111-1111-1111-111111111111', 'U-014', 'yxkj_002_08', NULL, '', NULL, '', NULL, NULL);
INSERT INTO caps_device (id, device_id, person_id, user_id, user_name, mobile, create_by, create_time, update_by, update_time, remark)
VALUES (15, '866652022591052', '11111111-1111-1111-1111-111111111111', 'U-015', 'yxkj_002_09', NULL, '', NULL, '', NULL, NULL);

INSERT INTO audible_voice_alarm (id, device_code, person_id, create_by, create_time, update_by, update_time, remark)
VALUES (2, '09-250406008', '11111111-1111-1111-1111-111111111111', '', NULL, '', NULL, NULL);

COMMIT;
