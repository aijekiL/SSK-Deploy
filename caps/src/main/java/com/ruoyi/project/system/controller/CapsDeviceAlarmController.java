package com.ruoyi.project.system.controller;

import java.util.List;
import javax.servlet.http.HttpServletResponse;

import io.swagger.annotations.Api;
import io.swagger.annotations.ApiOperation;
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
import com.ruoyi.project.system.domain.CapsDeviceAlarm;
import com.ruoyi.project.system.service.ICapsDeviceAlarmService;
import com.ruoyi.framework.web.controller.BaseController;
import com.ruoyi.framework.web.domain.AjaxResult;
import com.ruoyi.common.utils.poi.ExcelUtil;
import com.ruoyi.framework.web.page.TableDataInfo;

/**
 * 安全帽设备告警信息Controller
 *
 * @author ruoyi
 * @date 2025-04-19
 */
@Api(tags = {"安全帽设备告警信息管理"})
@RestController
@RequestMapping("/system/device/alarm")
public class CapsDeviceAlarmController extends BaseController
{
    @Autowired
    private ICapsDeviceAlarmService capsDeviceAlarmService;

    /**
     * 查询安全帽设备告警信息列表
     */
    @GetMapping("/list")
    @ApiOperation(value = "安全帽设备告警信息列表")
    public TableDataInfo list(CapsDeviceAlarm capsDeviceAlarm)
    {
        startPage();
        List<CapsDeviceAlarm> list = capsDeviceAlarmService.selectCapsDeviceAlarmList(capsDeviceAlarm);
        return getDataTable(list);
    }

    /**
     * 导出安全帽设备告警信息列表
     */
    @Log(title = "安全帽设备告警信息", businessType = BusinessType.EXPORT)
    @PostMapping("/export")
    @ApiOperation(value = "安全帽设备告警信息导出")
    public void export(HttpServletResponse response, CapsDeviceAlarm capsDeviceAlarm)
    {
        List<CapsDeviceAlarm> list = capsDeviceAlarmService.selectCapsDeviceAlarmList(capsDeviceAlarm);
        ExcelUtil<CapsDeviceAlarm> util = new ExcelUtil<CapsDeviceAlarm>(CapsDeviceAlarm.class);
        util.exportExcel(response, list, "安全帽设备告警信息数据");
    }

    /**
     * 获取安全帽设备告警信息详细信息
     */
    @GetMapping(value = "/{id}")
    @ApiOperation(value = "安全帽设备告警信息详细信息")
    public AjaxResult getInfo(@PathVariable("id") Integer id)
    {
        return success(capsDeviceAlarmService.selectCapsDeviceAlarmById(id));
    }

    /**
     * 新增安全帽设备告警信息
     */
    @PreAuthorize("@ss.hasPermi('system:alarm:add')")
    @Log(title = "安全帽设备告警信息", businessType = BusinessType.INSERT)
    @PostMapping
    public AjaxResult add(@RequestBody CapsDeviceAlarm capsDeviceAlarm)
    {
        return toAjax(capsDeviceAlarmService.insertCapsDeviceAlarm(capsDeviceAlarm));
    }

    /**
     * 修改安全帽设备告警信息
     */
    @PreAuthorize("@ss.hasPermi('system:alarm:edit')")
    @Log(title = "安全帽设备告警信息", businessType = BusinessType.UPDATE)
    @PutMapping
    public AjaxResult edit(@RequestBody CapsDeviceAlarm capsDeviceAlarm)
    {
        return toAjax(capsDeviceAlarmService.updateCapsDeviceAlarm(capsDeviceAlarm));
    }

    /**
     * 删除安全帽设备告警信息
     */
    @PreAuthorize("@ss.hasPermi('system:alarm:remove')")
    @Log(title = "安全帽设备告警信息", businessType = BusinessType.DELETE)
	@DeleteMapping("/{ids}")
    public AjaxResult remove(@PathVariable Integer[] ids)
    {
        return toAjax(capsDeviceAlarmService.deleteCapsDeviceAlarmByIds(ids));
    }
}
