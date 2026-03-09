package com.ruoyi.project.system.service.impl;

import java.util.List;

import com.alibaba.fastjson2.JSONObject;
import com.ruoyi.common.utils.DateUtils;
import com.ruoyi.project.system.domain.CapsDevice;
import com.ruoyi.project.system.mapper.CapsDeviceMapper;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import com.ruoyi.project.system.mapper.CapsDeviceReportMapper;
import com.ruoyi.project.system.domain.CapsDeviceReport;
import com.ruoyi.project.system.service.ICapsDeviceReportService;

/**
 * 安全帽设备实时数据Service业务层处理
 *
 * @author ruoyi
 * @date 2025-04-16
 */
@Service
public class CapsDeviceReportServiceImpl implements ICapsDeviceReportService
{
    @Autowired
    private CapsDeviceReportMapper capsDeviceReportMapper;

    @Autowired
    private CapsDeviceMapper capsDeviceMapper;

    /**
     * 查询安全帽设备实时数据
     *
     * @param id 安全帽设备实时数据主键
     * @return 安全帽设备实时数据
     */
    @Override
    public CapsDeviceReport selectCapsDeviceReportById(Integer id)
    {
        return capsDeviceReportMapper.selectCapsDeviceReportById(id);
    }

    /**
     * 查询安全帽设备实时数据列表
     *
     * @param capsDeviceReport 安全帽设备实时数据
     * @return 安全帽设备实时数据
     */
    @Override
    public List<CapsDeviceReport> selectCapsDeviceReportList(CapsDeviceReport capsDeviceReport)
    {
        return capsDeviceReportMapper.selectCapsDeviceReportList(capsDeviceReport);
    }

    /**
     * 新增安全帽设备实时数据
     *
     * @param capsDeviceReport 安全帽设备实时数据
     * @return 结果
     */
    @Override
    public int insertCapsDeviceReport(CapsDeviceReport capsDeviceReport)
    {
        capsDeviceReport.setCreateTime(DateUtils.getNowDate());
        return capsDeviceReportMapper.insertCapsDeviceReport(capsDeviceReport);
    }

    /**
     * 修改安全帽设备实时数据
     *
     * @param capsDeviceReport 安全帽设备实时数据
     * @return 结果
     */
    @Override
    public int updateCapsDeviceReport(CapsDeviceReport capsDeviceReport)
    {
        capsDeviceReport.setUpdateTime(DateUtils.getNowDate());
        return capsDeviceReportMapper.updateCapsDeviceReport(capsDeviceReport);
    }

    /**
     * 批量删除安全帽设备实时数据
     *
     * @param ids 需要删除的安全帽设备实时数据主键
     * @return 结果
     */
    @Override
    public int deleteCapsDeviceReportByIds(Integer[] ids)
    {
        return capsDeviceReportMapper.deleteCapsDeviceReportByIds(ids);
    }

    /**
     * 删除安全帽设备实时数据信息
     *
     * @param id 安全帽设备实时数据主键
     * @return 结果
     */
    @Override
    public int deleteCapsDeviceReportById(Integer id)
    {
        return capsDeviceReportMapper.deleteCapsDeviceReportById(id);
    }

