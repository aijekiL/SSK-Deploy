package com.ruoyi.project.system.domain.dto;

import com.fasterxml.jackson.annotation.JsonProperty;

import java.math.BigDecimal;

public class CapsGasReportItemDto
{
    @JsonProperty("dict_id")
    private String dictId;

    private BigDecimal val;

    private Integer flag;

    private String unit;

    private Long time;

    public String getDictId()
    {
        return dictId;
    }

    public void setDictId(String dictId)
    {
        this.dictId = dictId;
    }

    public BigDecimal getVal()
    {
        return val;
    }

    public void setVal(BigDecimal val)
    {
        this.val = val;
    }

    public Integer getFlag()
    {
        return flag;
    }

    public void setFlag(Integer flag)
    {
        this.flag = flag;
    }

    public String getUnit()
    {
        return unit;
    }

    public void setUnit(String unit)
    {
        this.unit = unit;
    }

    public Long getTime()
    {
        return time;
    }

    public void setTime(Long time)
    {
        this.time = time;
    }
}
