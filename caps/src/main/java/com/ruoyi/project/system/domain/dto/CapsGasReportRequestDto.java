package com.ruoyi.project.system.domain.dto;

import java.util.List;

public class CapsGasReportRequestDto
{
    private String sn;

    private List<CapsGasReportItemDto> list;

    public String getSn()
    {
        return sn;
    }

    public void setSn(String sn)
    {
        this.sn = sn;
    }

    public List<CapsGasReportItemDto> getList()
    {
        return list;
    }

    public void setList(List<CapsGasReportItemDto> list)
    {
        this.list = list;
    }
}
