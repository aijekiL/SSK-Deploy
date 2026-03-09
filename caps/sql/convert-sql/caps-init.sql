-- ============================================================
-- 达梦数据库（DM8）初始化脚本
-- 兼容 ry-zhdj 模式下的所有业务表 + Quartz + 系统管理表
-- 作者：根据用户提供的 MySQL 脚本自动转换
-- 日期：2025-11-22
-- ============================================================

-- ----------------------------
-- 1. 创建表（无注释）
-- ----------------------------

CREATE TABLE "audible_voice_alarm" (
  "id" INT NOT NULL IDENTITY(1,1),
  "device_code" VARCHAR(125) NOT NULL,
  "person_id" VARCHAR(125) NOT NULL,
  "create_by" VARCHAR(64) DEFAULT '',
  "create_time" DATETIME DEFAULT NULL,
  "update_by" VARCHAR(64) DEFAULT '',
  "update_time" DATETIME DEFAULT NULL,
  "remark" VARCHAR(255) DEFAULT NULL,
  PRIMARY KEY ("id")
);

CREATE TABLE "caps_device" (
  "id" INT NOT NULL IDENTITY(1,1),
  "device_id" VARCHAR(125) NOT NULL,
  "person_id" VARCHAR(125) NOT NULL,
  "user_id" VARCHAR(125) NOT NULL,
  "user_name" VARCHAR(125) DEFAULT NULL,
  "mobile" VARCHAR(125) DEFAULT NULL,
  "create_by" VARCHAR(64) DEFAULT '',
  "create_time" DATETIME DEFAULT NULL,
  "update_by" VARCHAR(64) DEFAULT '',
  "update_time" DATETIME DEFAULT NULL,
  "remark" VARCHAR(255) DEFAULT NULL,
  PRIMARY KEY ("id")
);

CREATE TABLE "caps_device_alarm" (
  "id" INT NOT NULL IDENTITY(1,1),
  "device_id" VARCHAR(125) DEFAULT NULL,
  "type" INT DEFAULT 0,
  "type_info" VARCHAR(125) DEFAULT NULL,
  "x_point" VARCHAR(125) DEFAULT NULL,
  "y_point" VARCHAR(125) DEFAULT NULL,
  "create_by" VARCHAR(64) DEFAULT '',
  "create_time" DATETIME DEFAULT NULL,
  "update_by" VARCHAR(64) DEFAULT '',
  "update_time" DATETIME DEFAULT NULL,
  "remark" VARCHAR(255) DEFAULT NULL,
  PRIMARY KEY ("id")
);

CREATE TABLE "caps_device_report" (
  "id" INT NOT NULL IDENTITY(1,1),
  "create_time" DATETIME DEFAULT NULL,
  "device_id" VARCHAR(125) DEFAULT NULL,
  "altitude" VARCHAR(25) DEFAULT NULL,
  "bat_l" VARCHAR(25) DEFAULT NULL,
  "bat_v" VARCHAR(25) DEFAULT NULL,
  "bt_signal_info" VARCHAR(125) DEFAULT NULL,
  "c_speed" VARCHAR(25) DEFAULT NULL,
  "charging" VARCHAR(25) DEFAULT NULL,
  "gps" VARCHAR(25) DEFAULT NULL,
  "gps_level" VARCHAR(25) DEFAULT NULL,
  "in_use" VARCHAR(25) DEFAULT NULL,
  "is_weared" VARCHAR(25) DEFAULT NULL,
  "net_strenth" VARCHAR(25) DEFAULT NULL,
  "net_type" VARCHAR(25) DEFAULT NULL,
  "phoneNumber" VARCHAR(25) DEFAULT NULL,
  "sim_data_num" VARCHAR(25) DEFAULT NULL,
  "sim_status" VARCHAR(25) DEFAULT NULL,
  "tcard_status" VARCHAR(25) DEFAULT NULL,
  "tempdata" VARCHAR(125) DEFAULT NULL,
  "user_id" VARCHAR(25) DEFAULT NULL,
  "x_point" VARCHAR(25) DEFAULT NULL,
  "y_point" VARCHAR(25) DEFAULT NULL,
  "create_by" VARCHAR(64) DEFAULT '',
  "update_by" VARCHAR(64) DEFAULT '',
  "update_time" DATETIME DEFAULT NULL,
  "remark" VARCHAR(255) DEFAULT NULL,
  "ch4" VARCHAR(100) DEFAULT NULL,
  "o2" VARCHAR(100) DEFAULT NULL,
  "co" VARCHAR(100) DEFAULT NULL,
  "h2s" VARCHAR(100) DEFAULT NULL,
  PRIMARY KEY ("id")
);

CREATE TABLE "gen_table" (
  "table_id" BIGINT NOT NULL IDENTITY(1,1),
  "table_name" VARCHAR(200) DEFAULT '',
  "table_comment" VARCHAR(500) DEFAULT '',
  "sub_table_name" VARCHAR(64) DEFAULT NULL,
  "sub_table_fk_name" VARCHAR(64) DEFAULT NULL,
  "class_name" VARCHAR(100) DEFAULT '',
  "tpl_category" VARCHAR(200) DEFAULT 'crud',
  "tpl_web_type" VARCHAR(30) DEFAULT '',
  "package_name" VARCHAR(100) DEFAULT NULL,
  "module_name" VARCHAR(30) DEFAULT NULL,
  "business_name" VARCHAR(30) DEFAULT NULL,
  "function_name" VARCHAR(50) DEFAULT NULL,
  "function_author" VARCHAR(50) DEFAULT NULL,
  "gen_type" CHAR(1) DEFAULT '0',
  "gen_path" VARCHAR(200) DEFAULT '/',
  "options" VARCHAR(1000) DEFAULT NULL,
  "create_by" VARCHAR(64) DEFAULT '',
  "create_time" DATETIME DEFAULT NULL,
  "update_by" VARCHAR(64) DEFAULT '',
  "update_time" DATETIME DEFAULT NULL,
  "remark" VARCHAR(500) DEFAULT NULL,
  PRIMARY KEY ("table_id")
);

CREATE TABLE "gen_table_column" (
  "column_id" BIGINT NOT NULL IDENTITY(1,1),
  "table_id" BIGINT DEFAULT NULL,
  "column_name" VARCHAR(200) DEFAULT NULL,
  "column_comment" VARCHAR(500) DEFAULT NULL,
  "column_type" VARCHAR(100) DEFAULT NULL,
  "java_type" VARCHAR(500) DEFAULT NULL,
  "java_field" VARCHAR(200) DEFAULT NULL,
  "is_pk" CHAR(1) DEFAULT NULL,
  "is_increment" CHAR(1) DEFAULT NULL,
  "is_required" CHAR(1) DEFAULT NULL,
  "is_insert" CHAR(1) DEFAULT NULL,
  "is_edit" CHAR(1) DEFAULT NULL,
  "is_list" CHAR(1) DEFAULT NULL,
  "is_query" CHAR(1) DEFAULT NULL,
  "query_type" VARCHAR(200) DEFAULT 'EQ',
  "html_type" VARCHAR(200) DEFAULT NULL,
  "dict_type" VARCHAR(200) DEFAULT '',
  "sort" INT DEFAULT NULL,
  "create_by" VARCHAR(64) DEFAULT '',
  "create_time" DATETIME DEFAULT NULL,
  "update_by" VARCHAR(64) DEFAULT '',
  "update_time" DATETIME DEFAULT NULL,
  PRIMARY KEY ("column_id")
);

CREATE TABLE "qrtz_blob_triggers" (
  "sched_name" VARCHAR(120) NOT NULL,
  "trigger_name" VARCHAR(200) NOT NULL,
  "trigger_group" VARCHAR(200) NOT NULL,
  "blob_data" BLOB,
  PRIMARY KEY ("sched_name", "trigger_name", "trigger_group")
);

CREATE TABLE "qrtz_calendars" (
  "sched_name" VARCHAR(120) NOT NULL,
  "calendar_name" VARCHAR(200) NOT NULL,
  "calendar" BLOB NOT NULL,
  PRIMARY KEY ("sched_name", "calendar_name")
);

