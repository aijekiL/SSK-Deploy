package com.ruoyi.project.system.mapper;

import java.util.List;
import com.ruoyi.project.system.domain.CapsDeviceReport;

/**
 * 安全帽设备实时数据Mapper接口
 *
 * @author ruoyi
 * @date 2025-04-16
 */
public interface CapsDeviceReportMapper
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
     * 新增安全帽设备历史快照数据
     *
     * @param capsDeviceReport 安全帽设备历史快照数据
     * @return 结果
     */
    public int insertCapsDeviceReportHistory(CapsDeviceReport capsDeviceReport);

    /**
     * 修改安全帽设备实时数据
     *
     * @param capsDeviceReport 安全帽设备实时数据
     * @return 结果
     */
    public int updateCapsDeviceReport(CapsDeviceReport capsDeviceReport);

    /**
     * 删除安全帽设备实时数据
     *
     * @param id 安全帽设备实时数据主键
     * @return 结果
     */
    public int deleteCapsDeviceReportById(Integer id);

    /**
     * 批量删除安全帽设备实时数据
     *
     * @param ids 需要删除的数据主键集合
     * @return 结果
     */
    public int deleteCapsDeviceReportByIds(Integer[] ids);

    /**
     * 根据设备id查询最新的实时数据
     * @param deviceId deviceId
     * @return CapsDeviceReport
     */
    CapsDeviceReport selectCapsDeviceReportByDeviceId(String deviceId);

    /**
     * 根据设备id查询最新的历史快照数据
     * @param deviceId deviceId
     * @return CapsDeviceReport
     */
    CapsDeviceReport selectLatestCapsDeviceReportHistoryByDeviceId(String deviceId);
}
