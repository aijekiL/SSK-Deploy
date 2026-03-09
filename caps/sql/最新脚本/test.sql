CREATE SCHEMA IF NOT EXISTS CAPS AUTHORIZATION SYSDBA;

-- 2. 切换到新创建的模式
-- 这相当于在 MySQL 中使用 USE `caps`
SET SCHEMA 'CAPS';

-- 导出  表 ry-zhdj.audible_voice_alarm 结构
CREATE TABLE IF NOT EXISTS "audible_voice_alarm" (
  "id" INT NOT NULL IDENTITY(1,1) COMMENT 'ID',
  "device_code" VARCHAR(125) NOT NULL COMMENT '设备编码',
  "person_id" VARCHAR(125) NOT NULL COMMENT '客户id',
  "create_by" VARCHAR(64) DEFAULT '' COMMENT '创建者',
  "create_time" DATETIME DEFAULT NULL COMMENT '创建时间',
  "update_by" VARCHAR(64) DEFAULT '' COMMENT '更新者',
  "update_time" DATETIME DEFAULT NULL COMMENT '更新时间',
  "remark" VARCHAR(255) DEFAULT NULL COMMENT '备注',
  PRIMARY KEY ("id")
) COMMENT '声光报警器设备信息表';

-- 导出  表 ry-zhdj.caps_device 结构
CREATE TABLE IF NOT EXISTS "caps_device" (
  "id" INT NOT NULL IDENTITY(1,1) COMMENT 'ID',
  "device_id" VARCHAR(125) NOT NULL COMMENT '设备id',
  "person_id" VARCHAR(125) NOT NULL COMMENT '客户id',
  "user_id" VARCHAR(125) NOT NULL COMMENT '用户id',
  "user_name" VARCHAR(125) DEFAULT NULL COMMENT '用户名',
  "mobile" VARCHAR(125) DEFAULT NULL COMMENT '手机号',
  "create_by" VARCHAR(64) DEFAULT '' COMMENT '创建者',
  "create_time" DATETIME DEFAULT NULL COMMENT '创建时间',
  "update_by" VARCHAR(64) DEFAULT '' COMMENT '更新者',
  "update_time" DATETIME DEFAULT NULL COMMENT '更新时间',
  "remark" VARCHAR(255) DEFAULT NULL COMMENT '备注',
  PRIMARY KEY ("id")
) COMMENT '安全帽设备信息表';

-- 导出  表 ry-zhdj.caps_device_alarm 结构
CREATE TABLE IF NOT EXISTS "caps_device_alarm" (
  "id" INT NOT NULL IDENTITY(1,1) COMMENT 'ID',
  "device_id" VARCHAR(125) DEFAULT NULL COMMENT '设备id',
  "type" INT DEFAULT 0 COMMENT '报警类型',
  "type_info" VARCHAR(125) DEFAULT NULL COMMENT '报警信息',
  "x_point" VARCHAR(125) DEFAULT NULL COMMENT '纬度',
  "y_point" VARCHAR(125) DEFAULT NULL COMMENT '经度',
  "create_by" VARCHAR(64) DEFAULT '' COMMENT '创建者',
  "create_time" DATETIME DEFAULT NULL COMMENT '创建时间',
  "update_by" VARCHAR(64) DEFAULT '' COMMENT '更新者',
  "update_time" DATETIME DEFAULT NULL COMMENT '更新时间',
  "remark" VARCHAR(255) DEFAULT NULL COMMENT '备注',
  PRIMARY KEY ("id")
) COMMENT '安全帽设备告警信息表';

-- 导出  表 ry-zhdj.caps_device_report 结构
CREATE TABLE IF NOT EXISTS "caps_device_report" (
  "id" INT NOT NULL IDENTITY(1,1) COMMENT 'ID',
  "create_time" DATETIME DEFAULT NULL COMMENT '创建时间',
  "device_id" VARCHAR(125) DEFAULT NULL COMMENT '设备id',
  "altitude" VARCHAR(25) DEFAULT NULL COMMENT '相对开机时高度-米',
  "bat_l" VARCHAR(25) DEFAULT NULL COMMENT '当前电量百分比',
  "bat_v" VARCHAR(25) DEFAULT NULL COMMENT '当前电压',
  "bt_signal_info" VARCHAR(125) DEFAULT NULL COMMENT '蓝牙设备信息',
  "c_speed" VARCHAR(25) DEFAULT NULL COMMENT '速度',
  "charging" VARCHAR(25) DEFAULT NULL COMMENT '充电状态 0-正在充电 1-没有充电',
  "gps" VARCHAR(25) DEFAULT NULL COMMENT '定位类型 0-基站定位 1-gps 2-wifi X-其他',
  "gps_level" VARCHAR(25) DEFAULT NULL COMMENT 'gps信号 0-弱 1-强 -1 无信号',
  "in_use" VARCHAR(25) DEFAULT NULL COMMENT '帽子状态 1-在线 2-监控 3-视频通话',
  "is_weared" VARCHAR(25) DEFAULT NULL COMMENT '穿戴状态 默认0 1-未佩戴 2-已佩戴',
  "net_strenth" VARCHAR(25) DEFAULT NULL COMMENT '网络信号强度 0-5 5最强',
  "net_type" VARCHAR(25) DEFAULT NULL COMMENT '网络类型 1-4G 2-wifi',
  "phoneNumber" VARCHAR(25) DEFAULT NULL COMMENT '当前sim卡号',
  "sim_data_num" VARCHAR(25) DEFAULT NULL COMMENT '当月已用流量 MB',
  "sim_status" VARCHAR(25) DEFAULT NULL COMMENT 'sim卡监测 0未知 1-有卡 2-无卡',
  "tcard_status" VARCHAR(25) DEFAULT NULL COMMENT '内存卡监测 0位置 1有卡 2无卡',
  "tempdata" VARCHAR(125) DEFAULT NULL COMMENT '主板温度 - 摄氏度',
  "user_id" VARCHAR(25) DEFAULT NULL COMMENT '用户id',
  "x_point" VARCHAR(25) DEFAULT NULL COMMENT '纬度-高德',
  "y_point" VARCHAR(25) DEFAULT NULL COMMENT '经度-高德',
  "create_by" VARCHAR(64) DEFAULT '' COMMENT '创建者',
  "update_by" VARCHAR(64) DEFAULT '' COMMENT '更新者',
  "update_time" DATETIME DEFAULT NULL COMMENT '更新时间',
  "remark" VARCHAR(255) DEFAULT NULL COMMENT '备注',
  "ch4" VARCHAR(100) DEFAULT NULL,
  "o2" VARCHAR(100) DEFAULT NULL,
  "co" VARCHAR(100) DEFAULT NULL,
  "h2s" VARCHAR(100) DEFAULT NULL,
  PRIMARY KEY ("id")
) COMMENT '安全帽设备实时数据';