CREATE TABLE "qrtz_cron_triggers" (
  "sched_name" VARCHAR(120) NOT NULL,
  "trigger_name" VARCHAR(200) NOT NULL,
  "trigger_group" VARCHAR(200) NOT NULL,
  "cron_expression" VARCHAR(200) NOT NULL,
  "time_zone_id" VARCHAR(80) DEFAULT NULL,
  PRIMARY KEY ("sched_name", "trigger_name", "trigger_group")
);

CREATE TABLE "qrtz_fired_triggers" (
  "sched_name" VARCHAR(120) NOT NULL,
  "entry_id" VARCHAR(95) NOT NULL,
  "trigger_name" VARCHAR(200) NOT NULL,
  "trigger_group" VARCHAR(200) NOT NULL,
  "instance_name" VARCHAR(200) NOT NULL,
  "fired_time" BIGINT NOT NULL,
  "sched_time" BIGINT NOT NULL,
  "priority" INT NOT NULL,
  "state" VARCHAR(16) NOT NULL,
  "job_name" VARCHAR(200) DEFAULT NULL,
  "job_group" VARCHAR(200) DEFAULT NULL,
  "is_nonconcurrent" VARCHAR(1) DEFAULT NULL,
  "requests_recovery" VARCHAR(1) DEFAULT NULL,
  PRIMARY KEY ("sched_name", "entry_id")
);

CREATE TABLE "qrtz_job_details" (
  "sched_name" VARCHAR(120) NOT NULL,
  "job_name" VARCHAR(200) NOT NULL,
  "job_group" VARCHAR(200) NOT NULL,
  "description" VARCHAR(250) DEFAULT NULL,
  "job_class_name" VARCHAR(250) NOT NULL,
  "is_durable" VARCHAR(1) NOT NULL,
  "is_nonconcurrent" VARCHAR(1) NOT NULL,
  "is_update_data" VARCHAR(1) NOT NULL,
  "requests_recovery" VARCHAR(1) NOT NULL,
  "job_data" BLOB,
  PRIMARY KEY ("sched_name", "job_name", "job_group")
);

CREATE TABLE "qrtz_locks" (
  "sched_name" VARCHAR(120) NOT NULL,
  "lock_name" VARCHAR(40) NOT NULL,
  PRIMARY KEY ("sched_name", "lock_name")
);

CREATE TABLE "qrtz_paused_trigger_grps" (
  "sched_name" VARCHAR(120) NOT NULL,
  "trigger_group" VARCHAR(200) NOT NULL,
  PRIMARY KEY ("sched_name", "trigger_group")
);

CREATE TABLE "qrtz_scheduler_state" (
  "sched_name" VARCHAR(120) NOT NULL,
  "instance_name" VARCHAR(200) NOT NULL,
  "last_checkin_time" BIGINT NOT NULL,
  "checkin_interval" BIGINT NOT NULL,
  PRIMARY KEY ("sched_name", "instance_name")
);

CREATE TABLE "qrtz_simple_triggers" (
  "sched_name" VARCHAR(120) NOT NULL,
  "trigger_name" VARCHAR(200) NOT NULL,
  "trigger_group" VARCHAR(200) NOT NULL,
  "repeat_count" BIGINT NOT NULL,
  "repeat_interval" BIGINT NOT NULL,
  "times_triggered" BIGINT NOT NULL,
  PRIMARY KEY ("sched_name", "trigger_name", "trigger_group")
);

CREATE TABLE "qrtz_simprop_triggers" (
  "sched_name" VARCHAR(120) NOT NULL,
  "trigger_name" VARCHAR(200) NOT NULL,
  "trigger_group" VARCHAR(200) NOT NULL,
  "str_prop_1" VARCHAR(512) DEFAULT NULL,
  "str_prop_2" VARCHAR(512) DEFAULT NULL,
  "str_prop_3" VARCHAR(512) DEFAULT NULL,
  "int_prop_1" INT DEFAULT NULL,
  "int_prop_2" INT DEFAULT NULL,
  "long_prop_1" BIGINT DEFAULT NULL,
  "long_prop_2" BIGINT DEFAULT NULL,
  "dec_prop_1" DECIMAL(13,4) DEFAULT NULL,
  "dec_prop_2" DECIMAL(13,4) DEFAULT NULL,
  "bool_prop_1" VARCHAR(1) DEFAULT NULL,
  "bool_prop_2" VARCHAR(1) DEFAULT NULL,
  PRIMARY KEY ("sched_name", "trigger_name", "trigger_group")
);

CREATE TABLE "qrtz_triggers" (
  "sched_name" VARCHAR(120) NOT NULL,
  "trigger_name" VARCHAR(200) NOT NULL,
  "trigger_group" VARCHAR(200) NOT NULL,
  "job_name" VARCHAR(200) NOT NULL,
  "job_group" VARCHAR(200) NOT NULL,
  "description" VARCHAR(250) DEFAULT NULL,
  "next_fire_time" BIGINT DEFAULT NULL,
  "prev_fire_time" BIGINT DEFAULT NULL,
  "priority" INT DEFAULT NULL,
  "trigger_state" VARCHAR(16) NOT NULL,
  "trigger_type" VARCHAR(8) NOT NULL,
  "start_time" BIGINT NOT NULL,
  "end_time" BIGINT DEFAULT NULL,
  "calendar_name" VARCHAR(200) DEFAULT NULL,
  "misfire_instr" SMALLINT DEFAULT NULL,
  "job_data" BLOB,
  PRIMARY KEY ("sched_name", "trigger_name", "trigger_group")
);

CREATE TABLE "sys_config" (
  "config_id" INT NOT NULL IDENTITY(1,1),
  "config_name" VARCHAR(100) DEFAULT '',
  "config_key" VARCHAR(100) DEFAULT '',
  "config_value" VARCHAR(500) DEFAULT '',
  "config_type" CHAR(1) DEFAULT 'N',
  "create_by" VARCHAR(64) DEFAULT '',
  "create_time" DATETIME DEFAULT NULL,
  "update_by" VARCHAR(64) DEFAULT '',
  "update_time" DATETIME DEFAULT NULL,
  "remark" VARCHAR(500) DEFAULT NULL,
  PRIMARY KEY ("config_id")
);

CREATE TABLE "sys_dept" (
  "dept_id" BIGINT NOT NULL IDENTITY(1,1),
  "parent_id" BIGINT DEFAULT 0,
  "ancestors" VARCHAR(50) DEFAULT '',
  "dept_name" VARCHAR(30) DEFAULT '',
  "order_num" INT DEFAULT 0,
  "leader" VARCHAR(20) DEFAULT NULL,
  "phone" VARCHAR(11) DEFAULT NULL,
  "email" VARCHAR(50) DEFAULT NULL,
  "status" CHAR(1) DEFAULT '0',
  "del_flag" CHAR(1) DEFAULT '0',
  "create_by" VARCHAR(64) DEFAULT '',
  "create_time" DATETIME DEFAULT NULL,
  "update_by" VARCHAR(64) DEFAULT '',
  "update_time" DATETIME DEFAULT NULL,
  PRIMARY KEY ("dept_id")
);

CREATE TABLE "sys_dict_data" (
  "dict_code" BIGINT NOT NULL IDENTITY(1,1),
  "dict_sort" INT DEFAULT 0,
  "dict_label" VARCHAR(100) DEFAULT '',
  "dict_value" VARCHAR(100) DEFAULT '',
  "dict_type" VARCHAR(100) DEFAULT '',
  "css_class" VARCHAR(100) DEFAULT NULL,
  "list_class" VARCHAR(100) DEFAULT NULL,
  "is_default" CHAR(1) DEFAULT 'N',
  "status" CHAR(1) DEFAULT '0',
  "create_by" VARCHAR(64) DEFAULT '',
  "create_time" DATETIME DEFAULT NULL,
  "update_by" VARCHAR(64) DEFAULT '',
  "update_time" DATETIME DEFAULT NULL,
  "remark" VARCHAR(500) DEFAULT NULL,
  PRIMARY KEY ("dict_code")
);