    /**
     * 解析安全帽心跳数据包
     *
     * @param data   数据包
     * data:{
     *    altitude,bat_l,bat_v,bt_signal_info,c_speed,charging,gps,
     *    gps_level,in_use,is_weared,net_strenth,net_type,phoneNumber
     *    ,sim_data_num,sim_status,tcard_status,tempdata,x_point,y_point
     * }
     */
    @Override
    public String parseCapsDeviceReport(JSONObject data) {
        //解析json数据包
        if (data != null) {
            String userId = data.getString("user_id");
            if (userId != null && !userId.isEmpty()) {
                CapsDevice capsDevice = capsDeviceMapper.selectCapsDeviceByUserId(userId);
                if (capsDevice != null) {
                    String deviceId = capsDevice.getDeviceId();
                    CapsDeviceReport capsDeviceReport = new CapsDeviceReport();
                    capsDeviceReport.setDeviceId(deviceId);
                    capsDeviceReport.setUserId(userId);
                    capsDeviceReport.setCreateTime(DateUtils.getNowDate());
                    // 解析气体监测数据
                    // "CD4":"CH4--0.0;O2--20.9;CO--0.0;H2S--0.0;NH3--;CH2O--;NO2--;SF6--;H2--"
                    if(data.containsKey("CD4")){
                        String cd4 = data.getString("CD4");
                        if (cd4 != null && !cd4.isEmpty()) {
                            String[] cd4s = cd4.split(";");
                            String ch4 = cd4s[0].split("--")[1];
                            String o2 = cd4s[1].split("--")[1];
                            String co = cd4s[2].split("--")[1];
                            String h2s = cd4s[3].split("--")[1];
                            capsDeviceReport.setCh4(ch4);
                            capsDeviceReport.setO2(o2);
                            capsDeviceReport.setCo(co);
                            capsDeviceReport.setH2s(h2s);
                        }
                    }
                    if (data.containsKey("altitude")) {
                        capsDeviceReport.setAltitude(data.getString("altitude"));
                    }
                    if (data.containsKey("bat_l")) {
                        capsDeviceReport.setBatL(data.getString("bat_l"));
                    }
                    if (data.containsKey("bat_v")) {
                        capsDeviceReport.setBatV(data.getString("bat_v"));
                    }
                    if (data.containsKey("bt_signal_info")) {
                        capsDeviceReport.setBtSignalInfo(data.getString("bt_signal_info"));
                    }
                    if (data.containsKey("c_speed")) {
                        capsDeviceReport.setcSpeed(data.getString("c_speed"));
                    }
                    if (data.containsKey("charging")) {
                        capsDeviceReport.setCharging(data.getString("charging"));
                    }
                    if (data.containsKey("gps")) {
                        capsDeviceReport.setGps(data.getString("gps"));
                    }
                    if (data.containsKey("gps_level")) {
                        capsDeviceReport.setGpsLevel(data.getString("gps_level"));
                    }
                    if (data.containsKey("in_use")) {
                        capsDeviceReport.setInUse(data.getString("in_use"));
                    }
                    if (data.containsKey("is_weared")) {
                        capsDeviceReport.setIsWeared(data.getString("is_weared"));
                    }
                    if (data.containsKey("net_strenth")) {
                        capsDeviceReport.setNetStrenth(data.getString("net_strenth"));
                    }
                    if (data.containsKey("net_type")) {
                        capsDeviceReport.setNetType(data.getString("net_type"));
                    }
                    if (data.containsKey("phoneNumber")) {
                        capsDeviceReport.setPhoneNumber(data.getString("phoneNumber"));
                    }
                    if (data.containsKey("sim_data_num")) {
                        capsDeviceReport.setSimDataNum(data.getString("sim_data_num"));
                    }
                    if (data.containsKey("sim_status")) {
                        capsDeviceReport.setSimStatus(data.getString("sim_status"));
                    }
                    if (data.containsKey("tcard_status")) {
                        capsDeviceReport.setTcardStatus(data.getString("tcard_status"));
                    }
                    if (data.containsKey("tempdata")) {
                        capsDeviceReport.setTempdata(data.getString("tempdata"));
                    }
                    if (data.containsKey("x_point")) {
                        capsDeviceReport.setxPoint(data.getString("x_point"));
                    }
                    if (data.containsKey("y_point")) {
                        capsDeviceReport.setyPoint(data.getString("y_point"));
                    }
                    capsDeviceReportMapper.insertCapsDeviceReport(capsDeviceReport);
                    return deviceId;
                }
            }
        }
        return null;
    }

    /**
     * 根据设备id查询最新的一条实时数据
     *
     * @param deviceId
     */
    @Override
    public CapsDeviceReport selectCapsDeviceReportByDeviceId(String deviceId) {
        if (deviceId != null && !deviceId.isEmpty()) {
            CapsDeviceReport capsDeviceReport = capsDeviceReportMapper.selectCapsDeviceReportByDeviceId(deviceId);
            return capsDeviceReport;
        }
        return null;
    }

}