-- 导出  表 ry-zhdj.gen_table 结构
CREATE TABLE IF NOT EXISTS "gen_table" (
  "table_id" BIGINT NOT NULL IDENTITY(1,1) COMMENT '编号',
  "table_name" VARCHAR(200) DEFAULT '' COMMENT '表名称',
  "table_comment" VARCHAR(500) DEFAULT '' COMMENT '表描述',
  "sub_table_name" VARCHAR(64) DEFAULT NULL COMMENT '关联子表的表名',
  "sub_table_fk_name" VARCHAR(64) DEFAULT NULL COMMENT '子表关联的外键名',
  "class_name" VARCHAR(100) DEFAULT '' COMMENT '实体类名称',
  "tpl_category" VARCHAR(200) DEFAULT 'crud' COMMENT '使用的模板（crud单表操作 tree树表操作）',
  "tpl_web_type" VARCHAR(30) DEFAULT '' COMMENT '前端模板类型（element-ui模版 element-plus模版）',
  "package_name" VARCHAR(100) DEFAULT NULL COMMENT '生成包路径',
  "module_name" VARCHAR(30) DEFAULT NULL COMMENT '生成模块名',
  "business_name" VARCHAR(30) DEFAULT NULL COMMENT '生成业务名',
  "function_name" VARCHAR(50) DEFAULT NULL COMMENT '生成功能名',
  "function_author" VARCHAR(50) DEFAULT NULL COMMENT '生成功能作者',
  "gen_type" CHAR(1) DEFAULT '0' COMMENT '生成代码方式（0zip压缩包 1自定义路径）',
  "gen_path" VARCHAR(200) DEFAULT '/' COMMENT '生成路径（不填默认项目路径）',
  "options" VARCHAR(1000) DEFAULT NULL COMMENT '其它生成选项',
  "create_by" VARCHAR(64) DEFAULT '' COMMENT '创建者',
  "create_time" DATETIME DEFAULT NULL COMMENT '创建时间',
  "update_by" VARCHAR(64) DEFAULT '' COMMENT '更新者',
  "update_time" DATETIME DEFAULT NULL COMMENT '更新时间',
  "remark" VARCHAR(500) DEFAULT NULL COMMENT '备注',
  PRIMARY KEY ("table_id")
) COMMENT '代码生成业务表';

-- 导出  表 ry-zhdj.gen_table_column 结构
CREATE TABLE IF NOT EXISTS "gen_table_column" (
  "column_id" BIGINT NOT NULL IDENTITY(1,1) COMMENT '编号',
  "table_id" BIGINT DEFAULT NULL COMMENT '归属表编号',
  "column_name" VARCHAR(200) DEFAULT NULL COMMENT '列名称',
  "column_comment" VARCHAR(500) DEFAULT NULL COMMENT '列描述',
  "column_type" VARCHAR(100) DEFAULT NULL COMMENT '列类型',
  "java_type" VARCHAR(500) DEFAULT NULL COMMENT 'JAVA类型',
  "java_field" VARCHAR(200) DEFAULT NULL COMMENT 'JAVA字段名',
  "is_pk" CHAR(1) DEFAULT NULL COMMENT '是否主键（1是）',
  "is_increment" CHAR(1) DEFAULT NULL COMMENT '是否自增（1是）',
  "is_required" CHAR(1) DEFAULT NULL COMMENT '是否必填（1是）',
  "is_insert" CHAR(1) DEFAULT NULL COMMENT '是否为插入字段（1是）',
  "is_edit" CHAR(1) DEFAULT NULL COMMENT '是否编辑字段（1是）',
  "is_list" CHAR(1) DEFAULT NULL COMMENT '是否列表字段（1是）',
  "is_query" CHAR(1) DEFAULT NULL COMMENT '是否查询字段（1是）',
  "query_type" VARCHAR(200) DEFAULT 'EQ' COMMENT '查询方式（等于、不等于、大于、小于、范围）',
  "html_type" VARCHAR(200) DEFAULT NULL COMMENT '显示类型（文本框、文本域、下拉框、复选框、单选框、日期控件）',
  "dict_type" VARCHAR(200) DEFAULT '' COMMENT '字典类型',
  "sort" INT DEFAULT NULL COMMENT '排序',
  "create_by" VARCHAR(64) DEFAULT '' COMMENT '创建者',
  "create_time" DATETIME DEFAULT NULL COMMENT '创建时间',
  "update_by" VARCHAR(64) DEFAULT '' COMMENT '更新者',
  "update_time" DATETIME DEFAULT NULL COMMENT '更新时间',
  PRIMARY KEY ("column_id")
) COMMENT '代码生成业务表字段';

-- 导出  表 ry-zhdj.sys_config 结构
CREATE TABLE IF NOT EXISTS "sys_config" (
  "config_id" INT NOT NULL IDENTITY(1,1) COMMENT '参数主键',
  "config_name" VARCHAR(100) DEFAULT '' COMMENT '参数名称',
  "config_key" VARCHAR(100) DEFAULT '' COMMENT '参数键名',
  "config_value" VARCHAR(500) DEFAULT '' COMMENT '参数键值',
  "config_type" CHAR(1) DEFAULT 'N' COMMENT '系统内置（Y是 N否）',
  "create_by" VARCHAR(64) DEFAULT '' COMMENT '创建者',
  "create_time" DATETIME DEFAULT NULL COMMENT '创建时间',
  "update_by" VARCHAR(64) DEFAULT '' COMMENT '更新者',
  "update_time" DATETIME DEFAULT NULL COMMENT '更新时间',
  "remark" VARCHAR(500) DEFAULT NULL COMMENT '备注',
  PRIMARY KEY ("config_id")
) COMMENT '参数配置表';