CREATE TABLE "sys_dict_type" (
  "dict_id" BIGINT NOT NULL IDENTITY(1,1),
  "dict_name" VARCHAR(100) DEFAULT '',
  "dict_type" VARCHAR(100) DEFAULT '',
  "status" CHAR(1) DEFAULT '0',
  "create_by" VARCHAR(64) DEFAULT '',
  "create_time" DATETIME DEFAULT NULL,
  "update_by" VARCHAR(64) DEFAULT '',
  "update_time" DATETIME DEFAULT NULL,
  "remark" VARCHAR(500) DEFAULT NULL,
  PRIMARY KEY ("dict_id")
);

CREATE TABLE "sys_job" (
  "job_id" BIGINT NOT NULL IDENTITY(1,1),
  "job_name" VARCHAR(64) NOT NULL DEFAULT '',
  "job_group" VARCHAR(64) NOT NULL DEFAULT 'DEFAULT',
  "invoke_target" VARCHAR(500) NOT NULL,
  "cron_expression" VARCHAR(255) DEFAULT '',
  "misfire_policy" VARCHAR(20) DEFAULT '3',
  "concurrent" CHAR(1) DEFAULT '1',
  "status" CHAR(1) DEFAULT '0',
  "create_by" VARCHAR(64) DEFAULT '',
  "create_time" DATETIME DEFAULT NULL,
  "update_by" VARCHAR(64) DEFAULT '',
  "update_time" DATETIME DEFAULT NULL,
  "remark" VARCHAR(500) DEFAULT '',
  PRIMARY KEY ("job_id", "job_name", "job_group")
);

CREATE TABLE "sys_job_log" (
  "job_log_id" BIGINT NOT NULL IDENTITY(1,1),
  "job_name" VARCHAR(64) NOT NULL,
  "job_group" VARCHAR(64) NOT NULL,
  "invoke_target" VARCHAR(500) NOT NULL,
  "job_message" VARCHAR(500) DEFAULT NULL,
  "status" CHAR(1) DEFAULT '0',
  "exception_info" VARCHAR(2000) DEFAULT '',
  "create_time" DATETIME DEFAULT NULL,
  PRIMARY KEY ("job_log_id")
);

CREATE TABLE "sys_logininfor" (
  "info_id" BIGINT NOT NULL IDENTITY(1,1),
  "user_name" VARCHAR(50) DEFAULT '',
  "ipaddr" VARCHAR(128) DEFAULT '',
  "login_location" VARCHAR(255) DEFAULT '',
  "browser" VARCHAR(50) DEFAULT '',
  "os" VARCHAR(50) DEFAULT '',
  "status" CHAR(1) DEFAULT '0',
  "msg" VARCHAR(255) DEFAULT '',
  "login_time" DATETIME DEFAULT NULL,
  PRIMARY KEY ("info_id")
);

CREATE TABLE "sys_menu" (
  "menu_id" BIGINT NOT NULL IDENTITY(1,1),
  "menu_name" VARCHAR(50) NOT NULL,
  "parent_id" BIGINT DEFAULT 0,
  "order_num" INT DEFAULT 0,
  "path" VARCHAR(200) DEFAULT '',
  "component" VARCHAR(255) DEFAULT NULL,
  "query" VARCHAR(255) DEFAULT NULL,
  "route_name" VARCHAR(50) DEFAULT '',
  "is_frame" INT DEFAULT 1,
  "is_cache" INT DEFAULT 0,
  "menu_type" CHAR(1) DEFAULT '',
  "visible" CHAR(1) DEFAULT '0',
  "status" CHAR(1) DEFAULT '0',
  "perms" VARCHAR(100) DEFAULT NULL,
  "icon" VARCHAR(100) DEFAULT '#',
  "create_by" VARCHAR(64) DEFAULT '',
  "create_time" DATETIME DEFAULT NULL,
  "update_by" VARCHAR(64) DEFAULT '',
  "update_time" DATETIME DEFAULT NULL,
  "remark" VARCHAR(500) DEFAULT '',
  PRIMARY KEY ("menu_id")
);

CREATE TABLE "sys_notice" (
  "notice_id" INT NOT NULL IDENTITY(1,1),
  "notice_title" VARCHAR(50) NOT NULL,
  "notice_type" CHAR(1) NOT NULL,
  "notice_content" TEXT,
  "status" CHAR(1) DEFAULT '0',
  "create_by" VARCHAR(64) DEFAULT '',
  "create_time" DATETIME DEFAULT NULL,
  "update_by" VARCHAR(64) DEFAULT '',
  "update_time" DATETIME DEFAULT NULL,
  "remark" VARCHAR(255) DEFAULT NULL,
  PRIMARY KEY ("notice_id")
);

CREATE TABLE "sys_oper_log" (
  "oper_id" BIGINT NOT NULL IDENTITY(1,1),
  "title" VARCHAR(50) DEFAULT '',
  "business_type" INT DEFAULT 0,
  "method" VARCHAR(200) DEFAULT '',
  "request_method" VARCHAR(10) DEFAULT '',
  "operator_type" INT DEFAULT 0,
  "oper_name" VARCHAR(50) DEFAULT '',
  "dept_name" VARCHAR(50) DEFAULT '',
  "oper_url" VARCHAR(255) DEFAULT '',
  "oper_ip" VARCHAR(128) DEFAULT '',
  "oper_location" VARCHAR(255) DEFAULT '',
  "oper_param" VARCHAR(2000) DEFAULT '',
  "json_result" VARCHAR(2000) DEFAULT '',
  "status" INT DEFAULT 0,
  "error_msg" VARCHAR(2000) DEFAULT '',
  "oper_time" DATETIME DEFAULT NULL,
  "cost_time" BIGINT DEFAULT 0,
  PRIMARY KEY ("oper_id")
);

CREATE TABLE "sys_post" (
  "post_id" BIGINT NOT NULL IDENTITY(1,1),
  "post_code" VARCHAR(64) NOT NULL,
  "post_name" VARCHAR(50) NOT NULL,
  "post_sort" INT NOT NULL,
  "status" CHAR(1) NOT NULL,
  "create_by" VARCHAR(64) DEFAULT '',
  "create_time" DATETIME DEFAULT NULL,
  "update_by" VARCHAR(64) DEFAULT '',
  "update_time" DATETIME DEFAULT NULL,
  "remark" VARCHAR(500) DEFAULT NULL,
  PRIMARY KEY ("post_id")
);

CREATE TABLE "sys_role" (
  "role_id" BIGINT NOT NULL IDENTITY(1,1),
  "role_name" VARCHAR(30) NOT NULL,
  "role_key" VARCHAR(100) NOT NULL,
  "role_sort" INT NOT NULL,
  "data_scope" CHAR(1) DEFAULT '1',
  "menu_check_strictly" TINYINT DEFAULT 1,
  "dept_check_strictly" TINYINT DEFAULT 1,
  "status" CHAR(1) NOT NULL,
  "del_flag" CHAR(1) DEFAULT '0',
  "create_by" VARCHAR(64) DEFAULT '',
  "create_time" DATETIME DEFAULT NULL,
  "update_by" VARCHAR(64) DEFAULT '',
  "update_time" DATETIME DEFAULT NULL,
  "remark" VARCHAR(500) DEFAULT NULL,
  PRIMARY KEY ("role_id")
);

CREATE TABLE "sys_role_dept" (
  "role_id" BIGINT NOT NULL,
  "dept_id" BIGINT NOT NULL,
  PRIMARY KEY ("role_id", "dept_id")
);

CREATE TABLE "sys_role_menu" (
  "role_id" BIGINT NOT NULL,
  "menu_id" BIGINT NOT NULL,
  PRIMARY KEY ("role_id", "menu_id")
);

CREATE TABLE "sys_user" (
  "user_id" BIGINT NOT NULL IDENTITY(1,1),
  "dept_id" BIGINT DEFAULT NULL,
  "user_name" VARCHAR(30) NOT NULL,
  "nick_name" VARCHAR(30) NOT NULL,
  "user_type" VARCHAR(2) DEFAULT '00',
  "email" VARCHAR(50) DEFAULT '',
  "phonenumber" VARCHAR(11) DEFAULT '',
  "sex" CHAR(1) DEFAULT '0',
  "avatar" VARCHAR(100) DEFAULT '',
  "password" VARCHAR(100) DEFAULT '',
  "status" CHAR(1) DEFAULT '0',
  "del_flag" CHAR(1) DEFAULT '0',
  "login_ip" VARCHAR(128) DEFAULT '',
  "login_date" DATETIME DEFAULT NULL,
  "create_by" VARCHAR(64) DEFAULT '',
  "create_time" DATETIME DEFAULT NULL,
  "update_by" VARCHAR(64) DEFAULT '',
  "update_time" DATETIME DEFAULT NULL,
  "remark" VARCHAR(500) DEFAULT NULL,
  PRIMARY KEY ("user_id")
);

