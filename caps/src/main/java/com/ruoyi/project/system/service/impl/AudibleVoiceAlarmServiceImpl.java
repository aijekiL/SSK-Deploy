package com.ruoyi.project.system.service.impl;

import java.util.ArrayList;
import java.util.List;
import com.ruoyi.common.utils.DateUtils;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import com.ruoyi.project.system.mapper.AudibleVoiceAlarmMapper;
import com.ruoyi.project.system.domain.AudibleVoiceAlarm;
import com.ruoyi.project.system.service.IAudibleVoiceAlarmService;

/**
 * 声光报警器设备信息Service业务层处理
 *
 * @author ruoyi
 * @date 2025-06-18
 */
@Service
public class AudibleVoiceAlarmServiceImpl implements IAudibleVoiceAlarmService
{
    @Autowired
    private AudibleVoiceAlarmMapper audibleVoiceAlarmMapper;

    /**
     * 查询声光报警器设备信息
     *
     * @param id 声光报警器设备信息主键
     * @return 声光报警器设备信息
     */
    @Override
    public AudibleVoiceAlarm selectAudibleVoiceAlarmById(Integer id)
    {
        return audibleVoiceAlarmMapper.selectAudibleVoiceAlarmById(id);
    }

    /**
     * 查询声光报警器设备信息列表
     *
     * @param audibleVoiceAlarm 声光报警器设备信息
     * @return 声光报警器设备信息
     */
    @Override
    public List<AudibleVoiceAlarm> selectAudibleVoiceAlarmList(AudibleVoiceAlarm audibleVoiceAlarm)
    {
        return audibleVoiceAlarmMapper.selectAudibleVoiceAlarmList(audibleVoiceAlarm);
    }

    /**
     * 新增声光报警器设备信息
     *
     * @param audibleVoiceAlarm 声光报警器设备信息
     * @return 结果
     */
    @Override
    public int insertAudibleVoiceAlarm(AudibleVoiceAlarm audibleVoiceAlarm)
    {
        audibleVoiceAlarm.setCreateTime(DateUtils.getNowDate());
        return audibleVoiceAlarmMapper.insertAudibleVoiceAlarm(audibleVoiceAlarm);
    }

    /**
     * 修改声光报警器设备信息
     *
     * @param audibleVoiceAlarm 声光报警器设备信息
     * @return 结果
     */
    @Override
    public int updateAudibleVoiceAlarm(AudibleVoiceAlarm audibleVoiceAlarm)
    {
        audibleVoiceAlarm.setUpdateTime(DateUtils.getNowDate());
        return audibleVoiceAlarmMapper.updateAudibleVoiceAlarm(audibleVoiceAlarm);
    }

    /**
     * 批量删除声光报警器设备信息
     *
     * @param ids 需要删除的声光报警器设备信息主键
     * @return 结果
     */
    @Override
    public int deleteAudibleVoiceAlarmByIds(Integer[] ids)
    {
        return audibleVoiceAlarmMapper.deleteAudibleVoiceAlarmByIds(ids);
    }

    /**
     * 删除声光报警器设备信息信息
     *
     * @param id 声光报警器设备信息主键
     * @return 结果
     */
    @Override
    public int deleteAudibleVoiceAlarmById(Integer id)
    {
        return audibleVoiceAlarmMapper.deleteAudibleVoiceAlarmById(id);
    }

    @Override
    public int saveOrUpdateAudibleVoiceAlarm(String personId, List<String> deviceIds) {
        int count = 0;
        for (String deviceId : deviceIds) {
            AudibleVoiceAlarm audibleVoiceAlarm = audibleVoiceAlarmMapper.selectAudibleVoiceAlarmByCode(deviceId);
            if (audibleVoiceAlarm != null) {
                audibleVoiceAlarm.setPersonId(personId);
                int i = audibleVoiceAlarmMapper.updateAudibleVoiceAlarm(audibleVoiceAlarm);
                count += i;
            }else{
                audibleVoiceAlarm = new AudibleVoiceAlarm();
                audibleVoiceAlarm.setPersonId(personId);
                audibleVoiceAlarm.setDeviceCode(deviceId);
                int i = audibleVoiceAlarmMapper.insertAudibleVoiceAlarm(audibleVoiceAlarm);
                count += i;
            }
        }
        return count;
    }

    @Override
    public List<String> getDeviceIdsByPersonId(String personId) {
        AudibleVoiceAlarm audibleVoiceAlarm = new AudibleVoiceAlarm();
        audibleVoiceAlarm.setPersonId(personId);
        List<AudibleVoiceAlarm> AudibleVoiceAlarmList =
                audibleVoiceAlarmMapper.selectAudibleVoiceAlarmList(audibleVoiceAlarm);
        List<String> deviceIds = new ArrayList<>();
        if (AudibleVoiceAlarmList != null && !AudibleVoiceAlarmList.isEmpty()) {
            AudibleVoiceAlarmList.forEach(item -> {
                deviceIds.add(item.getDeviceCode());
            });
        }
        return deviceIds;
    }
}