-- 导出  表 ry-zhdj.sys_dept 结构
CREATE TABLE IF NOT EXISTS "sys_dept" (
  "dept_id" BIGINT NOT NULL IDENTITY(1,1) COMMENT '部门id',
  "parent_id" BIGINT DEFAULT 0 COMMENT '父部门id',
  "ancestors" VARCHAR(50) DEFAULT '' COMMENT '祖级列表',
  "dept_name" VARCHAR(30) DEFAULT '' COMMENT '部门名称',
  "order_num" INT DEFAULT 0 COMMENT '显示顺序',
  "leader" VARCHAR(20) DEFAULT NULL COMMENT '负责人',
  "phone" VARCHAR(11) DEFAULT NULL COMMENT '联系电话',
  "email" VARCHAR(50) DEFAULT NULL COMMENT '邮箱',
  "status" CHAR(1) DEFAULT '0' COMMENT '部门状态（0正常 1停用）',
  "del_flag" CHAR(1) DEFAULT '0' COMMENT '删除标志（0代表存在 2代表删除）',
  "create_by" VARCHAR(64) DEFAULT '' COMMENT '创建者',
  "create_time" DATETIME DEFAULT NULL COMMENT '创建时间',
  "update_by" VARCHAR(64) DEFAULT '' COMMENT '更新者',
  "update_time" DATETIME DEFAULT NULL COMMENT '更新时间',
  PRIMARY KEY ("dept_id")
) COMMENT '部门表';

-- 导出  表 ry-zhdj.sys_dict_type 结构
CREATE TABLE IF NOT EXISTS "sys_dict_type" (
  "dict_id" BIGINT NOT NULL IDENTITY(1,1) COMMENT '字典主键',
  "dict_name" VARCHAR(100) DEFAULT '' COMMENT '字典名称',
  "dict_type" VARCHAR(100) DEFAULT '' COMMENT '字典类型',
  "status" CHAR(1) DEFAULT '0' COMMENT '状态（0正常 1停用）',
  "create_by" VARCHAR(64) DEFAULT '' COMMENT '创建者',
  "create_time" DATETIME DEFAULT NULL COMMENT '创建时间',
  "update_by" VARCHAR(64) DEFAULT '' COMMENT '更新者',
  "update_time" DATETIME DEFAULT NULL COMMENT '更新时间',
  "remark" VARCHAR(500) DEFAULT NULL COMMENT '备注',
  PRIMARY KEY ("dict_id"),
  UNIQUE KEY "uk_sys_dict_type_dict_type" ("dict_type")
) COMMENT '字典类型表';

-- 导出  表 ry-zhdj.sys_dict_data 结构
CREATE TABLE IF NOT EXISTS "sys_dict_data" (
  "dict_code" BIGINT NOT NULL IDENTITY(1,1) COMMENT '字典编码',
  "dict_sort" INT DEFAULT 0 COMMENT '字典排序',
  "dict_label" VARCHAR(100) DEFAULT '' COMMENT '字典标签',
  "dict_value" VARCHAR(100) DEFAULT '' COMMENT '字典键值',
  "dict_type" VARCHAR(100) DEFAULT '' COMMENT '字典类型',
  "css_class" VARCHAR(100) DEFAULT NULL COMMENT '样式属性（其他样式扩展）',
  "list_class" VARCHAR(100) DEFAULT NULL COMMENT '表格回显样式',
  "is_default" CHAR(1) DEFAULT 'N' COMMENT '是否默认（Y是 N否）',
  "status" CHAR(1) DEFAULT '0' COMMENT '状态（0正常 1停用）',
  "create_by" VARCHAR(64) DEFAULT '' COMMENT '创建者',
  "create_time" DATETIME DEFAULT NULL COMMENT '创建时间',
  "update_by" VARCHAR(64) DEFAULT '' COMMENT '更新者',
  "update_time" DATETIME DEFAULT NULL COMMENT '更新时间',
  PRIMARY KEY ("dict_code")
) COMMENT '字典数据表';

-- 导出  表 ry-zhdj.sys_job 结构
CREATE TABLE IF NOT EXISTS "sys_job" (
  "job_id" BIGINT NOT NULL IDENTITY(1,1) COMMENT '任务ID',
  "job_name" VARCHAR(64) NOT NULL DEFAULT '' COMMENT '任务名称',
  "job_group" VARCHAR(64) NOT NULL DEFAULT 'DEFAULT' COMMENT '任务组名',
  "invoke_target" VARCHAR(500) NOT NULL COMMENT '调用目标字符串',
  "cron_expression" VARCHAR(255) DEFAULT '' COMMENT 'cron执行表达式',
  "misfire_policy" VARCHAR(20) DEFAULT '3' COMMENT '计划执行错误策略（1立即执行 2执行一次 3放弃执行）',
  "concurrent" CHAR(1) DEFAULT '1' COMMENT '是否并发执行（0允许 1禁止）',
  "status" CHAR(1) DEFAULT '0' COMMENT '状态（0正常 1暂停）',
  "create_by" VARCHAR(64) DEFAULT '' COMMENT '创建者',
  "create_time" DATETIME DEFAULT NULL COMMENT '创建时间',
  "update_by" VARCHAR(64) DEFAULT '' COMMENT '更新者',
  "update_time" DATETIME DEFAULT NULL COMMENT '更新时间',
  "remark" VARCHAR(500) DEFAULT '' COMMENT '备注信息',
  PRIMARY KEY ("job_id", "job_name", "job_group")
) COMMENT '定时任务调度表';

