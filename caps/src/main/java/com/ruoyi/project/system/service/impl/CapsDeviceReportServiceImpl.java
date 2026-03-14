package com.ruoyi.project.system.service.impl;

import java.math.BigDecimal;
import java.util.List;
import java.util.Locale;

import com.alibaba.fastjson2.JSONObject;
import com.ruoyi.common.utils.DateUtils;
import com.ruoyi.project.system.domain.CapsDevice;
import com.ruoyi.project.system.domain.CapsDeviceReport;
import com.ruoyi.project.system.domain.dto.CapsGasMonitorItemDto;
import com.ruoyi.project.system.domain.dto.CapsGasMonitorReportDto;
import com.ruoyi.project.system.mapper.CapsDeviceMapper;
import com.ruoyi.project.system.mapper.CapsDeviceReportMapper;
import com.ruoyi.project.system.service.ICapsDeviceReportService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

/**
 * Safety helmet realtime report service implementation.
 */
@Service
public class CapsDeviceReportServiceImpl implements ICapsDeviceReportService
{
    private static final String DICT_ID_H2S = "a21028";
    private static final String DICT_ID_CO = "a21005";
    private static final String DICT_ID_O2 = "a19001";
    private static final String DICT_ID_CH4 = "qt00001";

    @Autowired
    private CapsDeviceReportMapper capsDeviceReportMapper;

    @Autowired
    private CapsDeviceMapper capsDeviceMapper;

    @Override
    public CapsDeviceReport selectCapsDeviceReportById(Integer id)
    {
        return capsDeviceReportMapper.selectCapsDeviceReportById(id);
    }

    @Override
    public List<CapsDeviceReport> selectCapsDeviceReportList(CapsDeviceReport capsDeviceReport)
    {
        return capsDeviceReportMapper.selectCapsDeviceReportList(capsDeviceReport);
    }

    @Override
    public int insertCapsDeviceReport(CapsDeviceReport capsDeviceReport)
    {
        capsDeviceReport.setCreateTime(DateUtils.getNowDate());
        return capsDeviceReportMapper.insertCapsDeviceReport(capsDeviceReport);
    }

    @Override
    public int updateCapsDeviceReport(CapsDeviceReport capsDeviceReport)
    {
        capsDeviceReport.setUpdateTime(DateUtils.getNowDate());
        return capsDeviceReportMapper.updateCapsDeviceReport(capsDeviceReport);
    }

    @Override
    public int deleteCapsDeviceReportByIds(Integer[] ids)
    {
        return capsDeviceReportMapper.deleteCapsDeviceReportByIds(ids);
    }

    @Override
    public int deleteCapsDeviceReportById(Integer id)
    {
        return capsDeviceReportMapper.deleteCapsDeviceReportById(id);
    }

    /**
     * Parse websocket report packet from helmet.
     */
    @Override
    public String parseCapsDeviceReport(JSONObject data)
    {
        if (data != null)
        {
            String userId = data.getString("user_id");
            if (userId != null && !userId.isEmpty())
            {
                CapsDevice capsDevice = capsDeviceMapper.selectCapsDeviceByUserId(userId);
                if (capsDevice != null)
                {
                    String deviceId = capsDevice.getDeviceId();
                    CapsDeviceReport capsDeviceReport = new CapsDeviceReport();
                    capsDeviceReport.setDeviceId(deviceId);
                    capsDeviceReport.setUserId(userId);
                    capsDeviceReport.setCreateTime(DateUtils.getNowDate());

                    // 按当前规则：气体值以 /caps/api/gas/report 的全局推送为准。
                    // 设备心跳上报时继承该设备上一条记录中的气体值，避免被 CD4 覆盖。
                    CapsDeviceReport latestReport = capsDeviceReportMapper.selectCapsDeviceReportByDeviceId(deviceId);
                    if (latestReport != null)
                    {
                        copyGasValues(latestReport, capsDeviceReport);
                    }

                    if (data.containsKey("altitude"))
                    {
                        capsDeviceReport.setAltitude(data.getString("altitude"));
                    }
                    if (data.containsKey("bat_l"))
                    {
                        capsDeviceReport.setBatL(data.getString("bat_l"));
                    }
                    if (data.containsKey("bat_v"))
                    {
                        capsDeviceReport.setBatV(data.getString("bat_v"));
                    }
                    if (data.containsKey("bt_signal_info"))
                    {
                        capsDeviceReport.setBtSignalInfo(data.getString("bt_signal_info"));
                    }
                    if (data.containsKey("c_speed"))
                    {
                        capsDeviceReport.setcSpeed(data.getString("c_speed"));
                    }
                    if (data.containsKey("charging"))
                    {
                        capsDeviceReport.setCharging(data.getString("charging"));
                    }
                    if (data.containsKey("gps"))
                    {
                        capsDeviceReport.setGps(data.getString("gps"));
                    }
                    if (data.containsKey("gps_level"))
                    {
                        capsDeviceReport.setGpsLevel(data.getString("gps_level"));
                    }
                    if (data.containsKey("in_use"))
                    {
                        capsDeviceReport.setInUse(data.getString("in_use"));
                    }
                    if (data.containsKey("is_weared"))
                    {
                        capsDeviceReport.setIsWeared(data.getString("is_weared"));
                    }
                    if (data.containsKey("net_strenth"))
                    {
                        capsDeviceReport.setNetStrenth(data.getString("net_strenth"));
                    }
                    if (data.containsKey("net_type"))
                    {
                        capsDeviceReport.setNetType(data.getString("net_type"));
                    }
                    if (data.containsKey("phoneNumber"))
                    {
                        capsDeviceReport.setPhoneNumber(data.getString("phoneNumber"));
                    }
                    if (data.containsKey("sim_data_num"))
                    {
                        capsDeviceReport.setSimDataNum(data.getString("sim_data_num"));
                    }
                    if (data.containsKey("sim_status"))
                    {
                        capsDeviceReport.setSimStatus(data.getString("sim_status"));
                    }
                    if (data.containsKey("tcard_status"))
                    {
                        capsDeviceReport.setTcardStatus(data.getString("tcard_status"));
                    }
                    if (data.containsKey("tempdata"))
                    {
                        capsDeviceReport.setTempdata(data.getString("tempdata"));
                    }
                    if (data.containsKey("x_point"))
                    {
                        capsDeviceReport.setxPoint(data.getString("x_point"));
                    }
                    if (data.containsKey("y_point"))
                    {
                        capsDeviceReport.setyPoint(data.getString("y_point"));
                    }
                    capsDeviceReportMapper.insertCapsDeviceReport(capsDeviceReport);
                    return deviceId;
                }
            }
        }
        return null;
    }