CREATE TABLE "sys_user_post" (
  "user_id" BIGINT NOT NULL,
  "post_id" BIGINT NOT NULL,
  PRIMARY KEY ("user_id", "post_id")
);

CREATE TABLE "sys_user_role" (
  "user_id" BIGINT NOT NULL,
  "role_id" BIGINT NOT NULL,
  PRIMARY KEY ("user_id", "role_id")
);

-- ----------------------------
-- 2. 添加表和列注释
-- ----------------------------

COMMENT ON TABLE "audible_voice_alarm" IS '声光报警器设备信息表';
COMMENT ON COLUMN "audible_voice_alarm"."id" IS 'ID';
COMMENT ON COLUMN "audible_voice_alarm"."device_code" IS '设备编码';
COMMENT ON COLUMN "audible_voice_alarm"."person_id" IS '客户id';
COMMENT ON COLUMN "audible_voice_alarm"."create_by" IS '创建者';
COMMENT ON COLUMN "audible_voice_alarm"."create_time" IS '创建时间';
COMMENT ON COLUMN "audible_voice_alarm"."update_by" IS '更新者';
COMMENT ON COLUMN "audible_voice_alarm"."update_time" IS '更新时间';
COMMENT ON COLUMN "audible_voice_alarm"."remark" IS '备注';

COMMENT ON TABLE "caps_device" IS '安全帽设备信息表';
COMMENT ON COLUMN "caps_device"."id" IS 'ID';
COMMENT ON COLUMN "caps_device"."device_id" IS '设备id';
COMMENT ON COLUMN "caps_device"."person_id" IS '客户id';
COMMENT ON COLUMN "caps_device"."user_id" IS '用户id';
COMMENT ON COLUMN "caps_device"."user_name" IS '用户名';
COMMENT ON COLUMN "caps_device"."mobile" IS '手机号';
COMMENT ON COLUMN "caps_device"."create_by" IS '创建者';
COMMENT ON COLUMN "caps_device"."create_time" IS '创建时间';
COMMENT ON COLUMN "caps_device"."update_by" IS '更新者';
COMMENT ON COLUMN "caps_device"."update_time" IS '更新时间';
COMMENT ON COLUMN "caps_device"."remark" IS '备注';

COMMENT ON TABLE "caps_device_alarm" IS '安全帽设备告警信息表';
COMMENT ON COLUMN "caps_device_alarm"."id" IS 'ID';
COMMENT ON COLUMN "caps_device_alarm"."device_id" IS '设备id';
COMMENT ON COLUMN "caps_device_alarm"."type" IS '报警类型';
COMMENT ON COLUMN "caps_device_alarm"."type_info" IS '报警信息';
COMMENT ON COLUMN "caps_device_alarm"."x_point" IS '纬度';
COMMENT ON COLUMN "caps_device_alarm"."y_point" IS '经度';
COMMENT ON COLUMN "caps_device_alarm"."create_by" IS '创建者';
COMMENT ON COLUMN "caps_device_alarm"."create_time" IS '创建时间';
COMMENT ON COLUMN "caps_device_alarm"."update_by" IS '更新者';
COMMENT ON COLUMN "caps_device_alarm"."update_time" IS '更新时间';
COMMENT ON COLUMN "caps_device_alarm"."remark" IS '备注';

COMMENT ON TABLE "caps_device_report" IS '安全帽设备实时数据';
COMMENT ON COLUMN "caps_device_report"."id" IS 'ID';
COMMENT ON COLUMN "caps_device_report"."create_time" IS '创建时间';
COMMENT ON COLUMN "caps_device_report"."device_id" IS '设备id';
COMMENT ON COLUMN "caps_device_report"."altitude" IS '相对开机时高度-米';
COMMENT ON COLUMN "caps_device_report"."bat_l" IS '当前电量百分比';
COMMENT ON COLUMN "caps_device_report"."bat_v" IS '当前电压';
COMMENT ON COLUMN "caps_device_report"."bt_signal_info" IS '蓝牙设备信息';
COMMENT ON COLUMN "caps_device_report"."c_speed" IS '速度';
COMMENT ON COLUMN "caps_device_report"."charging" IS '充电状态 0-正在充电 1-没有充电';
COMMENT ON COLUMN "caps_device_report"."gps" IS '定位类型 0-基站定位 1-gps 2-wifi X-其他';
COMMENT ON COLUMN "caps_device_report"."gps_level" IS 'gps信号 0-弱 1-强 -1 无信号';
COMMENT ON COLUMN "caps_device_report"."in_use" IS '帽子状态 1-在线 2-监控 3-视频通话';
COMMENT ON COLUMN "caps_device_report"."is_weared" IS '穿戴状态 默认0 1-未佩戴 2-已佩戴';
COMMENT ON COLUMN "caps_device_report"."net_strenth" IS '网络信号强度 0-5 5最强';
COMMENT ON COLUMN "caps_device_report"."net_type" IS '网络类型 1-4G 2-wifi';
COMMENT ON COLUMN "caps_device_report"."phoneNumber" IS '当前sim卡号';
COMMENT ON COLUMN "caps_device_report"."sim_data_num" IS '当月已用流量 MB';
COMMENT ON COLUMN "caps_device_report"."sim_status" IS 'sim卡监测 0未知 1-有卡 2-无卡';
COMMENT ON COLUMN "caps_device_report"."tcard_status" IS '内存卡监测 0位置 1有卡 2无卡';
COMMENT ON COLUMN "caps_device_report"."tempdata" IS '主板温度 - 摄氏度';
COMMENT ON COLUMN "caps_device_report"."user_id" IS '用户id';
COMMENT ON COLUMN "caps_device_report"."x_point" IS '纬度-高德';
COMMENT ON COLUMN "caps_device_report"."y_point" IS '经度-高德';
COMMENT ON COLUMN "caps_device_report"."create_by" IS '创建者';
COMMENT ON COLUMN "caps_device_report"."update_by" IS '更新者';
COMMENT ON COLUMN "caps_device_report"."update_time" IS '更新时间';
COMMENT ON COLUMN "caps_device_report"."remark" IS '备注';
COMMENT ON COLUMN "caps_device_report"."ch4" IS 'ch4';
COMMENT ON COLUMN "caps_device_report"."o2" IS 'o2';
COMMENT ON COLUMN "caps_device_report"."co" IS 'co';
COMMENT ON COLUMN "caps_device_report"."h2s" IS 'h2s';

COMMENT ON TABLE "gen_table" IS '代码生成业务表';
COMMENT ON COLUMN "gen_table"."table_id" IS '编号';
COMMENT ON COLUMN "gen_table"."table_name" IS '表名称';
COMMENT ON COLUMN "gen_table"."table_comment" IS '表描述';
COMMENT ON COLUMN "gen_table"."sub_table_name" IS '关联子表的表名';
COMMENT ON COLUMN "gen_table"."sub_table_fk_name" IS '子表关联的外键名';
COMMENT ON COLUMN "gen_table"."class_name" IS '实体类名称';
COMMENT ON COLUMN "gen_table"."tpl_category" IS '使用的模板（crud单表操作 tree树表操作）';
COMMENT ON COLUMN "gen_table"."tpl_web_type" IS '前端模板类型（element-ui模版 element-plus模版）';
COMMENT ON COLUMN "gen_table"."package_name" IS '生成包路径';
COMMENT ON COLUMN "gen_table"."module_name" IS '生成模块名';
COMMENT ON COLUMN "gen_table"."business_name" IS '生成业务名';
COMMENT ON COLUMN "gen_table"."function_name" IS '生成功能名';
COMMENT ON COLUMN "gen_table"."function_author" IS '生成功能作者';
COMMENT ON COLUMN "gen_table"."gen_type" IS '生成代码方式（0zip压缩包 1自定义路径）';
COMMENT ON COLUMN "gen_table"."gen_path" IS '生成路径（不填默认项目路径）';
COMMENT ON COLUMN "gen_table"."options" IS '其它生成选项';
COMMENT ON COLUMN "gen_table"."create_by" IS '创建者';
COMMENT ON COLUMN "gen_table"."create_time" IS '创建时间';
COMMENT ON COLUMN "gen_table"."update_by" IS '更新者';
COMMENT ON COLUMN "gen_table"."update_time" IS '更新时间';
COMMENT ON COLUMN "gen_table"."remark" IS '备注';

