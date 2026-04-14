package com.ruoyi.common.constant;

import javax.annotation.PostConstruct;

import lombok.Getter;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;

@Getter
@Component
public class CapsConstants
{
    private static final String DEFAULT_RTMP_IP = "192.168.166.210";

    public static final String SOCKET_EVENT_LOGIN = "ca_login";
    public static final String SOCKET_EVENT_REPORT = "ca_report_location";

    public static final String GAS_DICT_O2 = "a19001";
    public static final String GAS_DICT_CO = "a21005";
    public static final String GAS_DICT_H2S = "a21028";
    public static final String GAS_DICT_CH4 = "qt00001";

    public static final String SOCKET_EVENT_SOS = "ca_sos";
    public static final String SOCKET_EVENT_SIP_SOS = "ca_sip_sos";

    private static String rtmpIp = DEFAULT_RTMP_IP;

    @Value("${rtmp.url:" + DEFAULT_RTMP_IP + "}")
    private String configuredRtmpIp;

    @PostConstruct
    public void init()
    {
        if (configuredRtmpIp != null && !configuredRtmpIp.trim().isEmpty())
        {
            rtmpIp = configuredRtmpIp.trim();
        }
    }

    public static String getRtmpIp()
    {
        return rtmpIp;
    }

    public static String getIpAddress()
    {
        return "http://" + rtmpIp + ":5010";
    }

    public static String getSocketAddress()
    {
        return "ws://" + rtmpIp + ":5010";
    }

    public static String getWebrtcAddress()
    {
        return "https://" + rtmpIp + ":8949";
    }

    public static String getRtmpAddress()
    {
        return "rtmp://" + rtmpIp + ":1935";
    }
}
