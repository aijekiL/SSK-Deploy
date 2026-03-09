package com.ruoyi.project.system.service.impl;

import java.util.List;
import java.util.UUID;
import java.util.stream.Collectors;

import com.alibaba.fastjson2.JSONObject;
import com.ruoyi.common.utils.DateUtils;
import com.ruoyi.project.system.domain.CapsDevice;
import com.ruoyi.project.system.domain.CapsDeviceReport;
import com.ruoyi.project.system.domain.dto.CapsDeviceUpdateDto;
import com.ruoyi.project.system.mapper.CapsDeviceMapper;
import com.ruoyi.project.system.mapper.CapsDeviceReportMapper;
import com.ruoyi.project.system.service.IAudibleVoiceAlarmService;
import com.ruoyi.project.system.service.ICapsDeviceService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

/**
 * 安全帽设备信息Service业务层处理
 *
 * @author ruoyi
 * @date 2025-04-15
 */
@Service
public class CapsDeviceServiceImpl implements ICapsDeviceService
{
    @Autowired
    private CapsDeviceMapper capsDeviceMapper;

    @Autowired
    private CapsDeviceReportMapper capsDeviceReportMapper;

    @Autowired
    private IAudibleVoiceAlarmService authenticationService;

    /**
     * 查询安全帽设备信息
     *
     * @param id 安全帽设备信息主键
     * @return 安全帽设备信息
     */
    @Override
    public CapsDevice selectCapsDeviceById(Integer id)
    {
        CapsDevice capsDevice = capsDeviceMapper.selectCapsDeviceById(id);
        if (capsDevice != null) {
            CapsDeviceReport capsDeviceReport =
                    capsDeviceReportMapper.selectCapsDeviceReportByDeviceId(capsDevice.getDeviceId());
            capsDevice.setCapsDeviceReport(capsDeviceReport);
        }
        return capsDevice;
    }

    @Override
    public CapsDevice selectCapsDeviceByUserId(String userId) {
        return capsDeviceMapper.selectCapsDeviceByUserId(userId);
    }

    @Override
    public CapsDevice selectCapsDeviceByDeviceId(String deviceId) {
        return capsDeviceMapper.selectCapsDeviceByDeviceId(deviceId);
    }

    /**
     * 查询安全帽设备信息列表
     *
     * @param capsDevice 安全帽设备信息
     * @return 安全帽设备信息
     */
    @Override
    public List<CapsDevice> selectCapsDeviceList(CapsDevice capsDevice)
    {
        List<CapsDevice> capsDevices = capsDeviceMapper.selectCapsDeviceList(capsDevice);
        if (!capsDevices.isEmpty()) {
            for (CapsDevice capsDevice1 : capsDevices) {
                CapsDeviceReport capsDeviceReport =
                        capsDeviceReportMapper.selectCapsDeviceReportByDeviceId(capsDevice1.getDeviceId());
                capsDevice1.setCapsDeviceReport(capsDeviceReport);
            }
         }
        return capsDevices;
    }

    /**
     * 新增安全帽设备信息
     *
     * @param capsDevice 安全帽设备信息
     * @return 结果
     */
    @Override
    public int insertCapsDevice(CapsDevice capsDevice)
    {
        capsDevice.setCreateTime(DateUtils.getNowDate());
        return capsDeviceMapper.insertCapsDevice(capsDevice);
    }

    /**
     * 修改安全帽设备信息
     *
     * @param capsDevice 安全帽设备信息
     * @return 结果
     */
    @Override
    public int updateCapsDeviceUserId(CapsDevice capsDevice) {

        String deviceId = capsDevice.getDeviceId();
        String userId = capsDevice.getUserId();
        if(deviceId == null || userId == null){
            return 0;
        }
        return capsDeviceMapper.updateCapsDeviceUserId(deviceId,userId);
    }

    /**
     * 修改安全帽设备信息
     *
     * @param capsDevice 安全帽设备信息
     * @return 结果
     */
    @Override
    public int updateCapsDevice(CapsDevice capsDevice)
    {
        capsDevice.setUpdateTime(DateUtils.getNowDate());
        return capsDeviceMapper.updateCapsDevice(capsDevice);
    }

    /**
     * 批量删除安全帽设备信息
     *
     * @param ids 需要删除的安全帽设备信息主键
     * @return 结果
     */
    @Override
    public int deleteCapsDeviceByIds(Integer[] ids)
    {
        return capsDeviceMapper.deleteCapsDeviceByIds(ids);
    }

    /**
     * 删除安全帽设备信息信息
     *
     * @param id 安全帽设备信息主键
     * @return 结果
     */
    @Override
    public int deleteCapsDeviceById(Integer id)
    {
        return capsDeviceMapper.deleteCapsDeviceById(id);
    }

