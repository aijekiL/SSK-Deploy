package com.ruoyi.project.system.domain.dto;

import lombok.Data;
import lombok.experimental.Accessors;

import java.util.List;

/**
 * 功能描述
 *
 * @author: gzh
 * @date: 2025年06月18日 15:55
 */
@Data
@Accessors(chain = true)
public class CapsDeviceUpdateDto {
    //客户id
    private String personId;
    //安全帽设备id
    private List<String> deviceIds;
    //声光报警器设备id
    private List<String> audibleIds;

}
