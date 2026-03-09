package com.ruoyi.common.enums;

/**
 * 安全帽报警类型
 */
public enum AlarmType {
    REMOVE_SIP_SOS_ALARM(0, "手动SOS紧急报警"),
    REMOVE_HAT_ALARM(1, "脱帽报警"),
    FENCE_ALARM_LEAVE_WORK_AREA(2, "围栏报警(离开工作区域)"),
    TEMPERATURE_ALARM(3, "温度报警"),
    FALL_ALARM(4, "跌落"),
    FENCE_ALARM_ENTER_DANGER_AREA(5, "围栏报警(进入危险区域)"),
    SILENT_ALARM(6, "静默报警"),
    SOS_EMERGENCY_ALARM(7, "SOS紧急报警"),
    ELECTRICITY_PROXIMITY_ALARM(11, "近电报警"),
    CLIMBING_ALARM_1(12, "登高报警"),
    OXYGEN_ALARM_1(13, "氧气浓度过低"),
    OXYGEN_ALARM_2(15, "氧气浓度过高"),
    ENTER_BLUETOOTH_BEACON_ALARM(17, "进入蓝牙信标报警"),
    LEAVE_BLUETOOTH_BEACON_ALARM(18, "离开蓝牙信标报警"),
    CARBON_MONOXIDE_ALARM(19, "一氧化碳报警"),
    HYDROGEN_SULFIDE_ALARM(23, "硫化氢报警"),
    CLIMBING_ALARM_2(24, "登高报警"),
    BENZENE_CONCENTRATION_EXCEEDED(41, "苯浓度超标");

    private final int code;
    private final String value;

    AlarmType(int code, String value) {
        this.code = code;
        this.value = value;
    }

    public int getCode() {
        return code;
    }

    public String getValue() {
        return value;
    }

    public static String getValueByCode(int code) {
        for (AlarmType alarmType : values()) {
            if (alarmType.code == code) {
                return alarmType.value;
            }
        }
        return null;
    }
}
