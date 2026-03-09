package com.ruoyi.project.system.domain;

import io.swagger.annotations.ApiModel;
import io.swagger.annotations.ApiModelProperty;
import org.apache.commons.lang3.builder.ToStringBuilder;
import org.apache.commons.lang3.builder.ToStringStyle;
import com.ruoyi.framework.aspectj.lang.annotation.Excel;
import com.ruoyi.framework.web.domain.BaseEntity;

/**
 * 安全帽设备信息对象 caps_device
 *
 * @author ruoyi
 * @date 2025-04-15
 */
@ApiModel(value = "安全帽设备信息对象 caps_device")
public class CapsDevice extends BaseEntity
{
    private static final long serialVersionUID = 1L;

    /** ID */
    private Integer id;

    /** 设备id */
    @ApiModelProperty( value = "设备id")
    @Excel(name = "设备id")
    private String deviceId;

    @ApiModelProperty( value = "客户id")
    @Excel(name = "客户id")
    private String personId;

    /** 用户id */
    @Excel(name = "用户id")
    @ApiModelProperty( value = "用户id")
    private String userId;

    /** 用户名 */
    @Excel(name = "用户名")
    @ApiModelProperty( value = "用户名")
    private String userName;

    /** 手机号 */
    @Excel(name = "手机号")
    @ApiModelProperty( value = "手机号")
    private String mobile;

    private CapsDeviceReport capsDeviceReport;

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

    public String getPersonId() {
        return personId;
    }

    public void setPersonId(String personId) {
        this.personId = personId;
    }

    public void setUserId(String userId)
    {
        this.userId = userId;
    }

    public String getUserId()
    {
        return userId;
    }
    public void setUserName(String userName)
    {
        this.userName = userName;
    }

    public String getUserName()
    {
        return userName;
    }
    public void setMobile(String mobile)
    {
        this.mobile = mobile;
    }

    public String getMobile()
    {
        return mobile;
    }

    public CapsDeviceReport getCapsDeviceReport() {
        return capsDeviceReport;
    }

    public void setCapsDeviceReport(CapsDeviceReport capsDeviceReport) {
        this.capsDeviceReport = capsDeviceReport;
    }
    @Override
    public String toString() {
        return new ToStringBuilder(this,ToStringStyle.MULTI_LINE_STYLE)
            .append("id", getId())
            .append("deviceId", getDeviceId())
            .append("userId", getUserId())
            .append("userName", getUserName())
            .append("mobile", getMobile())
            .append("createBy", getCreateBy())
            .append("createTime", getCreateTime())
            .append("updateBy", getUpdateBy())
            .append("updateTime", getUpdateTime())
            .append("remark", getRemark())
            .toString();
    }
}