COMMENT ON TABLE "gen_table_column" IS '代码生成业务表字段';
COMMENT ON COLUMN "gen_table_column"."column_id" IS '编号';
COMMENT ON COLUMN "gen_table_column"."table_id" IS '归属表编号';
COMMENT ON COLUMN "gen_table_column"."column_name" IS '列名称';
COMMENT ON COLUMN "gen_table_column"."column_comment" IS '列描述';
COMMENT ON COLUMN "gen_table_column"."column_type" IS '列类型';
COMMENT ON COLUMN "gen_table_column"."java_type" IS 'JAVA类型';
COMMENT ON COLUMN "gen_table_column"."java_field" IS 'JAVA字段名';
COMMENT ON COLUMN "gen_table_column"."is_pk" IS '是否主键（1是）';
COMMENT ON COLUMN "gen_table_column"."is_increment" IS '是否自增（1是）';
COMMENT ON COLUMN "gen_table_column"."is_required" IS '是否必填（1是）';
COMMENT ON COLUMN "gen_table_column"."is_insert" IS '是否为插入字段（1是）';
COMMENT ON COLUMN "gen_table_column"."is_edit" IS '是否编辑字段（1是）';
COMMENT ON COLUMN "gen_table_column"."is_list" IS '是否列表字段（1是）';
COMMENT ON COLUMN "gen_table_column"."is_query" IS '是否查询字段（1是）';
COMMENT ON COLUMN "gen_table_column"."query_type" IS '查询方式（等于、不等于、大于、小于、范围）';
COMMENT ON COLUMN "gen_table_column"."html_type" IS '显示类型（文本框、文本域、下拉框、复选框、单选框、日期控件）';
COMMENT ON COLUMN "gen_table_column"."dict_type" IS '字典类型';
COMMENT ON COLUMN "gen_table_column"."sort" IS '排序';
COMMENT ON COLUMN "gen_table_column"."create_by" IS '创建者';
COMMENT ON COLUMN "gen_table_column"."create_time" IS '创建时间';
COMMENT ON COLUMN "gen_table_column"."update_by" IS '更新者';
COMMENT ON COLUMN "gen_table_column"."update_time" IS '更新时间';

COMMENT ON TABLE "qrtz_blob_triggers" IS 'Blob类型的触发器表';
COMMENT ON COLUMN "qrtz_blob_triggers"."sched_name" IS '调度名称';
COMMENT ON COLUMN "qrtz_blob_triggers"."trigger_name" IS 'qrtz_triggers表trigger_name的外键';
COMMENT ON COLUMN "qrtz_blob_triggers"."trigger_group" IS 'qrtz_triggers表trigger_group的外键';
COMMENT ON COLUMN "qrtz_blob_triggers"."blob_data" IS '存放持久化Trigger对象';

COMMENT ON TABLE "qrtz_calendars" IS '日历信息表';
COMMENT ON COLUMN "qrtz_calendars"."sched_name" IS '调度名称';
COMMENT ON COLUMN "qrtz_calendars"."calendar_name" IS '日历名称';
COMMENT ON COLUMN "qrtz_calendars"."calendar" IS '存放持久化calendar对象';

COMMENT ON TABLE "qrtz_cron_triggers" IS 'Cron类型的触发器表';
COMMENT ON COLUMN "qrtz_cron_triggers"."sched_name" IS '调度名称';
COMMENT ON COLUMN "qrtz_cron_triggers"."trigger_name" IS 'qrtz_triggers表trigger_name的外键';
COMMENT ON COLUMN "qrtz_cron_triggers"."trigger_group" IS 'qrtz_triggers表trigger_group的外键';
COMMENT ON COLUMN "qrtz_cron_triggers"."cron_expression" IS 'cron表达式';
COMMENT ON COLUMN "qrtz_cron_triggers"."time_zone_id" IS '时区';

COMMENT ON TABLE "qrtz_fired_triggers" IS '已触发的触发器表';
COMMENT ON COLUMN "qrtz_fired_triggers"."sched_name" IS '调度名称';
COMMENT ON COLUMN "qrtz_fired_triggers"."entry_id" IS '调度器实例id';
COMMENT ON COLUMN "qrtz_fired_triggers"."trigger_name" IS 'qrtz_triggers表trigger_name的外键';
COMMENT ON COLUMN "qrtz_fired_triggers"."trigger_group" IS 'qrtz_triggers表trigger_group的外键';
COMMENT ON COLUMN "qrtz_fired_triggers"."instance_name" IS '调度器实例名';
COMMENT ON COLUMN "qrtz_fired_triggers"."fired_time" IS '触发的时间';
COMMENT ON COLUMN "qrtz_fired_triggers"."sched_time" IS '定时器制定的时间';
COMMENT ON COLUMN "qrtz_fired_triggers"."priority" IS '优先级';
COMMENT ON COLUMN "qrtz_fired_triggers"."state" IS '状态';
COMMENT ON COLUMN "qrtz_fired_triggers"."job_name" IS '任务名称';
COMMENT ON COLUMN "qrtz_fired_triggers"."job_group" IS '任务组名';
COMMENT ON COLUMN "qrtz_fired_triggers"."is_nonconcurrent" IS '是否并发';
COMMENT ON COLUMN "qrtz_fired_triggers"."requests_recovery" IS '是否接受恢复执行';

COMMENT ON TABLE "qrtz_job_details" IS '任务详细信息表';
COMMENT ON COLUMN "qrtz_job_details"."sched_name" IS '调度名称';
COMMENT ON COLUMN "qrtz_job_details"."job_name" IS '任务名称';
COMMENT ON COLUMN "qrtz_job_details"."job_group" IS '任务组名';
COMMENT ON COLUMN "qrtz_job_details"."description" IS '相关介绍';
COMMENT ON COLUMN "qrtz_job_details"."job_class_name" IS '执行任务类名称';
COMMENT ON COLUMN "qrtz_job_details"."is_durable" IS '是否持久化';
COMMENT ON COLUMN "qrtz_job_details"."is_nonconcurrent" IS '是否并发';
COMMENT ON COLUMN "qrtz_job_details"."is_update_data" IS '是否更新数据';
COMMENT ON COLUMN "qrtz_job_details"."requests_recovery" IS '是否接受恢复执行';
COMMENT ON COLUMN "qrtz_job_details"."job_data" IS '存放持久化job对象';

COMMENT ON TABLE "qrtz_locks" IS '存储的悲观锁信息表';
COMMENT ON COLUMN "qrtz_locks"."sched_name" IS '调度名称';
COMMENT ON COLUMN "qrtz_locks"."lock_name" IS '悲观锁名称';

COMMENT ON TABLE "qrtz_paused_trigger_grps" IS '暂停的触发器表';
COMMENT ON COLUMN "qrtz_paused_trigger_grps"."sched_name" IS '调度名称';
COMMENT ON COLUMN "qrtz_paused_trigger_grps"."trigger_group" IS 'qrtz_triggers表trigger_group的外键';

COMMENT ON TABLE "qrtz_scheduler_state" IS '调度器状态表';
COMMENT ON COLUMN "qrtz_scheduler_state"."sched_name" IS '调度名称';
COMMENT ON COLUMN "qrtz_scheduler_state"."instance_name" IS '实例名称';
COMMENT ON COLUMN "qrtz_scheduler_state"."last_checkin_time" IS '上次检查时间';
COMMENT ON COLUMN "qrtz_scheduler_state"."checkin_interval" IS '检查间隔时间';

