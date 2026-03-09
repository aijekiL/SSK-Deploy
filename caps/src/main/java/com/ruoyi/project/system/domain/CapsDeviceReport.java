package com.ruoyi.project.system.domain;

import org.apache.commons.lang3.builder.ToStringBuilder;
import org.apache.commons.lang3.builder.ToStringStyle;
import com.ruoyi.framework.aspectj.lang.annotation.Excel;
import com.ruoyi.framework.web.domain.BaseEntity;

/**
 * 安全帽设备实时数据对象 caps_device_report
 *
 * @author ruoyi
 * @date 2025-04-16
 */
public class CapsDeviceReport extends BaseEntity
{
    private static final long serialVersionUID = 1L;

    /** ID */
    private Integer id;

    /** 设备id */
    @Excel(name = "设备id")
    private String deviceId;

    /** 相对开机时高度-米 */
    @Excel(name = "相对开机时高度-米")
    private String altitude;

    /** 当前电量百分比 */
    @Excel(name = "当前电量百分比")
    private String batL;

    /** 当前电压 */
    @Excel(name = "当前电压")
    private String batV;

    /** 蓝牙设备信息 */
    @Excel(name = "蓝牙设备信息")
    private String btSignalInfo;

    /** 速度 */
    @Excel(name = "速度")
    private String cSpeed;

    /** 充电状态 0-正在充电 1-没有充电 */
    @Excel(name = "充电状态 0-正在充电 1-没有充电")
    private String charging;

    /** 定位类型 0-基站定位 1-gps 2-wifi X-其他 */
    @Excel(name = "定位类型 0-基站定位 1-gps 2-wifi X-其他")
    private String gps;

    /** gps信号 0-弱 1-强 -1 无信号 */
    @Excel(name = "gps信号 0-弱 1-强 -1 无信号")
    private String gpsLevel;

    /** 帽子状态 1-在线 2-监控 3-视频通话 */
    @Excel(name = "帽子状态 1-在线 2-监控 3-视频通话")
    private String inUse;

    /** 穿戴状态 默认0 1-未佩戴 2-已佩戴 */
    @Excel(name = "穿戴状态 默认0 1-未佩戴 2-已佩戴")
    private String isWeared;

    /** 网络信号强度 0-5  5最强 */
    @Excel(name = "网络信号强度 0-5  5最强")
    private String netStrenth;

    /** 网络类型 1-4G 2-wifi */
    @Excel(name = "网络类型 1-4G 2-wifi")
    private String netType;

    /** 当前sim卡号 */
    @Excel(name = "当前sim卡号")
    private String phoneNumber;

    /** 当月已用流量 MB */
    @Excel(name = "当月已用流量 MB")
    private String simDataNum;

    /** sim卡监测 0未知 1-有卡  2-无卡 */
    @Excel(name = "sim卡监测 0未知 1-有卡  2-无卡")
    private String simStatus;

    /** 内存卡监测 0位置 1有卡 2无卡 */
    @Excel(name = "内存卡监测 0位置 1有卡 2无卡")
    private String tcardStatus;

    /** 主板温度 - 摄氏度 */
    @Excel(name = "主板温度 - 摄氏度")
    private String tempdata;

    /** 用户id */
    @Excel(name = "用户id")
    private String userId;

    /** 纬度-高德 */
    @Excel(name = "纬度-高德")
    private String xPoint;

    /** 经度-高德 */
    @Excel(name = "经度-高德")
    private String yPoint;

    private String ch4;

    private String o2;

    private String co;

    private String h2s;

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
    public void setAltitude(String altitude)
    {
        this.altitude = altitude;
    }

    public String getAltitude()
    {
        return altitude;
    }
    public void setBatL(String batL)
    {
        this.batL = batL;
    }

    public String getBatL()
    {
        return batL;
    }
    public void setBatV(String batV)
    {
        this.batV = batV;
    }

    public String getBatV()
    {
        return batV;
    }
    public void setBtSignalInfo(String btSignalInfo)
    {
        this.btSignalInfo = btSignalInfo;
    }