-- 导出  表 ry-zhdj.sys_job_log 结构
CREATE TABLE IF NOT EXISTS "sys_job_log" (
  "job_log_id" BIGINT NOT NULL IDENTITY(1,1) COMMENT '任务日志ID',
  "job_name" VARCHAR(64) NOT NULL COMMENT '任务名称',
  "job_group" VARCHAR(64) NOT NULL COMMENT '任务组名',
  "invoke_target" VARCHAR(500) NOT NULL COMMENT '调用目标字符串',
  "job_message" VARCHAR(500) DEFAULT NULL COMMENT '日志信息',
  "status" CHAR(1) DEFAULT '0' COMMENT '执行状态（0正常 1失败）',
  "exception_info" VARCHAR(2000) DEFAULT '' COMMENT '异常信息',
  "create_time" DATETIME DEFAULT NULL COMMENT '创建时间',
  PRIMARY KEY ("job_log_id")
) COMMENT '定时任务调度日志表';

-- 导出  表 ry-zhdj.sys_logininfor 结构
CREATE TABLE IF NOT EXISTS "sys_logininfor" (
  "info_id" BIGINT NOT NULL IDENTITY(1,1) COMMENT '访问ID',
  "user_name" VARCHAR(50) DEFAULT '' COMMENT '用户账号',
  "ipaddr" VARCHAR(128) DEFAULT '' COMMENT '登录IP地址',
  "login_location" VARCHAR(255) DEFAULT '' COMMENT '登录地点',
  "browser" VARCHAR(50) DEFAULT '' COMMENT '浏览器类型',
  "os" VARCHAR(50) DEFAULT '' COMMENT '操作系统',
  "status" CHAR(1) DEFAULT '0' COMMENT '登录状态（0成功 1失败）',
  "msg" VARCHAR(255) DEFAULT '' COMMENT '提示消息',
  "login_time" DATETIME DEFAULT NULL COMMENT '访问时间',
  PRIMARY KEY ("info_id")
) COMMENT '系统访问记录';
CREATE INDEX "idx_sys_logininfor_s" ON "sys_logininfor" ("status");
CREATE INDEX "idx_sys_logininfor_lt" ON "sys_logininfor" ("login_time");

-- 导出  表 ry-zhdj.sys_menu 结构
CREATE TABLE IF NOT EXISTS "sys_menu" (
  "menu_id" BIGINT NOT NULL IDENTITY(1,1) COMMENT '菜单ID',
  "menu_name" VARCHAR(50) NOT NULL COMMENT '菜单名称',
  "parent_id" BIGINT DEFAULT 0 COMMENT '父菜单ID',
  "order_num" INT DEFAULT 0 COMMENT '显示顺序',
  "path" VARCHAR(200) DEFAULT '' COMMENT '路由地址',
  "component" VARCHAR(255) DEFAULT NULL COMMENT '组件路径',
  "query" VARCHAR(255) DEFAULT NULL COMMENT '路由参数',
  "route_name" VARCHAR(50) DEFAULT '' COMMENT '路由名称',
  "is_frame" INT DEFAULT 1 COMMENT '是否为外链（0是 1否）',
  "is_cache" INT DEFAULT 0 COMMENT '是否缓存（0缓存 1不缓存）',
  "menu_type" CHAR(1) DEFAULT '' COMMENT '菜单类型（M目录 C菜单 F按钮）',
  "visible" CHAR(1) DEFAULT '0' COMMENT '菜单状态（0显示 1隐藏）',
  "status" CHAR(1) DEFAULT '0' COMMENT '菜单状态（0正常 1停用）',
  "perms" VARCHAR(100) DEFAULT NULL COMMENT '权限标识',
  "icon" VARCHAR(100) DEFAULT '#' COMMENT '菜单图标',
  "create_by" VARCHAR(64) DEFAULT '' COMMENT '创建者',
  "create_time" DATETIME DEFAULT NULL COMMENT '创建时间',
  "update_by" VARCHAR(64) DEFAULT '' COMMENT '更新者',
  "update_time" DATETIME DEFAULT NULL COMMENT '更新时间',
  PRIMARY KEY ("menu_id")
) COMMENT '菜单权限表';

-- 导出  表 ry-zhdj.sys_notice 结构
CREATE TABLE IF NOT EXISTS "sys_notice" (
  "notice_id" INT NOT NULL IDENTITY(1,1) COMMENT '公告ID',
  "notice_title" VARCHAR(50) NOT NULL COMMENT '公告标题',
  "notice_type" CHAR(1) NOT NULL COMMENT '公告类型（1通知 2公告）',
  "notice_content" BLOB COMMENT '公告内容',
  "status" CHAR(1) DEFAULT '0' COMMENT '公告状态（0正常 1关闭）',
  "create_by" VARCHAR(64) DEFAULT '' COMMENT '创建者',
  "create_time" DATETIME DEFAULT NULL COMMENT '创建时间',
  "update_by" VARCHAR(64) DEFAULT '' COMMENT '更新者',
  "update_time" DATETIME DEFAULT NULL COMMENT '更新时间',
  PRIMARY KEY ("notice_id")
) COMMENT '通知公告表';

-- 导出  表 ry-zhdj.sys_oper_log 结构
CREATE TABLE IF NOT EXISTS "sys_oper_log" (
  "oper_id" BIGINT NOT NULL IDENTITY(1,1) COMMENT '日志主键',
  "title" VARCHAR(50) DEFAULT '' COMMENT '模块标题',
  "business_type" INT DEFAULT 0 COMMENT '业务类型（0其它 1新增 2修改 3删除）',
  "method" VARCHAR(200) DEFAULT '' COMMENT '方法名称',
  "request_method" VARCHAR(10) DEFAULT '' COMMENT '请求方式',
  "operator_type" INT DEFAULT 0 COMMENT '操作类别（0其它 1后台用户 2手机端用户）',
  "oper_name" VARCHAR(50) DEFAULT '' COMMENT '操作人员',
  "dept_name" VARCHAR(50) DEFAULT '' COMMENT '部门名称',
  "oper_url" VARCHAR(255) DEFAULT '' COMMENT '请求URL',
  "oper_ip" VARCHAR(128) DEFAULT '' COMMENT '主机地址',
  "oper_location" VARCHAR(255) DEFAULT '' COMMENT '操作地点',
  "oper_param" VARCHAR(2000) DEFAULT '' COMMENT '请求参数',
  "json_result" VARCHAR(2000) DEFAULT '' COMMENT '返回参数',
  "status" INT DEFAULT 0 COMMENT '操作状态（0正常 1异常）',
  "error_msg" VARCHAR(2000) DEFAULT '' COMMENT '错误消息',
  "oper_time" DATETIME DEFAULT NULL COMMENT '操作时间',
  "cost_time" BIGINT DEFAULT 0 COMMENT '消耗时间',
  PRIMARY KEY ("oper_id")
) COMMENT '操作日志记录';
CREATE INDEX "idx_sys_oper_log_bt" ON "sys_oper_log" ("business_type");
CREATE INDEX "idx_sys_oper_log_s" ON "sys_oper_log" ("status");
CREATE INDEX "idx_sys_oper_log_ot" ON "sys_oper_log" ("oper_time");

