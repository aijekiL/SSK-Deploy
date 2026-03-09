package com.ruoyi.project.system.service;


import com.alibaba.fastjson2.JSONObject;
import com.ruoyi.project.system.domain.CapsDevice;
import com.ruoyi.project.system.domain.dto.CapsDeviceUpdateDto;

import java.util.List;

/**
 * 安全帽设备信息Service接口
 *
 * @author ruoyi
 * @date 2025-04-15
 */
public interface ICapsDeviceService
{
    /**
     * 查询安全帽设备信息
     *
     * @param id 安全帽设备信息主键
     * @return 安全帽设备信息
     */
    public CapsDevice selectCapsDeviceById(Integer id);

    CapsDevice selectCapsDeviceByUserId(String userId);
    CapsDevice selectCapsDeviceByDeviceId(String deviceId);


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
     * 修改安全帽设备信息
     *
     * @param capsDevice 安全帽设备信息
     * @return 结果
     */
    public int updateCapsDeviceUserId(CapsDevice capsDevice);

    /**
     * 批量删除安全帽设备信息
     *
     * @param ids 需要删除的安全帽设备信息主键集合
     * @return 结果
     */
    public int deleteCapsDeviceByIds(Integer[] ids);

    /**
     * 删除安全帽设备信息信息
     *
     * @param id 安全帽设备信息主键
     * @return 结果
     */
    public int deleteCapsDeviceById(Integer id);

    /**
     * 组建websocket服务端登录响应包
     */
    JSONObject buildLoginResponse(String deviceId) throws Exception;

    /**
     * 绑定客户id和安全帽设备id
     */
    int saveOrUpdateCapsDevice(String personId, List<String> deviceIds);

    void updatePersonIdAndDeviceInfo(CapsDeviceUpdateDto dto) throws Exception;
}