    /**
     * 组建websocket服务端登录响应包
     * {
     * "user_id": "84990",
     * "device_id": "866652021057907",
     * "bt_scanner": null,
     * "ca_ver": "rd_aqm6811-q0.mp1-L5.0Ah-PV1-6811GasRtk-m6811gasrtk-user-svn6857-
     * v1.2.2-20231025_1753",
     * "mobile": "11111111111",
     * "sim": "89860321442012792856",
     * "user_name": "acsa",
     * "real_name": "acsa",
     * "f_id": "0",
     * "cap_type": 2,
     * "b1": "",
     * "b2": "",
     * "sos_height": "10000.00",
     * "tk": "2",
     * "nearvolt": "2",
     * "health": "2",
     * "pressure": "2",
     * "account_id": "11444",
     * "w_id": null,
     * "acc_list": "11444,11636,11847",
     * "c_user_id": "11847",
     * "gas_volt": "0",
     * "new_low": 0,
     * "uwb_device_id": "",
     * "voice_pkg": null,
     * "fb_id": null,
     * "min_o2": "19.0",
     * "max_o2": "23.5",
     * "watch": "",
     * "local_record": "0",
     * "watch_mac": "",
     * "co": "10.0",
     * "nh3": null,
     * "h2s": null,
     * "ch4": null,
     * "shrink": null,
     * "diastolic": null,
     * "video_pre": null,
     * "obstacle_distance_up": null,
     * "obstacle_distance_rear": null
     * }
     *
     * @param deviceId deviceId
     */
    @Override
    public JSONObject buildLoginResponse(String deviceId) throws Exception {
        JSONObject jsonObject = new JSONObject();
        try {
            CapsDevice capsDevice = capsDeviceMapper.selectCapsDeviceByDeviceId(deviceId);
            if (capsDevice != null) {
                jsonObject.put("user_id", capsDevice.getUserId());
            }else{
                jsonObject.put("user_id", "");
            }
            jsonObject.put("device_id", deviceId);
//            jsonObject.put("bt_scanner", "");
//            jsonObject.put("ca_ver", "rd_aqm6811-q0.mp1-L5.0Ah-PV1-6811GasRtk-m6811gasrtk-user-svn6857-v1.2.2-20231025_1753");
//            jsonObject.put("mobile", "11111111111");
//            jsonObject.put("sim", "89860321442012792856");
//            jsonObject.put("user_name", capsDevice.getUserName());
//            jsonObject.put("real_name", capsDevice.getUserName());
//            jsonObject.put("f_id", "0");
//            jsonObject.put("cap_type", 2);
//            jsonObject.put("b1", "");
//            jsonObject.put("b2", "");
//            jsonObject.put("sos_height", "10000.00");
//            jsonObject.put("tk", "2");
//            jsonObject.put("nearvolt", "2");
//            jsonObject.put("health", "2");
//            jsonObject.put("pressure", "2");
//            jsonObject.put("account_id", "11444");
//            jsonObject.put("w_id", "");
//            jsonObject.put("acc_list", "11444,11636,11847");
//            jsonObject.put("c_user_id", "11847");
//            jsonObject.put("gas_volt", "0");
//            jsonObject.put("new_low", 0);
//            jsonObject.put("uwb_device_id", "");
//            jsonObject.put("voice_pkg", "");
//            jsonObject.put("fb_id", "");
            jsonObject.put("min_o2", "19.0");
            jsonObject.put("max_o2", "23.5");
//            jsonObject.put("watch", "");
//            jsonObject.put("local_record", "0");
//            jsonObject.put("watch_mac", "");
            jsonObject.put("co", "10.0");
//            jsonObject.put("nh3", "");
            jsonObject.put("h2s", "");
            jsonObject.put("ch4", "");
//            jsonObject.put("shrink", "");
//            jsonObject.put("diastolic", "");
//            jsonObject.put("video_pre", "");
//            jsonObject.put("obstacle_distance_up", "");
//            jsonObject.put("obstacle_distance_rear", "");
        } catch (Exception e) {
            throw new Exception("build login message err:"+e.getMessage());
        }
        return jsonObject;
    }

    /**
     * 绑定客户id和安全帽设备id
     *
     * @param personId
     * @param deviceIds
     */
    @Override
    public int saveOrUpdateCapsDevice(String personId, List<String> deviceIds) {
        //遍历设备id列表，如果设备id存在则更新personId,如果不存在则插入数据并保存personId和userId. userId用随机数不可重复
        int count = 0;
        for (String deviceId : deviceIds) {
            CapsDevice capsDevice = capsDeviceMapper.selectCapsDeviceByDeviceId(deviceId);
            if (capsDevice != null) {
                capsDevice.setPersonId(personId);
                int i = capsDeviceMapper.updateCapsPersonIdByDeviceId(deviceId, personId);
                count += i;
            }else{
                capsDevice = new CapsDevice();
                capsDevice.setPersonId(personId);
                capsDevice.setDeviceId(deviceId);
                capsDevice.setUserId(UUID.randomUUID().toString());
                int i = capsDeviceMapper.insertCapsDevice(capsDevice);
                count += i;
            }
        }
        return count;
    }

    @Override
    @Transactional(rollbackFor = Exception.class)
    public void updatePersonIdAndDeviceInfo(CapsDeviceUpdateDto dto) throws Exception {
        String personId = dto.getPersonId();
        if (personId == null || personId.isEmpty()) {
            throw new Exception("personId is null");
        }
        List<String> deviceIds = dto.getDeviceIds();
        List<String> audibleIds = dto.getAudibleIds();
        if(deviceIds != null && !deviceIds.isEmpty()){
            //去重
            deviceIds = deviceIds.stream().distinct().collect(Collectors.toList());
            int count = saveOrUpdateCapsDevice(personId, deviceIds);
            if(count != deviceIds.size()){
                throw new Exception("save or update caps device err");
            }
        }
        if(audibleIds != null && !audibleIds.isEmpty()){
            //去重
            audibleIds = audibleIds.stream().distinct().collect(Collectors.toList());
            int count = authenticationService.saveOrUpdateAudibleVoiceAlarm(personId, audibleIds);
            if(count != audibleIds.size()){
                throw new Exception("save or update audible voice alarm err");
            }
        }
    }
}
