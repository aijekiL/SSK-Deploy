package com.ruoyi.project.system.mapper;

import java.util.List;
import com.ruoyi.project.system.domain.AudibleVoiceAlarm;

/**
 * 声光报警器设备信息Mapper接口
 *
 * @author ruoyi
 * @date 2025-06-18
 */
public interface AudibleVoiceAlarmMapper
{
    /**
     * 查询声光报警器设备信息
     *
     * @param id 声光报警器设备信息主键
     * @return 声光报警器设备信息
     */
    public AudibleVoiceAlarm selectAudibleVoiceAlarmById(Integer id);

    /**
     * 查询声光报警器设备信息列表
     *
     * @param audibleVoiceAlarm 声光报警器设备信息
     * @return 声光报警器设备信息集合
     */
    public List<AudibleVoiceAlarm> selectAudibleVoiceAlarmList(AudibleVoiceAlarm audibleVoiceAlarm);

    /**
     * 新增声光报警器设备信息
     *
     * @param audibleVoiceAlarm 声光报警器设备信息
     * @return 结果
     */
    public int insertAudibleVoiceAlarm(AudibleVoiceAlarm audibleVoiceAlarm);

    /**
     * 修改声光报警器设备信息
     *
     * @param audibleVoiceAlarm 声光报警器设备信息
     * @return 结果
     */
    public int updateAudibleVoiceAlarm(AudibleVoiceAlarm audibleVoiceAlarm);

    /**
     * 删除声光报警器设备信息
     *
     * @param id 声光报警器设备信息主键
     * @return 结果
     */
    public int deleteAudibleVoiceAlarmById(Integer id);

    /**
     * 批量删除声光报警器设备信息
     *
     * @param ids 需要删除的数据主键集合
     * @return 结果
     */
    public int deleteAudibleVoiceAlarmByIds(Integer[] ids);

    AudibleVoiceAlarm selectAudibleVoiceAlarmByCode(String deviceCode);
}
