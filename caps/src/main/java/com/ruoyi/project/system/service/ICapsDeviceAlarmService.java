package com.ruoyi.project.system.service;

import java.util.List;
import com.ruoyi.project.system.domain.CapsDeviceAlarm;

/**
 * 安全帽设备告警信息Service接口
 * 
 * @author ruoyi
 * @date 2025-04-19
 */
public interface ICapsDeviceAlarmService 
{
    /**
     * 查询安全帽设备告警信息
     * 
     * @param id 安全帽设备告警信息主键
     * @return 安全帽设备告警信息
     */
    public CapsDeviceAlarm selectCapsDeviceAlarmById(Integer id);

    /**
     * 查询安全帽设备告警信息列表
     * 
     * @param capsDeviceAlarm 安全帽设备告警信息
     * @return 安全帽设备告警信息集合
     */
    public List<CapsDeviceAlarm> selectCapsDeviceAlarmList(CapsDeviceAlarm capsDeviceAlarm);

    /**
     * 新增安全帽设备告警信息
     * 
     * @param capsDeviceAlarm 安全帽设备告警信息
     * @return 结果
     */
    public int insertCapsDeviceAlarm(CapsDeviceAlarm capsDeviceAlarm);

    /**
     * 修改安全帽设备告警信息
     * 
     * @param capsDeviceAlarm 安全帽设备告警信息
     * @return 结果
     */
    public int updateCapsDeviceAlarm(CapsDeviceAlarm capsDeviceAlarm);

    /**
     * 批量删除安全帽设备告警信息
     * 
     * @param ids 需要删除的安全帽设备告警信息主键集合
     * @return 结果
     */
    public int deleteCapsDeviceAlarmByIds(Integer[] ids);

    /**
     * 删除安全帽设备告警信息信息
     * 
     * @param id 安全帽设备告警信息主键
     * @return 结果
     */
    public int deleteCapsDeviceAlarmById(Integer id);
}
