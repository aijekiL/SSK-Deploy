package com.ruoyi.project.system.domain.dto;

import java.util.List;

import lombok.Data;

@Data
public class CapsGasMonitorReportDto
{
    private String sn;

    private List<CapsGasMonitorItemDto> list;
}