COMMENT ON TABLE "qrtz_simple_triggers" IS '简单触发器的信息表';
COMMENT ON COLUMN "qrtz_simple_triggers"."sched_name" IS '调度名称';
COMMENT ON COLUMN "qrtz_simple_triggers"."trigger_name" IS 'qrtz_triggers表trigger_name的外键';
COMMENT ON COLUMN "qrtz_simple_triggers"."trigger_group" IS 'qrtz_triggers表trigger_group的外键';
COMMENT ON COLUMN "qrtz_simple_triggers"."repeat_count" IS '重复的次数统计';
COMMENT ON COLUMN "qrtz_simple_triggers"."repeat_interval" IS '重复的间隔时间';
COMMENT ON COLUMN "qrtz_simple_triggers"."times_triggered" IS '已经触发的次数';

COMMENT ON TABLE "qrtz_simprop_triggers" IS '同步机制的行锁表';
COMMENT ON COLUMN "qrtz_simprop_triggers"."sched_name" IS '调度名称';
COMMENT ON COLUMN "qrtz_simprop_triggers"."trigger_name" IS 'qrtz_triggers表trigger_name的外键';
COMMENT ON COLUMN "qrtz_simprop_triggers"."trigger_group" IS 'qrtz_triggers表trigger_group的外键';
COMMENT ON COLUMN "qrtz_simprop_triggers"."str_prop_1" IS 'String类型的trigger的第一个参数';
COMMENT ON COLUMN "qrtz_simprop_triggers"."str_prop_2" IS 'String类型的trigger的第二个参数';
COMMENT ON COLUMN "qrtz_simprop_triggers"."str_prop_3" IS 'String类型的trigger的第三个参数';
COMMENT ON COLUMN "qrtz_simprop_triggers"."int_prop_1" IS 'int类型的trigger的第一个参数';
COMMENT ON COLUMN "qrtz_simprop_triggers"."int_prop_2" IS 'int类型的trigger的第二个参数';
COMMENT ON COLUMN "qrtz_simprop_triggers"."long_prop_1" IS 'long类型的trigger的第一个参数';
COMMENT ON COLUMN "qrtz_simprop_triggers"."long_prop_2" IS 'long类型的trigger的第二个参数';
COMMENT ON COLUMN "qrtz_simprop_triggers"."dec_prop_1" IS 'decimal类型的trigger的第一个参数';
COMMENT ON COLUMN "qrtz_simprop_triggers"."dec_prop_2" IS 'decimal类型的trigger的第二个参数';
COMMENT ON COLUMN "qrtz_simprop_triggers"."bool_prop_1" IS 'Boolean类型的trigger的第一个参数';
COMMENT ON COLUMN "qrtz_simprop_triggers"."bool_prop_2" IS 'Boolean类型的trigger的第二个参数';

COMMENT ON TABLE "qrtz_triggers" IS '触发器详细信息表';
COMMENT ON COLUMN "qrtz_triggers"."sched_name" IS '调度名称';
COMMENT ON COLUMN "qrtz_triggers"."trigger_name" IS '触发器的名字';
COMMENT ON COLUMN "qrtz_triggers"."trigger_group" IS '触发器所属组的名字';
COMMENT ON COLUMN "qrtz_triggers"."job_name" IS 'qrtz_job_details表job_name的外键';
COMMENT ON COLUMN "qrtz_triggers"."job_group" IS 'qrtz_job_details表job_group的外键';
COMMENT ON COLUMN "qrtz_triggers"."description" IS '相关介绍';
COMMENT ON COLUMN "qrtz_triggers"."next_fire_time" IS '上一次触发时间（毫秒）';
COMMENT ON COLUMN "qrtz_triggers"."prev_fire_time" IS '下一次触发时间（默认为-1表示不触发）';
COMMENT ON COLUMN "qrtz_triggers"."priority" IS '优先级';
COMMENT ON COLUMN "qrtz_triggers"."trigger_state" IS '触发器状态';
COMMENT ON COLUMN "qrtz_triggers"."trigger_type" IS '触发器的类型';
COMMENT ON COLUMN "qrtz_triggers"."start_time" IS '开始时间';
COMMENT ON COLUMN "qrtz_triggers"."end_time" IS '结束时间';
COMMENT ON COLUMN "qrtz_triggers"."calendar_name" IS '日程表名称';
COMMENT ON COLUMN "qrtz_triggers"."misfire_instr" IS '补偿执行的策略';
COMMENT ON COLUMN "qrtz_triggers"."job_data" IS '存放持久化job对象';

COMMENT ON TABLE "sys_config" IS '参数配置表';
COMMENT ON COLUMN "sys_config"."config_id" IS '参数主键';
COMMENT ON COLUMN "sys_config"."config_name" IS '参数名称';
COMMENT ON COLUMN "sys_config"."config_key" IS '参数键名';
COMMENT ON COLUMN "sys_config"."config_value" IS '参数键值';
COMMENT ON COLUMN "sys_config"."config_type" IS '系统内置（Y是 N否）';
COMMENT ON COLUMN "sys_config"."create_by" IS '创建者';
COMMENT ON COLUMN "sys_config"."create_time" IS '创建时间';
COMMENT ON COLUMN "sys_config"."update_by" IS '更新者';
COMMENT ON COLUMN "sys_config"."update_time" IS '更新时间';
COMMENT ON COLUMN "sys_config"."remark" IS '备注';

COMMENT ON TABLE "sys_dept" IS '部门表';
COMMENT ON COLUMN "sys_dept"."dept_id" IS '部门id';
COMMENT ON COLUMN "sys_dept"."parent_id" IS '父部门id';
COMMENT ON COLUMN "sys_dept"."ancestors" IS '祖级列表';
COMMENT ON COLUMN "sys_dept"."dept_name" IS '部门名称';
COMMENT ON COLUMN "sys_dept"."order_num" IS '显示顺序';
COMMENT ON COLUMN "sys_dept"."leader" IS '负责人';
COMMENT ON COLUMN "sys_dept"."phone" IS '联系电话';
COMMENT ON COLUMN "sys_dept"."email" IS '邮箱';
COMMENT ON COLUMN "sys_dept"."status" IS '部门状态（0正常 1停用）';
COMMENT ON COLUMN "sys_dept"."del_flag" IS '删除标志（0代表存在 2代表删除）';
COMMENT ON COLUMN "sys_dept"."create_by" IS '创建者';
COMMENT ON COLUMN "sys_dept"."create_time" IS '创建时间';
COMMENT ON COLUMN "sys_dept"."update_by" IS '更新者';
COMMENT ON COLUMN "sys_dept"."update_time" IS '更新时间';

COMMENT ON TABLE "sys_dict_data" IS '字典数据表';
COMMENT ON COLUMN "sys_dict_data"."dict_code" IS '字典编码';
COMMENT ON COLUMN "sys_dict_data"."dict_sort" IS '字典排序';
COMMENT ON COLUMN "sys_dict_data"."dict_label" IS '字典标签';
COMMENT ON COLUMN "sys_dict_data"."dict_value" IS '字典键值';
COMMENT ON COLUMN "sys_dict_data"."dict_type" IS '字典类型';
COMMENT ON COLUMN "sys_dict_data"."css_class" IS '样式属性（其他样式扩展）';
COMMENT ON COLUMN "sys_dict_data"."list_class" IS '表格回显样式';
COMMENT ON COLUMN "sys_dict_data"."is_default" IS '是否默认（Y是 N否）';
COMMENT ON COLUMN "sys_dict_data"."status" IS '状态（0正常 1停用）';
COMMENT ON COLUMN "sys_dict_data"."create_by" IS '创建者';
COMMENT ON COLUMN "sys_dict_data"."create_time" IS '创建时间';
COMMENT ON COLUMN "sys_dict_data"."update_by" IS '更新者';
COMMENT ON COLUMN "sys_dict_data"."update_time" IS '更新时间';
COMMENT ON COLUMN "sys_dict_data"."remark" IS '备注';

