package com.ruoyi.project.system.controller;

import java.util.List;
import javax.servlet.http.HttpServletResponse;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.PutMapping;
import org.springframework.web.bind.annotation.DeleteMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;
import com.ruoyi.framework.aspectj.lang.annotation.Log;
import com.ruoyi.framework.aspectj.lang.enums.BusinessType;
import com.ruoyi.project.system.domain.CapsDeviceReport;
import com.ruoyi.project.system.service.ICapsDeviceReportService;
import com.ruoyi.framework.web.controller.BaseController;
import com.ruoyi.framework.web.domain.AjaxResult;
import com.ruoyi.common.utils.poi.ExcelUtil;
import com.ruoyi.framework.web.page.TableDataInfo;

/**
 * 安全帽设备实时数据Controller
 * 
 * @author ruoyi
 * @date 2025-04-16
 */
@RestController
@RequestMapping("/system/report")
public class CapsDeviceReportController extends BaseController
{
    @Autowired
    private ICapsDeviceReportService capsDeviceReportService;

    /**
     * 查询安全帽设备实时数据列表
     */
    @PreAuthorize("@ss.hasPermi('system:report:list')")
    @GetMapping("/list")
    public TableDataInfo list(CapsDeviceReport capsDeviceReport)
    {
        startPage();
        List<CapsDeviceReport> list = capsDeviceReportService.selectCapsDeviceReportList(capsDeviceReport);
        return getDataTable(list);
    }

    /**
     * 导出安全帽设备实时数据列表
     */
    @PreAuthorize("@ss.hasPermi('system:report:export')")
    @Log(title = "安全帽设备实时数据", businessType = BusinessType.EXPORT)
    @PostMapping("/export")
    public void export(HttpServletResponse response, CapsDeviceReport capsDeviceReport)
    {
        List<CapsDeviceReport> list = capsDeviceReportService.selectCapsDeviceReportList(capsDeviceReport);
        ExcelUtil<CapsDeviceReport> util = new ExcelUtil<CapsDeviceReport>(CapsDeviceReport.class);
        util.exportExcel(response, list, "安全帽设备实时数据数据");
    }

    /**
     * 获取安全帽设备实时数据详细信息
     */
    @PreAuthorize("@ss.hasPermi('system:report:query')")
    @GetMapping(value = "/{id}")
    public AjaxResult getInfo(@PathVariable("id") Integer id)
    {
        return success(capsDeviceReportService.selectCapsDeviceReportById(id));
    }

    /**
     * 新增安全帽设备实时数据
     */
    @PreAuthorize("@ss.hasPermi('system:report:add')")
    @Log(title = "安全帽设备实时数据", businessType = BusinessType.INSERT)
    @PostMapping
    public AjaxResult add(@RequestBody CapsDeviceReport capsDeviceReport)
    {
        return toAjax(capsDeviceReportService.insertCapsDeviceReport(capsDeviceReport));
    }

    /**
     * 修改安全帽设备实时数据
     */
    @PreAuthorize("@ss.hasPermi('system:report:edit')")
    @Log(title = "安全帽设备实时数据", businessType = BusinessType.UPDATE)
    @PutMapping
    public AjaxResult edit(@RequestBody CapsDeviceReport capsDeviceReport)
    {
        return toAjax(capsDeviceReportService.updateCapsDeviceReport(capsDeviceReport));
    }

    /**
     * 删除安全帽设备实时数据
     */
    @PreAuthorize("@ss.hasPermi('system:report:remove')")
    @Log(title = "安全帽设备实时数据", businessType = BusinessType.DELETE)
	@DeleteMapping("/{ids}")
    public AjaxResult remove(@PathVariable Integer[] ids)
    {
        return toAjax(capsDeviceReportService.deleteCapsDeviceReportByIds(ids));
    }
}
