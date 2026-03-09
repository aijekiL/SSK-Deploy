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
import com.ruoyi.project.system.domain.AudibleVoiceAlarm;
import com.ruoyi.project.system.service.IAudibleVoiceAlarmService;
import com.ruoyi.framework.web.controller.BaseController;
import com.ruoyi.framework.web.domain.AjaxResult;
import com.ruoyi.common.utils.poi.ExcelUtil;
import com.ruoyi.framework.web.page.TableDataInfo;

/**
 * 声光报警器设备信息Controller
 *
 * @author ruoyi
 * @date 2025-06-18
 */
@RestController
@RequestMapping("/system/alarm")
public class AudibleVoiceAlarmController extends BaseController
{
    @Autowired
    private IAudibleVoiceAlarmService audibleVoiceAlarmService;

    /**
     * 查询声光报警器设备信息列表
     */
    @GetMapping("/list")
    public TableDataInfo list(AudibleVoiceAlarm audibleVoiceAlarm)
    {
        startPage();
        List<AudibleVoiceAlarm> list = audibleVoiceAlarmService.selectAudibleVoiceAlarmList(audibleVoiceAlarm);
        return getDataTable(list);
    }

    /**
     * 导出声光报警器设备信息列表
     */
    @Log(title = "声光报警器设备信息", businessType = BusinessType.EXPORT)
    @PostMapping("/export")
    public void export(HttpServletResponse response, AudibleVoiceAlarm audibleVoiceAlarm)
    {
        List<AudibleVoiceAlarm> list = audibleVoiceAlarmService.selectAudibleVoiceAlarmList(audibleVoiceAlarm);
        ExcelUtil<AudibleVoiceAlarm> util = new ExcelUtil<AudibleVoiceAlarm>(AudibleVoiceAlarm.class);
        util.exportExcel(response, list, "声光报警器设备信息数据");
    }

    /**
     * 获取声光报警器设备信息详细信息
     */
    @GetMapping(value = "/{id}")
    public AjaxResult getInfo(@PathVariable("id") Integer id)
    {
        return success(audibleVoiceAlarmService.selectAudibleVoiceAlarmById(id));
    }

    /**
     * 新增声光报警器设备信息
     */
    @Log(title = "声光报警器设备信息", businessType = BusinessType.INSERT)
    @PostMapping
    public AjaxResult add(@RequestBody AudibleVoiceAlarm audibleVoiceAlarm)
    {
        return toAjax(audibleVoiceAlarmService.insertAudibleVoiceAlarm(audibleVoiceAlarm));
    }

    /**
     * 修改声光报警器设备信息
     */
    @Log(title = "声光报警器设备信息", businessType = BusinessType.UPDATE)
    @PutMapping
    public AjaxResult edit(@RequestBody AudibleVoiceAlarm audibleVoiceAlarm)
    {
        return toAjax(audibleVoiceAlarmService.updateAudibleVoiceAlarm(audibleVoiceAlarm));
    }

    /**
     * 删除声光报警器设备信息
     */
    @Log(title = "声光报警器设备信息", businessType = BusinessType.DELETE)
	@DeleteMapping("/{ids}")
    public AjaxResult remove(@PathVariable Integer[] ids)
    {
        return toAjax(audibleVoiceAlarmService.deleteAudibleVoiceAlarmByIds(ids));
    }
}
