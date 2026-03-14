package com.ruoyi.project.system.domain.dto;

import java.math.BigDecimal;

import com.fasterxml.jackson.annotation.JsonProperty;
import lombok.Data;

@Data
public class CapsGasMonitorItemDto
{
    @JsonProperty("dict_id")
    private String dictId;

    private BigDecimal val;

    private Integer flag;

    private String unit;

    private Long time;
}
