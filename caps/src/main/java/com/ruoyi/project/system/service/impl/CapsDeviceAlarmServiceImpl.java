package com.ruoyi.project.system.service.impl;

import java.util.List;
import com.ruoyi.common.utils.DateUtils;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import com.ruoyi.project.system.mapper.CapsDeviceAlarmMapper;
import com.ruoyi.project.system.domain.CapsDeviceAlarm;
import com.ruoyi.project.system.service.ICapsDeviceAlarmService;

/**
 * 安全帽设备告警信息Service业务层处理
 * 
 * @author ruoyi
 * @date 2025-04-19
 */
@Service
public class CapsDeviceAlarmServiceImpl implements ICapsDeviceAlarmService 
{
    @Autowired
    private CapsDeviceAlarmMapper capsDeviceAlarmMapper;

    /**
     * 查询安全帽设备告警信息
     * 
     * @param id 安全帽设备告警信息主键
     * @return 安全帽设备告警信息
     */
    @Override
    public CapsDeviceAlarm selectCapsDeviceAlarmById(Integer id)
    {
        return capsDeviceAlarmMapper.selectCapsDeviceAlarmById(id);
    }

    /**
     * 查询安全帽设备告警信息列表
     * 
     * @param capsDeviceAlarm 安全帽设备告警信息
     * @return 安全帽设备告警信息
     */
    @Override
    public List<CapsDeviceAlarm> selectCapsDeviceAlarmList(CapsDeviceAlarm capsDeviceAlarm)
    {
        return capsDeviceAlarmMapper.selectCapsDeviceAlarmList(capsDeviceAlarm);
    }

    /**
     * 新增安全帽设备告警信息
     * 
     * @param capsDeviceAlarm 安全帽设备告警信息
     * @return 结果
     */
    @Override
    public int insertCapsDeviceAlarm(CapsDeviceAlarm capsDeviceAlarm)
    {
        capsDeviceAlarm.setCreateTime(DateUtils.getNowDate());
        return capsDeviceAlarmMapper.insertCapsDeviceAlarm(capsDeviceAlarm);
    }

    /**
     * 修改安全帽设备告警信息
     * 
     * @param capsDeviceAlarm 安全帽设备告警信息
     * @return 结果
     */
    @Override
    public int updateCapsDeviceAlarm(CapsDeviceAlarm capsDeviceAlarm)
    {
        capsDeviceAlarm.setUpdateTime(DateUtils.getNowDate());
        return capsDeviceAlarmMapper.updateCapsDeviceAlarm(capsDeviceAlarm);
    }

    /**
     * 批量删除安全帽设备告警信息
     * 
     * @param ids 需要删除的安全帽设备告警信息主键
     * @return 结果
     */
    @Override
    public int deleteCapsDeviceAlarmByIds(Integer[] ids)
    {
        return capsDeviceAlarmMapper.deleteCapsDeviceAlarmByIds(ids);
    }

    /**
     * 删除安全帽设备告警信息信息
     * 
     * @param id 安全帽设备告警信息主键
     * @return 结果
     */
    @Override
    public int deleteCapsDeviceAlarmById(Integer id)
    {
        return capsDeviceAlarmMapper.deleteCapsDeviceAlarmById(id);
    }
}
