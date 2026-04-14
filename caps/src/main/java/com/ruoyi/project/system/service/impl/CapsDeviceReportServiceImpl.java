package com.ruoyi.project.system.service.impl;

import java.math.BigDecimal;
import java.util.Date;
import java.util.List;

import com.alibaba.fastjson2.JSONObject;
import com.ruoyi.common.constant.CapsConstants;
import com.ruoyi.common.utils.DateUtils;
import com.ruoyi.common.utils.StringUtils;
import com.ruoyi.common.utils.bean.BeanUtils;
import com.ruoyi.project.system.domain.CapsDevice;
import com.ruoyi.project.system.domain.CapsDeviceReport;
import com.ruoyi.project.system.domain.dto.CapsGasReportItemDto;
import com.ruoyi.project.system.domain.dto.CapsGasReportRequestDto;
import com.ruoyi.project.system.mapper.CapsDeviceMapper;
import com.ruoyi.project.system.mapper.CapsDeviceReportMapper;
import com.ruoyi.project.system.service.ICapsDeviceReportService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

@Service
public class CapsDeviceReportServiceImpl implements ICapsDeviceReportService
{
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

    @Override
    public String parseCapsDeviceReport(JSONObject data)
    {
        if (data == null) {
            return null;
        }

        String userId = data.getString("user_id");
        if (StringUtils.isEmpty(userId)) {
            return null;
        }

        CapsDevice capsDevice = capsDeviceMapper.selectCapsDeviceByUserId(userId);
        if (capsDevice == null) {
            return null;
        }

        String deviceId = capsDevice.getDeviceId();
        CapsDeviceReport capsDeviceReport = buildReportFromLatest(deviceId, userId);

        if (data.containsKey("CD4")) {
            String cd4 = data.getString("CD4");
            if (StringUtils.isNotEmpty(cd4)) {
                applyCd4Values(capsDeviceReport, cd4);
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

    @Override
    public CapsDeviceReport selectCapsDeviceReportByDeviceId(String deviceId)
    {
        if (StringUtils.isNotEmpty(deviceId)) {
            return capsDeviceReportMapper.selectCapsDeviceReportByDeviceId(deviceId);
        }
        return null;
    }

    @Override
    public int saveGasReport(CapsGasReportRequestDto gasReportRequest)
    {
        if (gasReportRequest.getList() == null || gasReportRequest.getList().isEmpty()) {
            throw new IllegalArgumentException("gas data list is required");
        }

        Date reportTime = extractReportTime(gasReportRequest.getList());
        if (reportTime == null) {
            reportTime = DateUtils.getNowDate();
        }
        List<CapsDevice> capsDevices = capsDeviceMapper.selectCapsDeviceList(new CapsDevice());
        if (capsDevices == null || capsDevices.isEmpty()) {
            throw new IllegalArgumentException("no caps device found");
        }

        boolean hasMappedValue = false;
        int updatedCount = 0;
        for (CapsDevice capsDevice : capsDevices) {
            CapsDeviceReport latestReport =
                    capsDeviceReportMapper.selectCapsDeviceReportByDeviceId(capsDevice.getDeviceId());
            CapsDeviceReport targetReport = latestReport;
            if (targetReport == null) {
                targetReport = new CapsDeviceReport();
                targetReport.setDeviceId(capsDevice.getDeviceId());
                targetReport.setUserId(capsDevice.getUserId());
            }
            boolean mappedCurrentDevice = applyGasValues(targetReport, gasReportRequest.getList());
            if (!mappedCurrentDevice) {
                continue;
            }
            hasMappedValue = true;
            targetReport.setUpdateTime(reportTime);
            if (latestReport == null) {
                insertCapsDeviceReport(targetReport);
            } else {
                capsDeviceReportMapper.updateCapsDeviceReport(targetReport);
            }
            updatedCount++;
        }

        if (!hasMappedValue) {
            throw new IllegalArgumentException("no supported gas dict_id found");
        }

        return updatedCount;
    }

    private CapsDeviceReport buildReportFromLatest(String deviceId, String userId)
    {
        CapsDeviceReport capsDeviceReport = new CapsDeviceReport();
        CapsDeviceReport latestReport = capsDeviceReportMapper.selectCapsDeviceReportByDeviceId(deviceId);
        if (latestReport != null) {
            BeanUtils.copyBeanProp(capsDeviceReport, latestReport);
        }
        capsDeviceReport.setId(null);
        capsDeviceReport.setDeviceId(deviceId);
        if (StringUtils.isNotEmpty(userId)) {
            capsDeviceReport.setUserId(userId);
        }
        capsDeviceReport.setCreateBy(null);
        capsDeviceReport.setCreateTime(DateUtils.getNowDate());
        capsDeviceReport.setUpdateBy(null);
        capsDeviceReport.setUpdateTime(null);
        return capsDeviceReport;
    }

    private String toPlainString(BigDecimal value)
    {
        return value == null ? null : value.toPlainString();
    }

    private boolean applyGasValues(CapsDeviceReport targetReport, List<CapsGasReportItemDto> items)
    {
        boolean hasMappedValue = false;
        for (CapsGasReportItemDto item : items) {
            if (item == null || StringUtils.isEmpty(item.getDictId()) || item.getVal() == null) {
                continue;
            }

            String value = toPlainString(item.getVal());
            switch (item.getDictId()) {
                case CapsConstants.GAS_DICT_H2S:
                    targetReport.setH2s(value);
                    hasMappedValue = true;
                    break;
                case CapsConstants.GAS_DICT_CO:
                    targetReport.setCo(value);
                    hasMappedValue = true;
                    break;
                case CapsConstants.GAS_DICT_O2:
                    targetReport.setO2(value);
                    hasMappedValue = true;
                    break;
                case CapsConstants.GAS_DICT_CH4:
                    targetReport.setCh4(value);
                    hasMappedValue = true;
                    break;
                default:
                    break;
            }
        }
        return hasMappedValue;
    }

    private Date extractReportTime(List<CapsGasReportItemDto> items)
    {
        Long latestEpochSecond = null;
        for (CapsGasReportItemDto item : items) {
            if (item == null || item.getTime() == null || item.getTime() <= 0) {
                continue;
            }
            if (latestEpochSecond == null || item.getTime() > latestEpochSecond) {
                latestEpochSecond = item.getTime();
            }
        }
        return latestEpochSecond == null ? null : new Date(latestEpochSecond * 1000L);
    }

    private void applyCd4Values(CapsDeviceReport capsDeviceReport, String cd4)
    {
        for (String gasItem : cd4.split(";")) {
            if (StringUtils.isEmpty(gasItem)) {
                continue;
            }

            int separatorIndex = gasItem.indexOf("--");
            if (separatorIndex < 0) {
                continue;
            }

            String gasName = gasItem.substring(0, separatorIndex);
            String gasValue = gasItem.substring(separatorIndex + 2);
            if (StringUtils.isEmpty(gasValue)) {
                continue;
            }

            switch (gasName) {
                case "CH4":
                    capsDeviceReport.setCh4(gasValue);
                    break;
                case "O2":
                    capsDeviceReport.setO2(gasValue);
                    break;
                case "CO":
                    capsDeviceReport.setCo(gasValue);
                    break;
                case "H2S":
                    capsDeviceReport.setH2s(gasValue);
                    break;
                default:
                    break;
            }
        }
    }
}
