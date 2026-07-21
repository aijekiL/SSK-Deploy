-- Safety first: back up caps_device_report before running this script.
-- This migration keeps caps_device_report as the latest-status table and
-- moves sampled/history records into caps_device_report_history.

CREATE TABLE IF NOT EXISTS `caps_device_report_history` (
  `id` int NOT NULL AUTO_INCREMENT COMMENT 'ID',
  `create_time` datetime DEFAULT NULL COMMENT 'create time',
  `device_id` varchar(125) DEFAULT NULL COMMENT 'device id',
  `altitude` varchar(25) DEFAULT NULL,
  `bat_l` varchar(25) DEFAULT NULL,
  `bat_v` varchar(25) DEFAULT NULL,
  `bt_signal_info` varchar(125) DEFAULT NULL,
  `c_speed` varchar(25) DEFAULT NULL,
  `charging` varchar(25) DEFAULT NULL,
  `gps` varchar(25) DEFAULT NULL,
  `gps_level` varchar(25) DEFAULT NULL,
  `in_use` varchar(25) DEFAULT NULL,
  `is_weared` varchar(25) DEFAULT NULL,
  `net_strenth` varchar(25) DEFAULT NULL,
  `net_type` varchar(25) DEFAULT NULL,
  `phoneNumber` varchar(25) DEFAULT NULL,
  `sim_data_num` varchar(25) DEFAULT NULL,
  `sim_status` varchar(25) DEFAULT NULL,
  `tcard_status` varchar(25) DEFAULT NULL,
  `tempdata` varchar(125) DEFAULT NULL,
  `user_id` varchar(25) DEFAULT NULL,
  `x_point` varchar(25) DEFAULT NULL,
  `y_point` varchar(25) DEFAULT NULL,
  `create_by` varchar(64) DEFAULT '',
  `update_by` varchar(64) DEFAULT '',
  `update_time` datetime DEFAULT NULL,
  `remark` varchar(255) DEFAULT NULL,
  `ch4` varchar(100) DEFAULT NULL,
  `o2` varchar(100) DEFAULT NULL,
  `co` varchar(100) DEFAULT NULL,
  `h2s` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_report_history_device_time` (`device_id`, `create_time`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 ROW_FORMAT=DYNAMIC COMMENT='safety helmet report history';

-- Optional one-time archive of existing report rows.
-- Run only once. If the history table already contains migrated data, skip it.
INSERT INTO `caps_device_report_history` (
  `create_time`, `device_id`, `altitude`, `bat_l`, `bat_v`, `bt_signal_info`,
  `c_speed`, `charging`, `gps`, `gps_level`, `in_use`, `is_weared`,
  `net_strenth`, `net_type`, `phoneNumber`, `sim_data_num`, `sim_status`,
  `tcard_status`, `tempdata`, `user_id`, `x_point`, `y_point`, `create_by`,
  `update_by`, `update_time`, `remark`, `ch4`, `o2`, `co`, `h2s`
)
SELECT
  `create_time`, `device_id`, `altitude`, `bat_l`, `bat_v`, `bt_signal_info`,
  `c_speed`, `charging`, `gps`, `gps_level`, `in_use`, `is_weared`,
  `net_strenth`, `net_type`, `phoneNumber`, `sim_data_num`, `sim_status`,
  `tcard_status`, `tempdata`, `user_id`, `x_point`, `y_point`, `create_by`,
  `update_by`, `update_time`, `remark`, `ch4`, `o2`, `co`, `h2s`
FROM `caps_device_report`;

-- Keep only one latest row per device in caps_device_report.
-- This leaves rows whose device_id is NULL untouched.
DELETE r
FROM `caps_device_report` r
JOIN (
  SELECT `device_id`, MAX(`id`) AS latest_id
  FROM `caps_device_report`
  WHERE `device_id` IS NOT NULL AND `device_id` <> ''
  GROUP BY `device_id`
) latest ON r.`device_id` = latest.`device_id`
WHERE r.`id` <> latest.`latest_id`;

-- Speed up latest-status lookups. If an index already exists, skip manually.
CREATE INDEX `idx_report_device_update_time`
ON `caps_device_report` (`device_id`, `update_time`, `create_time`);

-- Optional after the duplicate cleanup above:
-- ALTER TABLE `caps_device_report`
-- ADD UNIQUE KEY `uk_report_device_id` (`device_id`);
