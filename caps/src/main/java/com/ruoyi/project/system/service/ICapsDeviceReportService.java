package com.ruoyi.project.system.service;

import java.util.List;

import com.alibaba.fastjson2.JSONObject;
import com.ruoyi.project.system.domain.CapsDeviceReport;
import com.ruoyi.project.system.domain.dto.CapsGasReportRequestDto;

/**
 * 安全帽设备实时数据Service接口
 *
 * @author ruoyi
 * @date 2025-04-16
 */
public interface ICapsDeviceReportService
{
    /**
     * 查询安全帽设备实时数据
     *
     * @param id 安全帽设备实时数据主键
     * @return 安全帽设备实时数据
     */
    public CapsDeviceReport selectCapsDeviceReportById(Integer id);

    /**
     * 查询安全帽设备实时数据列表
     *
     * @param capsDeviceReport 安全帽设备实时数据
     * @return 安全帽设备实时数据集合
     */
    public List<CapsDeviceReport> selectCapsDeviceReportList(CapsDeviceReport capsDeviceReport);

    /**
     * 新增安全帽设备实时数据
     *
     * @param capsDeviceReport 安全帽设备实时数据
     * @return 结果
     */
    public int insertCapsDeviceReport(CapsDeviceReport capsDeviceReport);

    /**
     * 修改安全帽设备实时数据
     *
     * @param capsDeviceReport 安全帽设备实时数据
     * @return 结果
     */
    public int updateCapsDeviceReport(CapsDeviceReport capsDeviceReport);

    /**
     * 批量删除安全帽设备实时数据
     *
     * @param ids 需要删除的安全帽设备实时数据主键集合
     * @return 结果
     */
    public int deleteCapsDeviceReportByIds(Integer[] ids);

    /**
     * 删除安全帽设备实时数据信息
     *
     * @param id 安全帽设备实时数据主键
     * @return 结果
     */
    public int deleteCapsDeviceReportById(Integer id);
    /**
     * 解析安全帽心跳数据包
     * @param data    数据包
     */
    String parseCapsDeviceReport(JSONObject data);
    /**
     * 根据设备id查询最新的一条实时数据
     */
    CapsDeviceReport selectCapsDeviceReportByDeviceId(String deviceId);

    /**
     * 接收气体监测仪上报并转换为前端当前使用的气体字段
     */
    int saveGasReport(CapsGasReportRequestDto gasReportRequest);
}
