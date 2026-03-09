package com.ruoyi.project.system.mapper;


import com.ruoyi.project.system.domain.CapsDevice;

import java.util.List;

/**
 * 安全帽设备信息Mapper接口
 *
 * @author ruoyi
 * @date 2025-04-15
 */
public interface CapsDeviceMapper
{
    /**
     * 查询安全帽设备信息
     *
     * @param id 安全帽设备信息主键
     * @return 安全帽设备信息
     */
    public CapsDevice selectCapsDeviceById(Integer id);

    /**
     * 根据设备id查询
     * @param deviceId
     * @return
     */
    CapsDevice selectCapsDeviceByDeviceId(String deviceId);

    /**
     * 根据用户id查询
     * @param userId
     * @return
     */
    CapsDevice selectCapsDeviceByUserId(String userId);

    /**
     * 查询安全帽设备信息列表
     *
     * @param capsDevice 安全帽设备信息
     * @return 安全帽设备信息集合
     */
    public List<CapsDevice> selectCapsDeviceList(CapsDevice capsDevice);

    /**
     * 新增安全帽设备信息
     *
     * @param capsDevice 安全帽设备信息
     * @return 结果
     */
    public int insertCapsDevice(CapsDevice capsDevice);

    /**
     * 修改安全帽设备信息
     *
     * @param capsDevice 安全帽设备信息
     * @return 结果
     */
    public int updateCapsDevice(CapsDevice capsDevice);

    /**
     * 删除安全帽设备信息
     *
     * @param id 安全帽设备信息主键
     * @return 结果
     */
    public int deleteCapsDeviceById(Integer id);

    /**
     * 批量删除安全帽设备信息
     *
     * @param ids 需要删除的数据主键集合
     * @return 结果
     */
    public int deleteCapsDeviceByIds(Integer[] ids);

    /**
     * 根据设备id修改userId
     *
     */
    public int updateCapsDeviceUserId(String deviceId,String userId);
    public int updateCapsPersonIdByDeviceId(String deviceId,String personId);
}
