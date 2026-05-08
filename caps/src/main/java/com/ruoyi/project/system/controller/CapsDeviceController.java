package com.ruoyi.project.system.controller;

import java.util.List;
import javax.servlet.http.HttpServletResponse;

import com.ruoyi.project.system.domain.CapsDevice;
import com.ruoyi.project.system.domain.dto.CapsDeviceUpdateDto;
import com.ruoyi.project.system.service.ICapsDeviceReportService;
import com.ruoyi.project.system.service.ICapsDeviceService;
import io.swagger.annotations.Api;
import io.swagger.annotations.ApiOperation;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.*;
import com.ruoyi.framework.aspectj.lang.annotation.Log;
import com.ruoyi.framework.aspectj.lang.enums.BusinessType;
import com.ruoyi.framework.web.controller.BaseController;
import com.ruoyi.framework.web.domain.AjaxResult;
import com.ruoyi.common.utils.poi.ExcelUtil;
import com.ruoyi.framework.web.page.TableDataInfo;

/**
 * 安全帽设备信息Controller
 *
 * @author ruoyi
 * @date 2025-04-15
 */
@RestController
@RequestMapping("/system/device")
@Api(tags = {"安全帽设备信息管理"})
public class CapsDeviceController extends BaseController
{
    @Autowired
    private ICapsDeviceService capsDeviceService;

    @Autowired
    private ICapsDeviceReportService capsDeviceReportService;

    /**
     * 查询安全帽设备信息列表
     */
    @GetMapping("/list")
    @ApiOperation(value = "安全帽设备信息列表")
    public TableDataInfo list(CapsDevice capsDevice)
    {
        startPage();
        List<CapsDevice> list = capsDeviceService.selectCapsDeviceList(capsDevice);
        return getDataTable(list);
    }

    @GetMapping("/watchLatest")
    public AjaxResult watchLatest(@RequestParam("personId") String personId,
                                  @RequestParam(value = "excludeDeviceId", required = false) String excludeDeviceId)
    {
        return success(capsDeviceReportService.selectWatchLatestByPersonId(personId, excludeDeviceId));
    }

    /**
     * 导出安全帽设备信息列表
     */
    @Log(title = "安全帽设备信息", businessType = BusinessType.EXPORT)
    @PostMapping("/export")
    public void export(HttpServletResponse response, CapsDevice capsDevice)
    {
        List<CapsDevice> list = capsDeviceService.selectCapsDeviceList(capsDevice);
        ExcelUtil<CapsDevice> util = new ExcelUtil<CapsDevice>(CapsDevice.class);
        util.exportExcel(response, list, "安全帽设备信息数据");
    }

    /**
     * 获取安全帽设备信息详细信息
     */
    @GetMapping(value = "/{id}")
    @ApiOperation(value = "安全帽设备信息详细信息")
    public AjaxResult getInfo(@PathVariable("id") Integer id)
    {
        return success(capsDeviceService.selectCapsDeviceById(id));
    }

    /**
     * 新增安全帽设备信息
     */
    @Log(title = "安全帽设备信息", businessType = BusinessType.INSERT)
    @PostMapping
    public AjaxResult add(@RequestBody CapsDevice capsDevice)
    {
        return toAjax(capsDeviceService.insertCapsDevice(capsDevice));
    }

    /**
     * 修改安全帽设备信息
     */
    @Log(title = "安全帽设备信息", businessType = BusinessType.UPDATE)
    @ApiOperation(value = "安全帽设备信息修改")
    @PutMapping
    public AjaxResult edit(@RequestBody CapsDevice capsDevice)
    {
        return toAjax(capsDeviceService.updateCapsDevice(capsDevice));
    }

    /**
     * 根据deviceId修改安全帽设备userId
     */
    @Log(title = "根据deviceId修改安全帽设备userId", businessType = BusinessType.UPDATE)
    @ApiOperation(value = "根据deviceId修改安全帽设备userId")
    @PutMapping(value = "/setUserId")
    public AjaxResult editUserId(@RequestBody CapsDevice capsDevice)
    {
        return toAjax(capsDeviceService.updateCapsDeviceUserId(capsDevice));
    }


    @PostMapping(value = "/updatePersonIdAndDeviceInfo")
    public AjaxResult updatePersonIdAndDeviceInfo(@RequestBody CapsDeviceUpdateDto dto,
                                                  @RequestHeader("X-API-Secret") String requestSecret){
        try{
            capsDeviceService.updatePersonIdAndDeviceInfo(dto);
            return success("更新成功");
        }catch (Exception e){
            return error("更新失败："+e.getMessage());
        }
    }

    /**
     * 删除安全帽设备信息
     */
    @PreAuthorize("@ss.hasPermi('system:device:remove')")
    @Log(title = "安全帽设备信息", businessType = BusinessType.DELETE)
	@DeleteMapping("/{ids}")
    public AjaxResult remove(@PathVariable Integer[] ids)
    {
        return toAjax(capsDeviceService.deleteCapsDeviceByIds(ids));
    }

}
