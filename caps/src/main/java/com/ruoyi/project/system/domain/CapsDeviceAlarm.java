package com.ruoyi.project.system.domain;

import io.swagger.annotations.ApiModel;
import io.swagger.annotations.ApiModelProperty;
import org.apache.commons.lang3.builder.ToStringBuilder;
import org.apache.commons.lang3.builder.ToStringStyle;
import com.ruoyi.framework.aspectj.lang.annotation.Excel;
import com.ruoyi.framework.web.domain.BaseEntity;

/**
 * 安全帽设备告警信息对象 caps_device_alarm
 *
 * @author ruoyi
 * @date 2025-04-19
 */
@ApiModel(value = "安全帽设备告警信息对象")
public class CapsDeviceAlarm extends BaseEntity
{
    private static final long serialVersionUID = 1L;

    /** ID */
    private Integer id;

    /** 设备id */
    @Excel(name = "设备id")
    @ApiModelProperty( value = "设备id")
    private String deviceId;

    /** 报警类型 */
    @Excel(name = "报警类型")
    @ApiModelProperty( value = "报警类型")
    private Integer type;

    /** 报警信息 */
    @Excel(name = "报警信息")
    @ApiModelProperty( value = "报警信息")
    private String typeInfo;

    /** 纬度 */
    @Excel(name = "纬度")
    @ApiModelProperty( value = "纬度")
    private String xPoint;

    /** 经度 */
    @Excel(name = "经度")
    @ApiModelProperty( value = "经度")
    private String yPoint;

    public void setId(Integer id)
    {
        this.id = id;
    }

    public Integer getId()
    {
        return id;
    }
    public void setDeviceId(String deviceId)
    {
        this.deviceId = deviceId;
    }

    public String getDeviceId()
    {
        return deviceId;
    }
    public void setType(Integer type)
    {
        this.type = type;
    }

    public Integer getType()
    {
        return type;
    }
    public void setTypeInfo(String typeInfo)
    {
        this.typeInfo = typeInfo;
    }

    public String getTypeInfo()
    {
        return typeInfo;
    }
    public void setxPoint(String xPoint)
    {
        this.xPoint = xPoint;
    }

    public String getxPoint()
    {
        return xPoint;
    }
    public void setyPoint(String yPoint)
    {
        this.yPoint = yPoint;
    }

    public String getyPoint()
    {
        return yPoint;
    }

    @Override
    public String toString() {
        return new ToStringBuilder(this,ToStringStyle.MULTI_LINE_STYLE)
            .append("id", getId())
            .append("deviceId", getDeviceId())
            .append("type", getType())
            .append("typeInfo", getTypeInfo())
            .append("xPoint", getxPoint())
            .append("yPoint", getyPoint())
            .append("createBy", getCreateBy())
            .append("createTime", getCreateTime())
            .append("updateBy", getUpdateBy())
            .append("updateTime", getUpdateTime())
            .append("remark", getRemark())
            .toString();
    }
}