-- 导出  表 ry-zhdj.sys_post 结构
CREATE TABLE IF NOT EXISTS "sys_post" (
  "post_id" BIGINT NOT NULL IDENTITY(1,1) COMMENT '岗位ID',
  "post_code" VARCHAR(64) NOT NULL COMMENT '岗位编码',
  "post_name" VARCHAR(50) NOT NULL COMMENT '岗位名称',
  "post_sort" INT NOT NULL COMMENT '显示顺序',
  "status" CHAR(1) NOT NULL COMMENT '状态（0正常 1停用）',
  "create_by" VARCHAR(64) DEFAULT '' COMMENT '创建者',
  "create_time" DATETIME DEFAULT NULL COMMENT '创建时间',
  "update_by" VARCHAR(64) DEFAULT '' COMMENT '更新者',
  "update_time" DATETIME DEFAULT NULL COMMENT '更新时间',
  PRIMARY KEY ("post_id")
) COMMENT '岗位信息表';

-- 导出  表 ry-zhdj.sys_role 结构
CREATE TABLE IF NOT EXISTS "sys_role" (
  "role_id" BIGINT NOT NULL IDENTITY(1,1) COMMENT '角色ID',
  "role_name" VARCHAR(30) NOT NULL COMMENT '角色名称',
  "role_key" VARCHAR(100) NOT NULL COMMENT '角色权限字符串',
  "role_sort" INT NOT NULL COMMENT '显示顺序',
  "data_scope" CHAR(1) DEFAULT '1' COMMENT '数据范围（1：全部数据权限 2：自定数据权限 3：本部门数据权限 4：本部门及以下数据权限）',
  "menu_check_strictly" TINYINT DEFAULT 1 COMMENT '菜单树选择项是否关联显示',
  "dept_check_strictly" TINYINT DEFAULT 1 COMMENT '部门树选择项是否关联显示',
  "status" CHAR(1) NOT NULL COMMENT '角色状态（0正常 1停用）',
  "del_flag" CHAR(1) DEFAULT '0' COMMENT '删除标志（0代表存在 2代表删除）',
  "create_by" VARCHAR(64) DEFAULT '' COMMENT '创建者',
  "create_time" DATETIME DEFAULT NULL COMMENT '创建时间',
  "update_by" VARCHAR(64) DEFAULT '' COMMENT '更新者',
  "update_time" DATETIME DEFAULT NULL COMMENT '更新时间',
  PRIMARY KEY ("role_id")
) COMMENT '角色信息表';

-- 导出  表 ry-zhdj.sys_user 结构
CREATE TABLE IF NOT EXISTS "sys_user" (
  "user_id" BIGINT NOT NULL IDENTITY(1,1) COMMENT '用户ID',
  "dept_id" BIGINT DEFAULT NULL COMMENT '部门ID',
  "user_name" VARCHAR(30) NOT NULL COMMENT '用户账号',
  "nick_name" VARCHAR(30) NOT NULL COMMENT '用户昵称',
  "user_type" VARCHAR(2) DEFAULT '00' COMMENT '用户类型（00系统用户）',
  "email" VARCHAR(50) DEFAULT '' COMMENT '用户邮箱',
  "phonenumber" VARCHAR(11) DEFAULT '' COMMENT '手机号码',
  "sex" CHAR(1) DEFAULT '0' COMMENT '用户性别（0男 1女 2未知）',
  "avatar" VARCHAR(100) DEFAULT '' COMMENT '头像地址',
  "password" VARCHAR(100) DEFAULT '' COMMENT '密码',
  "status" CHAR(1) DEFAULT '0' COMMENT '帐号状态（0正常 1停用）',
  "del_flag" CHAR(1) DEFAULT '0' COMMENT '删除标志（0代表存在 2代表删除）',
  "login_ip" VARCHAR(128) DEFAULT '' COMMENT '最后登录IP',
  "login_date" DATETIME DEFAULT NULL COMMENT '最后登录时间',
  "create_by" VARCHAR(64) DEFAULT '' COMMENT '创建者',
  "create_time" DATETIME DEFAULT NULL COMMENT '创建时间',
  "update_by" VARCHAR(64) DEFAULT '' COMMENT '更新者',
  "update_time" DATETIME DEFAULT NULL COMMENT '更新时间',
  PRIMARY KEY ("user_id")
) COMMENT '用户信息表';

-- 导出  表 ry-zhdj.sys_role_dept 结构
CREATE TABLE IF NOT EXISTS "sys_role_dept" (
  "role_id" BIGINT NOT NULL COMMENT '角色ID',
  "dept_id" BIGINT NOT NULL COMMENT '部门ID',
  PRIMARY KEY ("role_id", "dept_id")
) COMMENT '角色和部门关联表';

-- 导出  表 ry-zhdj.sys_role_menu 结构
CREATE TABLE IF NOT EXISTS "sys_role_menu" (
  "role_id" BIGINT NOT NULL COMMENT '角色ID',
  "menu_id" BIGINT NOT NULL COMMENT '菜单ID',
  PRIMARY KEY ("role_id", "menu_id")
) COMMENT '角色和菜单关联表';

-- 导出  表 ry-zhdj.sys_user_post 结构
CREATE TABLE IF NOT EXISTS "sys_user_post" (
  "user_id" BIGINT NOT NULL COMMENT '用户ID',
  "post_id" BIGINT NOT NULL COMMENT '岗位ID',
  PRIMARY KEY ("user_id", "post_id")
) COMMENT '用户与岗位关联表';