    public String getBtSignalInfo()
    {
        return btSignalInfo;
    }
    public void setcSpeed(String cSpeed)
    {
        this.cSpeed = cSpeed;
    }

    public String getcSpeed()
    {
        return cSpeed;
    }
    public void setCharging(String charging)
    {
        this.charging = charging;
    }

    public String getCharging()
    {
        return charging;
    }
    public void setGps(String gps)
    {
        this.gps = gps;
    }

    public String getGps()
    {
        return gps;
    }
    public void setGpsLevel(String gpsLevel)
    {
        this.gpsLevel = gpsLevel;
    }

    public String getGpsLevel()
    {
        return gpsLevel;
    }
    public void setInUse(String inUse)
    {
        this.inUse = inUse;
    }

    public String getInUse()
    {
        return inUse;
    }
    public void setIsWeared(String isWeared)
    {
        this.isWeared = isWeared;
    }

    public String getIsWeared()
    {
        return isWeared;
    }
    public void setNetStrenth(String netStrenth)
    {
        this.netStrenth = netStrenth;
    }

    public String getNetStrenth()
    {
        return netStrenth;
    }
    public void setNetType(String netType)
    {
        this.netType = netType;
    }

    public String getNetType()
    {
        return netType;
    }
    public void setPhoneNumber(String phoneNumber)
    {
        this.phoneNumber = phoneNumber;
    }

    public String getPhoneNumber()
    {
        return phoneNumber;
    }
    public void setSimDataNum(String simDataNum)
    {
        this.simDataNum = simDataNum;
    }

    public String getSimDataNum()
    {
        return simDataNum;
    }
    public void setSimStatus(String simStatus)
    {
        this.simStatus = simStatus;
    }

    public String getSimStatus()
    {
        return simStatus;
    }
    public void setTcardStatus(String tcardStatus)
    {
        this.tcardStatus = tcardStatus;
    }

    public String getTcardStatus()
    {
        return tcardStatus;
    }
    public void setTempdata(String tempdata)
    {
        this.tempdata = tempdata;
    }

    public String getTempdata()
    {
        return tempdata;
    }
    public void setUserId(String userId)
    {
        this.userId = userId;
    }

    public String getUserId()
    {
        return userId;
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

    public String getCh4() {
        return ch4;
    }

    public void setCh4(String ch4) {
        this.ch4 = ch4;
    }

    public String getO2() {
        return o2;
    }

    public void setO2(String o2) {
        this.o2 = o2;
    }

    public String getCo() {
        return co;
    }

    public void setCo(String co) {
        this.co = co;
    }

    public String getH2s() {
        return h2s;
    }

    public void setH2s(String h2s) {
        this.h2s = h2s;
    }

    @Override
    public String toString() {
        return new ToStringBuilder(this,ToStringStyle.MULTI_LINE_STYLE)
            .append("id", getId())
            .append("deviceId", getDeviceId())
            .append("altitude", getAltitude())
            .append("batL", getBatL())
            .append("batV", getBatV())
            .append("btSignalInfo", getBtSignalInfo())
            .append("cSpeed", getcSpeed())
            .append("charging", getCharging())
            .append("gps", getGps())
            .append("gpsLevel", getGpsLevel())
            .append("inUse", getInUse())
            .append("isWeared", getIsWeared())
            .append("netStrenth", getNetStrenth())
            .append("netType", getNetType())
            .append("phoneNumber", getPhoneNumber())
            .append("simDataNum", getSimDataNum())
            .append("simStatus", getSimStatus())
            .append("tcardStatus", getTcardStatus())
            .append("tempdata", getTempdata())
            .append("userId", getUserId())
            .append("xPoint", getxPoint())
            .append("yPoint", getyPoint())
            .append("createBy", getCreateBy())
            .append("createTime", getCreateTime())
            .append("updateBy", getUpdateBy())
            .append("updateTime", getUpdateTime())
            .append("remark", getRemark())
                .append("ch4", getCh4())
                .append("o2", getO2())
                .append("co", getCo())
                .append("h2s", getH2s())
            .toString();
    }
}