    @Override
    public int saveGasMonitorReport(CapsGasMonitorReportDto gasReportDto)
    {
        if (gasReportDto == null || gasReportDto.getSn() == null || gasReportDto.getSn().trim().isEmpty())
        {
            return 0;
        }

        List<CapsGasMonitorItemDto> gasItems = gasReportDto.getList();
        if (gasItems == null || gasItems.isEmpty())
        {
            return 0;
        }

        // sn 不再与 device_id 一一对应；每次上报同步到所有设备。
        CapsDeviceReport gasValueCarrier = new CapsDeviceReport();
        boolean hasGasValue = false;
        for (CapsGasMonitorItemDto item : gasItems)
        {
            if (item == null || item.getDictId() == null || item.getVal() == null)
            {
                continue;
            }
            hasGasValue = applyGasValue(gasValueCarrier, item) || hasGasValue;
        }

        if (!hasGasValue)
        {
            return 0;
        }

        List<CapsDevice> capsDevices = capsDeviceMapper.selectCapsDeviceList(new CapsDevice());
        if (capsDevices == null || capsDevices.isEmpty())
        {
            return 0;
        }

        int affectedRows = 0;
        for (CapsDevice capsDevice : capsDevices)
        {
            if (capsDevice == null || capsDevice.getDeviceId() == null || capsDevice.getDeviceId().trim().isEmpty())
            {
                continue;
            }

            String deviceId = capsDevice.getDeviceId().trim();
            CapsDeviceReport capsDeviceReport = capsDeviceReportMapper.selectCapsDeviceReportByDeviceId(deviceId);
            boolean isInsert = false;
            if (capsDeviceReport == null)
            {
                isInsert = true;
                capsDeviceReport = new CapsDeviceReport();
                capsDeviceReport.setDeviceId(deviceId);
                capsDeviceReport.setUserId(capsDevice.getUserId());
                capsDeviceReport.setCreateTime(DateUtils.getNowDate());
            }
            else
            {
                capsDeviceReport.setUpdateTime(DateUtils.getNowDate());
            }

            copyGasValues(gasValueCarrier, capsDeviceReport);
            if (isInsert)
            {
                affectedRows += capsDeviceReportMapper.insertCapsDeviceReport(capsDeviceReport);
            }
            else
            {
                affectedRows += capsDeviceReportMapper.updateCapsDeviceReport(capsDeviceReport);
            }
        }
        return affectedRows;
    }

    private void copyGasValues(CapsDeviceReport source, CapsDeviceReport target)
    {
        target.setO2(source.getO2());
        target.setCo(source.getCo());
        target.setH2s(source.getH2s());
        target.setCh4(source.getCh4());
    }

    private boolean applyGasValue(CapsDeviceReport capsDeviceReport, CapsGasMonitorItemDto gasItem)
    {
        String dictId = gasItem.getDictId().trim().toLowerCase(Locale.ROOT);
        BigDecimal value = gasItem.getVal();
        String val = value.stripTrailingZeros().toPlainString();
        if (DICT_ID_H2S.equals(dictId))
        {
            capsDeviceReport.setH2s(val);
            return true;
        }
        if (DICT_ID_CO.equals(dictId))
        {
            capsDeviceReport.setCo(val);
            return true;
        }
        if (DICT_ID_O2.equals(dictId))
        {
            capsDeviceReport.setO2(val);
            return true;
        }
        if (DICT_ID_CH4.equals(dictId))
        {
            capsDeviceReport.setCh4(val);
            return true;
        }
        return false;
    }

    @Override
    public CapsDeviceReport selectCapsDeviceReportByDeviceId(String deviceId)
    {
        if (deviceId != null && !deviceId.isEmpty())
        {
            return capsDeviceReportMapper.selectCapsDeviceReportByDeviceId(deviceId);
        }
        return null;
    }
}