-- 导出  表 ry-zhdj.sys_user_role 结构
CREATE TABLE IF NOT EXISTS "sys_user_role" (
  "user_id" BIGINT NOT NULL COMMENT '用户ID',
  "role_id" BIGINT NOT NULL COMMENT '角色ID',
  PRIMARY KEY ("user_id", "role_id")
) COMMENT '用户和角色关联表';

-- Quartz 相关表
-- 导出  表 ry-zhdj.qrtz_blob_triggers 结构
CREATE TABLE IF NOT EXISTS "qrtz_blob_triggers" (
  "sched_name" VARCHAR(120) NOT NULL COMMENT '调度名称',
  "trigger_name" VARCHAR(200) NOT NULL COMMENT 'qrtz_triggers表trigger_name的外键',
  "trigger_group" VARCHAR(200) NOT NULL COMMENT 'qrtz_triggers表trigger_group的外键',
  "blob_data" BLOB COMMENT '存放持久化Trigger对象',
  PRIMARY KEY ("sched_name", "trigger_name", "trigger_group")
) COMMENT 'Blob类型的触发器表';

-- 导出  表 ry-zhdj.qrtz_calendars 结构
CREATE TABLE IF NOT EXISTS "qrtz_calendars" (
  "sched_name" VARCHAR(120) NOT NULL COMMENT '调度名称',
  "calendar_name" VARCHAR(200) NOT NULL COMMENT '日历名称',
  "calendar" BLOB NOT NULL COMMENT '存放持久化calendar对象',
  PRIMARY KEY ("sched_name", "calendar_name")
) COMMENT '日历信息表';

-- 导出  表 ry-zhdj.qrtz_cron_triggers 结构
CREATE TABLE IF NOT EXISTS "qrtz_cron_triggers" (
  "sched_name" VARCHAR(120) NOT NULL COMMENT '调度名称',
  "trigger_name" VARCHAR(200) NOT NULL COMMENT 'qrtz_triggers表trigger_name的外键',
  "trigger_group" VARCHAR(200) NOT NULL COMMENT 'qrtz_triggers表trigger_group的外键',
  "cron_expression" VARCHAR(200) NOT NULL COMMENT 'cron表达式',
  "time_zone_id" VARCHAR(80) DEFAULT NULL COMMENT '时区',
  PRIMARY KEY ("sched_name", "trigger_name", "trigger_group")
) COMMENT 'Cron类型的触发器表';

-- 导出  表 ry-zhdj.qrtz_fired_triggers 结构
CREATE TABLE IF NOT EXISTS "qrtz_fired_triggers" (
  "sched_name" VARCHAR(120) NOT NULL COMMENT '调度名称',
  "entry_id" VARCHAR(95) NOT NULL COMMENT '调度器实例id',
  "trigger_name" VARCHAR(200) NOT NULL COMMENT 'qrtz_triggers表trigger_name的外键',
  "trigger_group" VARCHAR(200) NOT NULL COMMENT 'qrtz_triggers表trigger_group的外键',
  "instance_name" VARCHAR(200) NOT NULL COMMENT '调度器实例名',
  "fired_time" BIGINT NOT NULL COMMENT '触发的时间',
  "sched_time" BIGINT NOT NULL COMMENT '定时器制定的时间',
  "priority" INT NOT NULL COMMENT '优先级',
  "state" VARCHAR(16) NOT NULL COMMENT '状态',
  "job_name" VARCHAR(200) DEFAULT NULL COMMENT '任务名称',
  "job_group" VARCHAR(200) DEFAULT NULL COMMENT '任务组名',
  "is_nonconcurrent" VARCHAR(1) DEFAULT NULL COMMENT '是否并发',
  "requests_recovery" VARCHAR(1) DEFAULT NULL COMMENT '是否接受恢复执行',
  PRIMARY KEY ("sched_name", "entry_id")
) COMMENT '已触发的触发器表';

-- 导出  表 ry-zhdj.qrtz_job_details 结构
CREATE TABLE IF NOT EXISTS "qrtz_job_details" (
  "sched_name" VARCHAR(120) NOT NULL COMMENT '调度名称',
  "job_name" VARCHAR(200) NOT NULL COMMENT '任务名称',
  "job_group" VARCHAR(200) NOT NULL COMMENT '任务组名',
  "description" VARCHAR(250) DEFAULT NULL COMMENT '相关介绍',
  "job_class_name" VARCHAR(250) NOT NULL COMMENT '执行任务类名称',
  "is_durable" VARCHAR(1) NOT NULL COMMENT '是否持久化',
  "is_nonconcurrent" VARCHAR(1) NOT NULL COMMENT '是否并发',
  "is_update_data" VARCHAR(1) NOT NULL COMMENT '是否更新数据',
  "requests_recovery" VARCHAR(1) NOT NULL COMMENT '是否接受恢复执行',
  "job_data" BLOB COMMENT '存放持久化job对象',
  PRIMARY KEY ("sched_name", "job_name", "job_group")
) COMMENT '任务详细信息表';

-- 导出  表 ry-zhdj.qrtz_locks 结构
CREATE TABLE IF NOT EXISTS "qrtz_locks" (
  "sched_name" VARCHAR(120) NOT NULL COMMENT '调度名称',
  "lock_name" VARCHAR(40) NOT NULL COMMENT '悲观锁名称',
  PRIMARY KEY ("sched_name", "lock_name")
) COMMENT '存储的悲观锁信息表';

-- 导出  表 ry-zhdj.qrtz_paused_trigger_grps 结构
CREATE TABLE IF NOT EXISTS "qrtz_paused_trigger_grps" (
  "sched_name" VARCHAR(120) NOT NULL COMMENT '调度名称',
  "trigger_group" VARCHAR(200) NOT NULL COMMENT 'qrtz_triggers表trigger_group的外键',
  PRIMARY KEY ("sched_name", "trigger_group")
) COMMENT '暂停的触发器表';

-- 导出  表 ry-zhdj.qrtz_scheduler_state 结构
CREATE TABLE IF NOT EXISTS "qrtz_scheduler_state" (
  "sched_name" VARCHAR(120) NOT NULL COMMENT '调度名称',
  "instance_name" VARCHAR(200) NOT NULL COMMENT '实例名称',
  "last_checkin_time" BIGINT NOT NULL COMMENT '上次检查时间',
  "checkin_interval" BIGINT NOT NULL COMMENT '检查间隔时间',
  PRIMARY KEY ("sched_name", "instance_name")
) COMMENT '调度器状态表';

