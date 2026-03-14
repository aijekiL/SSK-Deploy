package com.ruoyi.common.constant;

import lombok.Getter;
import org.springframework.stereotype.Component;

/**
 * 功能描述 设备socket事件?
 *
 * 
 *
 * ACT - 设备发往服务��?
 * CMD - 服务器发往设备
 */
@Getter
@Component
public class CapsConstants {
    // 登录
    public static final String SOCKET_EVENT_LOGIN = "ca_login";
    // 设备上报
    public static final String SOCKET_EVENT_REPORT = "ca_report_location";

//    public static String rtmpIp = "pqyxkjzy.huwork.cn";
    // 服务器地址
    public static String rtmpIp = "192.168.166.210";
    //public static String rtmpIp = "192.168.144.213";
    // sos
    public static final String SOCKET_EVENT_SOS = "ca_sos";
    public static final String SOCKET_EVENT_SIP_SOS = "ca_sip_sos";
    public static final String IP_ADDRESS = "http://"+rtmpIp+":5010";
//    public static final String IP_ADDRESS = "https://"+rtmpIp+":5010";
    public static final String SOCKET_ADDRESS = "ws://"+rtmpIp+":5010";
//    public static final String SOCKET_ADDRESS = "wss://"+rtmpIp+":5010";
    // webrtc视频通话
    public static final String WEBRTC_ADDRESS = "https://"+rtmpIp+":8949";
    //public static final String WEBRTC_ADDRESS = "https://"+rtmpIp+":19443";
    // 流媒体推送
    public static final String RTMP_ADDRESS = "rtmp://"+rtmpIp+":1935";


}