COMMENT ON TABLE "sys_dict_type" IS '字典类型表';
COMMENT ON COLUMN "sys_dict_type"."dict_id" IS '字典主键';
COMMENT ON COLUMN "sys_dict_type"."dict_name" IS '字典名称';
COMMENT ON COLUMN "sys_dict_type"."dict_type" IS '字典类型';
COMMENT ON COLUMN "sys_dict_type"."status" IS '状态（0正常 1停用）';
COMMENT ON COLUMN "sys_dict_type"."create_by" IS '创建者';
COMMENT ON COLUMN "sys_dict_type"."create_time" IS '创建时间';
COMMENT ON COLUMN "sys_dict_type"."update_by" IS '更新者';
COMMENT ON COLUMN "sys_dict_type"."update_time" IS '更新时间';
COMMENT ON COLUMN "sys_dict_type"."remark" IS '备注';

COMMENT ON TABLE "sys_job" IS '定时任务调度表';
COMMENT ON COLUMN "sys_job"."job_id" IS '任务ID';
COMMENT ON COLUMN "sys_job"."job_name" IS '任务名称';
COMMENT ON COLUMN "sys_job"."job_group" IS '任务组名';
COMMENT ON COLUMN "sys_job"."invoke_target" IS '调用目标字符串';
COMMENT ON COLUMN "sys_job"."cron_expression" IS 'cron执行表达式';
COMMENT ON COLUMN "sys_job"."misfire_policy" IS '计划执行错误策略（1立即执行 2执行一次 3放弃执行）';
COMMENT ON COLUMN "sys_job"."concurrent" IS '是否并发执行（0允许 1禁止）';
COMMENT ON COLUMN "sys_job"."status" IS '状态（0正常 1暂停）';
COMMENT ON COLUMN "sys_job"."create_by" IS '创建者';
COMMENT ON COLUMN "sys_job"."create_time" IS '创建时间';
COMMENT ON COLUMN "sys_job"."update_by" IS '更新者';
COMMENT ON COLUMN "sys_job"."update_time" IS '更新时间';
COMMENT ON COLUMN "sys_job"."remark" IS '备注信息';

COMMENT ON TABLE "sys_job_log" IS '定时任务调度日志表';
COMMENT ON COLUMN "sys_job_log"."job_log_id" IS '任务日志ID';
COMMENT ON COLUMN "sys_job_log"."job_name" IS '任务名称';
COMMENT ON COLUMN "sys_job_log"."job_group" IS '任务组名';
COMMENT ON COLUMN "sys_job_log"."invoke_target" IS '调用目标字符串';
COMMENT ON COLUMN "sys_job_log"."job_message" IS '日志信息';
COMMENT ON COLUMN "sys_job_log"."status" IS '执行状态（0正常 1失败）';
COMMENT ON COLUMN "sys_job_log"."exception_info" IS '异常信息';
COMMENT ON COLUMN "sys_job_log"."create_time" IS '创建时间';

COMMENT ON TABLE "sys_logininfor" IS '系统访问记录';
COMMENT ON COLUMN "sys_logininfor"."info_id" IS '访问ID';
COMMENT ON COLUMN "sys_logininfor"."user_name" IS '用户账号';
COMMENT ON COLUMN "sys_logininfor"."ipaddr" IS '登录IP地址';
COMMENT ON COLUMN "sys_logininfor"."login_location" IS '登录地点';
COMMENT ON COLUMN "sys_logininfor"."browser" IS '浏览器类型';
COMMENT ON COLUMN "sys_logininfor"."os" IS '操作系统';
COMMENT ON COLUMN "sys_logininfor"."status" IS '登录状态（0成功 1失败）';
COMMENT ON COLUMN "sys_logininfor"."msg" IS '提示消息';
COMMENT ON COLUMN "sys_logininfor"."login_time" IS '访问时间';

COMMENT ON TABLE "sys_menu" IS '菜单权限表';
COMMENT ON COLUMN "sys_menu"."menu_id" IS '菜单ID';
COMMENT ON COLUMN "sys_menu"."menu_name" IS '菜单名称';
COMMENT ON COLUMN "sys_menu"."parent_id" IS '父菜单ID';
COMMENT ON COLUMN "sys_menu"."order_num" IS '显示顺序';
COMMENT ON COLUMN "sys_menu"."path" IS '路由地址';
COMMENT ON COLUMN "sys_menu"."component" IS '组件路径';
COMMENT ON COLUMN "sys_menu"."query" IS '路由参数';
COMMENT ON COLUMN "sys_menu"."route_name" IS '路由名称';
COMMENT ON COLUMN "sys_menu"."is_frame" IS '是否为外链（0是 1否）';
COMMENT ON COLUMN "sys_menu"."is_cache" IS '是否缓存（0缓存 1不缓存）';
COMMENT ON COLUMN "sys_menu"."menu_type" IS '菜单类型（M目录 C菜单 F按钮）';
COMMENT ON COLUMN "sys_menu"."visible" IS '菜单状态（0显示 1隐藏）';
COMMENT ON COLUMN "sys_menu"."status" IS '菜单状态（0正常 1停用）';
COMMENT ON COLUMN "sys_menu"."perms" IS '权限标识';
COMMENT ON COLUMN "sys_menu"."icon" IS '菜单图标';
COMMENT ON COLUMN "sys_menu"."create_by" IS '创建者';
COMMENT ON COLUMN "sys_menu"."create_time" IS '创建时间';
COMMENT ON COLUMN "sys_menu"."update_by" IS '更新者';
COMMENT ON COLUMN "sys_menu"."update_time" IS '更新时间';
COMMENT ON COLUMN "sys_menu"."remark" IS '备注';

COMMENT ON TABLE "sys_notice" IS '通知公告表';
COMMENT ON COLUMN "sys_notice"."notice_id" IS '公告ID';
COMMENT ON COLUMN "sys_notice"."notice_title" IS '公告标题';
COMMENT ON COLUMN "sys_notice"."notice_type" IS '公告类型（1通知 2公告）';
COMMENT ON COLUMN "sys_notice"."notice_content" IS '公告内容';
COMMENT ON COLUMN "sys_notice"."status" IS '公告状态（0正常 1关闭）';
COMMENT ON COLUMN "sys_notice"."create_by" IS '创建者';
COMMENT ON COLUMN "sys_notice"."create_time" IS '创建时间';
COMMENT ON COLUMN "sys_notice"."update_by" IS '更新者';
COMMENT ON COLUMN "sys_notice"."update_time" IS '更新时间';
COMMENT ON COLUMN "sys_notice"."remark" IS '备注';

COMMENT ON TABLE "sys_oper_log" IS '操作日志记录';
COMMENT ON COLUMN "sys_oper_log"."oper_id" IS '日志主键';
COMMENT ON COLUMN "sys_oper_log"."title" IS '模块标题';
COMMENT ON COLUMN "sys_oper_log"."business_type" IS '业务类型（0其它 1新增 2修改 3删除）';
COMMENT ON COLUMN "sys_oper_log"."method" IS '方法名称';
COMMENT ON COLUMN "sys_oper_log"."request_method" IS '请求方式';
COMMENT ON COLUMN "sys_oper_log"."operator_type" IS '操作类别（0其它 1后台用户 2手机端用户）';
COMMENT ON COLUMN "sys_oper_log"."oper_name" IS '操作人员';
COMMENT ON COLUMN "sys_oper_log"."dept_name" IS '部门名称';
COMMENT ON COLUMN "sys_oper_log"."oper_url" IS '请求URL';
COMMENT ON COLUMN "sys_oper_log"."oper_ip" IS '主机地址';
COMMENT ON COLUMN "sys_oper_log"."oper_location" IS '操作地点';
COMMENT ON COLUMN "sys_oper_log"."oper_param" IS '请求参数';
COMMENT ON COLUMN "sys_oper_log"."json_result" IS '返回参数';
COMMENT ON COLUMN "sys_oper_log"."status" IS '操作状态（0正常 1异常）';
COMMENT ON COLUMN "sys_oper_log"."error_msg" IS '错误消息';
COMMENT ON COLUMN "sys_oper_log"."oper_time" IS '操作时间';
COMMENT ON COLUMN "sys_oper_log"."cost_time" IS '消耗时间';