-- 导出  表 ry-zhdj.qrtz_simple_triggers 结构
CREATE TABLE IF NOT EXISTS "qrtz_simple_triggers" (
  "sched_name" VARCHAR(120) NOT NULL COMMENT '调度名称',
  "trigger_name" VARCHAR(200) NOT NULL COMMENT 'qrtz_triggers表trigger_name的外键',
  "trigger_group" VARCHAR(200) NOT NULL COMMENT 'qrtz_triggers表trigger_group的外键',
  "repeat_count" BIGINT NOT NULL COMMENT '重复的次数统计',
  "repeat_interval" BIGINT NOT NULL COMMENT '重复的间隔时间',
  "times_triggered" BIGINT NOT NULL COMMENT '已经触发的次数',
  PRIMARY KEY ("sched_name", "trigger_name", "trigger_group")
) COMMENT '简单触发器的信息表';

-- 导出  表 ry-zhdj.qrtz_simprop_triggers 结构
CREATE TABLE IF NOT EXISTS "qrtz_simprop_triggers" (
  "sched_name" VARCHAR(120) NOT NULL COMMENT '调度名称',
  "trigger_name" VARCHAR(200) NOT NULL COMMENT 'qrtz_triggers表trigger_name的外键',
  "trigger_group" VARCHAR(200) NOT NULL COMMENT 'qrtz_triggers表trigger_group的外键',
  "str_prop_1" VARCHAR(512) DEFAULT NULL COMMENT 'String类型的trigger的第一个参数',
  "str_prop_2" VARCHAR(512) DEFAULT NULL COMMENT 'String类型的trigger的第二个参数',
  "str_prop_3" VARCHAR(512) DEFAULT NULL COMMENT 'String类型的trigger的第三个参数',
  "int_prop_1" INT DEFAULT NULL COMMENT 'int类型的trigger的第一个参数',
  "int_prop_2" INT DEFAULT NULL COMMENT 'int类型的trigger的第二个参数',
  "long_prop_1" BIGINT DEFAULT NULL COMMENT 'long类型的trigger的第一个参数',
  "long_prop_2" BIGINT DEFAULT NULL COMMENT 'long类型的trigger的第二个参数',
  "dec_prop_1" DECIMAL(13,4) DEFAULT NULL COMMENT 'decimal类型的trigger的第一个参数',
  "dec_prop_2" DECIMAL(13,4) DEFAULT NULL COMMENT 'decimal类型的trigger的第二个参数',
  "bool_prop_1" VARCHAR(1) DEFAULT NULL COMMENT 'Boolean类型的trigger的第一个参数',
  "bool_prop_2" VARCHAR(1) DEFAULT NULL COMMENT 'Boolean类型的trigger的第二个参数',
  PRIMARY KEY ("sched_name", "trigger_name", "trigger_group")
) COMMENT '同步机制的行锁表';

-- 导出  表 ry-zhdj.qrtz_triggers 结构
CREATE TABLE IF NOT EXISTS "qrtz_triggers" (
  "sched_name" VARCHAR(120) NOT NULL COMMENT '调度名称',
  "trigger_name" VARCHAR(200) NOT NULL COMMENT '触发器的名字',
  "trigger_group" VARCHAR(200) NOT NULL COMMENT '触发器所属组的名字',
  "job_name" VARCHAR(200) NOT NULL COMMENT 'qrtz_job_details表job_name的外键',
  "job_group" VARCHAR(200) NOT NULL COMMENT 'qrtz_job_details表job_group的外键',
  "description" VARCHAR(250) DEFAULT NULL COMMENT '相关介绍',
  "next_fire_time" BIGINT DEFAULT NULL COMMENT '上一次触发时间（毫秒）',
  "prev_fire_time" BIGINT DEFAULT NULL COMMENT '下一次触发时间（默认为-1表示不触发）',
  "priority" INT DEFAULT NULL COMMENT '优先级',
  "trigger_state" VARCHAR(16) NOT NULL COMMENT '触发器状态',
  "trigger_type" VARCHAR(8) NOT NULL COMMENT '触发器的类型',
  "start_time" BIGINT NOT NULL COMMENT '开始时间',
  "end_time" BIGINT DEFAULT NULL COMMENT '结束时间',
  "calendar_name" VARCHAR(200) DEFAULT NULL COMMENT '日程表名称',
  "misfire_instr" SMALLINT DEFAULT NULL COMMENT '补偿执行的策略',
  "job_data" BLOB COMMENT '存放持久化job对象',
  PRIMARY KEY ("sched_name", "trigger_name", "trigger_group")
) COMMENT '触发器详细信息表';
CREATE INDEX "idx_qrtz_triggers_job" ON "qrtz_triggers" ("sched_name", "job_name", "job_group");


-- ==============================================================
-- 3. 创建外键约束 (在所有表创建完毕后)
-- ==============================================================
-- 注意：为了脚本执行的稳健性，外键约束在单独的ALTER TABLE语句中创建，并且放在所有表创建之后。
-- DM中创建外键时，如果引用的表或列不存在，会直接报错。

ALTER TABLE "qrtz_blob_triggers" ADD CONSTRAINT "fk_qrtz_blob_triggers_triggers" FOREIGN KEY ("sched_name", "trigger_name", "trigger_group") REFERENCES "qrtz_triggers" ("sched_name", "trigger_name", "trigger_group");
ALTER TABLE "qrtz_cron_triggers" ADD CONSTRAINT "fk_qrtz_cron_triggers_triggers" FOREIGN KEY ("sched_name", "trigger_name", "trigger_group") REFERENCES "qrtz_triggers" ("sched_name", "trigger_name", "trigger_group");
ALTER TABLE "qrtz_simple_triggers" ADD CONSTRAINT "fk_qrtz_simple_triggers_triggers" FOREIGN KEY ("sched_name", "trigger_name", "trigger_group") REFERENCES "qrtz_triggers" ("sched_name", "trigger_name", "trigger_group");
ALTER TABLE "qrtz_simprop_triggers" ADD CONSTRAINT "fk_qrtz_simprop_triggers_triggers" FOREIGN KEY ("sched_name", "trigger_name", "trigger_group") REFERENCES "qrtz_triggers" ("sched_name", "trigger_name", "trigger_group");
ALTER TABLE "qrtz_triggers" ADD CONSTRAINT "fk_qrtz_triggers_job_details" FOREIGN KEY ("sched_name", "job_name", "job_group") REFERENCES "qrtz_job_details" ("sched_name", "job_name", "job_group");

