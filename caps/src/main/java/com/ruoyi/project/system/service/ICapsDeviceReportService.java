package com.ruoyi.project.system.service;

import java.util.List;

import com.alibaba.fastjson2.JSONObject;
import com.ruoyi.project.system.domain.CapsDeviceReport;
import com.ruoyi.project.system.domain.dto.CapsGasMonitorReportDto;

/**
 * Safety helmet realtime report service.
 */
public interface ICapsDeviceReportService
{
    /**
     * Query by primary key.
     */
    CapsDeviceReport selectCapsDeviceReportById(Integer id);

    /**
     * Query report list.
     */
    List<CapsDeviceReport> selectCapsDeviceReportList(CapsDeviceReport capsDeviceReport);

    /**
     * Insert one report.
     */
    int insertCapsDeviceReport(CapsDeviceReport capsDeviceReport);

    /**
     * Update one report.
     */
    int updateCapsDeviceReport(CapsDeviceReport capsDeviceReport);

    /**
     * Batch delete by ids.
     */
    int deleteCapsDeviceReportByIds(Integer[] ids);

    /**
     * Delete by id.
     */
    int deleteCapsDeviceReportById(Integer id);

    /**
     * Parse report payload from websocket.
     */
    String parseCapsDeviceReport(JSONObject data);

    /**
     * Save gas monitor push payload.
     */
    int saveGasMonitorReport(CapsGasMonitorReportDto gasReportDto);

    /**
     * Query latest report by device id.
     */
    CapsDeviceReport selectCapsDeviceReportByDeviceId(String deviceId);
}