COMMENT ON TABLE "sys_post" IS '岗位信息表';
COMMENT ON COLUMN "sys_post"."post_id" IS '岗位ID';
COMMENT ON COLUMN "sys_post"."post_code" IS '岗位编码';
COMMENT ON COLUMN "sys_post"."post_name" IS '岗位名称';
COMMENT ON COLUMN "sys_post"."post_sort" IS '显示顺序';
COMMENT ON COLUMN "sys_post"."status" IS '状态（0正常 1停用）';
COMMENT ON COLUMN "sys_post"."create_by" IS '创建者';
COMMENT ON COLUMN "sys_post"."create_time" IS '创建时间';
COMMENT ON COLUMN "sys_post"."update_by" IS '更新者';
COMMENT ON COLUMN "sys_post"."update_time" IS '更新时间';
COMMENT ON COLUMN "sys_post"."remark" IS '备注';

COMMENT ON TABLE "sys_role" IS '角色信息表';
COMMENT ON COLUMN "sys_role"."role_id" IS '角色ID';
COMMENT ON COLUMN "sys_role"."role_name" IS '角色名称';
COMMENT ON COLUMN "sys_role"."role_key" IS '角色权限字符串';
COMMENT ON COLUMN "sys_role"."role_sort" IS '显示顺序';
COMMENT ON COLUMN "sys_role"."data_scope" IS '数据范围（1：全部数据权限 2：自定数据权限 3：本部门数据权限 4：本部门及以下数据权限）';
COMMENT ON COLUMN "sys_role"."menu_check_strictly" IS '菜单树选择项是否关联显示';
COMMENT ON COLUMN "sys_role"."dept_check_strictly" IS '部门树选择项是否关联显示';
COMMENT ON COLUMN "sys_role"."status" IS '角色状态（0正常 1停用）';
COMMENT ON COLUMN "sys_role"."del_flag" IS '删除标志（0代表存在 2代表删除）';
COMMENT ON COLUMN "sys_role"."create_by" IS '创建者';
COMMENT ON COLUMN "sys_role"."create_time" IS '创建时间';
COMMENT ON COLUMN "sys_role"."update_by" IS '更新者';
COMMENT ON COLUMN "sys_role"."update_time" IS '更新时间';
COMMENT ON COLUMN "sys_role"."remark" IS '备注';

COMMENT ON TABLE "sys_role_dept" IS '角色和部门关联表';
COMMENT ON COLUMN "sys_role_dept"."role_id" IS '角色ID';
COMMENT ON COLUMN "sys_role_dept"."dept_id" IS '部门ID';

COMMENT ON TABLE "sys_role_menu" IS '角色和菜单关联表';
COMMENT ON COLUMN "sys_role_menu"."role_id" IS '角色ID';
COMMENT ON COLUMN "sys_role_menu"."menu_id" IS '菜单ID';

COMMENT ON TABLE "sys_user" IS '用户信息表';
COMMENT ON COLUMN "sys_user"."user_id" IS '用户ID';
COMMENT ON COLUMN "sys_user"."dept_id" IS '部门ID';
COMMENT ON COLUMN "sys_user"."user_name" IS '用户账号';
COMMENT ON COLUMN "sys_user"."nick_name" IS '用户昵称';
COMMENT ON COLUMN "sys_user"."user_type" IS '用户类型（00系统用户）';
COMMENT ON COLUMN "sys_user"."email" IS '用户邮箱';
COMMENT ON COLUMN "sys_user"."phonenumber" IS '手机号码';
COMMENT ON COLUMN "sys_user"."sex" IS '用户性别（0男 1女 2未知）';
COMMENT ON COLUMN "sys_user"."avatar" IS '头像地址';
COMMENT ON COLUMN "sys_user"."password" IS '密码';
COMMENT ON COLUMN "sys_user"."status" IS '帐号状态（0正常 1停用）';
COMMENT ON COLUMN "sys_user"."del_flag" IS '删除标志（0代表存在 2代表删除）';
COMMENT ON COLUMN "sys_user"."login_ip" IS '最后登录IP';
COMMENT ON COLUMN "sys_user"."login_date" IS '最后登录时间';
COMMENT ON COLUMN "sys_user"."create_by" IS '创建者';
COMMENT ON COLUMN "sys_user"."create_time" IS '创建时间';
COMMENT ON COLUMN "sys_user"."update_by" IS '更新者';
COMMENT ON COLUMN "sys_user"."update_time" IS '更新时间';
COMMENT ON COLUMN "sys_user"."remark" IS '备注';

COMMENT ON TABLE "sys_user_post" IS '用户与岗位关联表';
COMMENT ON COLUMN "sys_user_post"."user_id" IS '用户ID';
COMMENT ON COLUMN "sys_user_post"."post_id" IS '岗位ID';

COMMENT ON TABLE "sys_user_role" IS '用户和角色关联表';
COMMENT ON COLUMN "sys_user_role"."user_id" IS '用户ID';
COMMENT ON COLUMN "sys_user_role"."role_id" IS '角色ID';

-- ----------------------------
-- 3. 插入初始化数据


-- 启用 caps_device 表的自增列显式插入
SET IDENTITY_INSERT "caps_device" ON;

INSERT INTO "caps_device" ("id", "device_id", "person_id", "user_id", "user_name", "mobile", "create_by", "create_time", "update_by", "update_time", "remark") VALUES 
(3, '866652022496187', '', '3', '头箍式', '13026324515', '', NULL, '', NULL, NULL),
(1, '866652022038138', '002', '1', '测试', '13556263578', '', NULL, '', NULL, NULL),
(2, '866652022426580', '002', '2', '气体检测', '13026327533', '', NULL, '', NULL, NULL),
(4, '866652022399050', '11111111-1111-1111-1111-111111111111', '4', '中煤电气', '15556234555', '', NULL, '', NULL, NULL),
(5, '866652022498464', '11111111-1111-1111-1111-111111111111', '5', '智能安全帽-气体', '15556234555', '', NULL, '', NULL, NULL),
(6, '866652022498399', '11111111-1111-1111-1111-111111111111', '6', '智能安全帽-气体', '15556234555', '', NULL, '', NULL, NULL),
(7, '866652022498449', '11111111-1111-1111-1111-111111111111', '7', 'yxkj_002_01', NULL, '', NULL, '', NULL, NULL),
(8, '866652022498407', '11111111-1111-1111-1111-111111111111', '8', 'yxkj_002_02', NULL, '', NULL, '', NULL, NULL),
(9, '866652022498423', '11111111-1111-1111-1111-111111111111', '9', NULL, NULL, '', NULL, '', NULL, NULL),
(10, '866652022498415', '11111111-1111-1111-1111-111111111111', '10', NULL, NULL, '', NULL, '', NULL, NULL),
(11, '866652022498332', '11111111-1111-1111-1111-111111111111', '11', NULL, NULL, '', NULL, '', NULL, NULL),
(12, '866652022498431', '11111111-1111-1111-1111-111111111111', '12', NULL, NULL, '', NULL, '', NULL, NULL),
(13, '866652022589726', '11111111-1111-1111-1111-111111111111', '13', NULL, NULL, '', NULL, '', NULL, NULL),
(14, '866652022498340', '11111111-1111-1111-1111-111111111111', '14', '22222', NULL, '', NULL, '', NULL, NULL),
(15, '866652022591094', '16988537-1d72-446c-9ca7-7886f98ba2a4', '15', NULL, NULL, '', NULL, '', NULL, NULL),
(16, '866652022591052', '16988537-1d72-446c-9ca7-7886f98ba2a4', '16', '22222', NULL, '', NULL, '', NULL, NULL);

-- 关闭 caps_device 的自增列插入
SET IDENTITY_INSERT "caps_device" OFF;

-- 启用 audible_voice_alarm 表的自增列显式插入
SET IDENTITY_INSERT "audible_voice_alarm" ON;

INSERT INTO "audible_voice_alarm" ("id", "device_code", "person_id", "create_by", "create_time", "update_by", "update_time", "remark") VALUES 
(2, '09-250406008', '11111111-1111-1111-1111-111111111111', '', NULL, '', NULL, NULL);

-- 关闭 audible_voice_alarm 的自增列插入
SET IDENTITY_INSERT "audible_voice_alarm" OFF;