-- ==============================================================
-- 4. 插入初始数据
-- ==============================================================

-- 插入 caps_device 表数据
INSERT INTO "caps_device" ("id", "device_id", "person_id", "user_id", "user_name", "mobile", "create_by", "create_time", "update_by", "update_time", "remark") VALUES (3, '866652022496187', '', '3', '头箍式', '13026324515', '', NULL, '', NULL, NULL);
INSERT INTO "caps_device" ("id", "device_id", "person_id", "user_id", "user_name", "mobile", "create_by", "create_time", "update_by", "update_time", "remark") VALUES (1, '866652022038138', '002', '1', '测试', '13556263578', '', NULL, '', NULL, NULL);
INSERT INTO "caps_device" ("id", "device_id", "person_id", "user_id", "user_name", "mobile", "create_by", "create_time", "update_by", "update_time", "remark") VALUES (2, '866652022426580', '002', '2', '气体检测', '13026327533', '', NULL, '', NULL, NULL);
INSERT INTO "caps_device" ("id", "device_id", "person_id", "user_id", "user_name", "mobile", "create_by", "create_time", "update_by", "update_time", "remark") VALUES (4, '866652022399050', '11111111-1111-1111-1111-111111111111', '4', '中煤电气', '15556234555', '', NULL, '', NULL, NULL);
INSERT INTO "caps_device" ("id", "device_id", "person_id", "user_id", "user_name", "mobile", "create_by", "create_time", "update_by", "update_time", "remark") VALUES (5, '866652022498464', '11111111-1111-1111-1111-111111111111', '5', '智能安全帽-气体', '15556234555', '', NULL, '', NULL, NULL);
INSERT INTO "caps_device" ("id", "device_id", "person_id", "user_id", "user_name", "mobile", "create_by", "create_time", "update_by", "update_time", "remark") VALUES (6, '866652022498399', '11111111-1111-1111-1111-111111111111', '6', '智能安全帽-气体', '15556234555', '', NULL, '', NULL, NULL);
INSERT INTO "caps_device" ("id", "device_id", "person_id", "user_id", "user_name", "mobile", "create_by", "create_time", "update_by", "update_time", "remark") VALUES (7, '866652022498449', '11111111-1111-1111-1111-111111111111', '7', 'yxkj_002_01', NULL, '', NULL, '', NULL, NULL);
INSERT INTO "caps_device" ("id", "device_id", "person_id", "user_id", "user_name", "mobile", "create_by", "create_time", "update_by", "update_time", "remark") VALUES (8, '866652022498407', '11111111-1111-1111-1111-111111111111', '8', 'yxkj_002_02', NULL, '', NULL, '', NULL, NULL);
INSERT INTO "caps_device" ("id", "device_id", "person_id", "user_id", "user_name", "mobile", "create_by", "create_time", "update_by", "update_time", "remark") VALUES (9, '866652022498423', '11111111-1111-1111-1111-111111111111', '9', NULL, NULL, '', NULL, '', NULL, NULL);
INSERT INTO "caps_device" ("id", "device_id", "person_id", "user_id", "user_name", "mobile", "create_by", "create_time", "update_by", "update_time", "remark") VALUES (10, '866652022498415', '11111111-1111-1111-1111-111111111111', '10', NULL, NULL, '', NULL, '', NULL, NULL);
INSERT INTO "caps_device" ("id", "device_id", "person_id", "user_id", "user_name", "mobile", "create_by", "create_time", "update_by", "update_time", "remark") VALUES (11, '866652022498332', '11111111-1111-1111-1111-111111111111', '11', NULL, NULL, '', NULL, '', NULL, NULL);
INSERT INTO "caps_device" ("id", "device_id", "person_id", "user_id", "user_name", "mobile", "create_by", "create_time", "update_by", "update_time", "remark") VALUES (12, '866652022498431', '11111111-1111-1111-1111-111111111111', '12', NULL, NULL, '', NULL, '', NULL, NULL);
INSERT INTO "caps_device" ("id", "device_id", "person_id", "user_id", "user_name", "mobile", "create_by", "create_time", "update_by", "update_time", "remark") VALUES (13, '866652022589726', '11111111-1111-1111-1111-111111111111', '13', NULL, NULL, '', NULL, '', NULL, NULL);
INSERT INTO "caps_device" ("id", "device_id", "person_id", "user_id", "user_name", "mobile", "create_by", "create_time", "update_by", "update_time", "remark") VALUES (14, '866652022498340', '11111111-1111-1111-1111-111111111111', '14', '22222', NULL, '', NULL, '', NULL, NULL);
INSERT INTO "caps_device" ("id", "device_id", "person_id", "user_id", "user_name", "mobile", "create_by", "create_time", "update_by", "update_time", "remark") VALUES (15, '866652022591094', '16988537-1d72-446c-9ca7-7886f98ba2a4', '15', NULL, NULL, '', NULL, '', NULL, NULL);
INSERT INTO "caps_device" ("id", "device_id", "person_id", "user_id", "user_name", "mobile", "create_by", "create_time", "update_by", "update_time", "remark") VALUES (16, '866652022591052', '16988537-1d72-446c-9ca7-7886f98ba2a4', '16', '22222', NULL, '', NULL, '', NULL, NULL);

-- 插入 audible_voice_alarm 表数据
INSERT INTO "audible_voice_alarm" ("id", "device_code", "person_id", "create_by", "create_time", "update_by", "update_time", "remark") VALUES (2, '09-250406008', '11111111-1111-1111-1111-111111111111', '', NULL, '', NULL, NULL);


