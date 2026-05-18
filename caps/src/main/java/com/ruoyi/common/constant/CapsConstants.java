package com.ruoyi.common.constant;

import javax.annotation.PostConstruct;
import javax.servlet.http.HttpServletRequest;

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
    private static String webrtcAddress;

    @Value("${rtmp.url:" + DEFAULT_RTMP_IP + "}")
    private String configuredRtmpIp;

    @Value("${webrtc.url:}")
    private String configuredWebrtcAddress;

    @PostConstruct
    public void init()
    {
        if (configuredRtmpIp != null && !configuredRtmpIp.trim().isEmpty())
        {
            rtmpIp = configuredRtmpIp.trim();
        }
        if (configuredWebrtcAddress != null && !configuredWebrtcAddress.trim().isEmpty())
        {
            webrtcAddress = trimTrailingSlash(configuredWebrtcAddress.trim());
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

    public static String getIpAddress(HttpServletRequest request)
    {
        return buildBaseAddress(request, false);
    }

    public static String getSocketAddress(HttpServletRequest request)
    {
        return buildBaseAddress(request, true);
    }

    public static String getWebrtcAddress()
    {
        return webrtcAddress == null || webrtcAddress.isEmpty() ? "https://" + rtmpIp + ":8949" : webrtcAddress;
    }

    public static String getRtmpAddress()
    {
        return "rtmp://" + rtmpIp + ":1935";
    }

    private static String buildBaseAddress(HttpServletRequest request, boolean websocket)
    {
        if (request == null)
        {
            return websocket ? getSocketAddress() : getIpAddress();
        }

        String scheme = firstHeaderValue(request, "X-Forwarded-Proto");
        if (scheme == null || scheme.isEmpty())
        {
            scheme = request.getScheme();
        }
        if (websocket)
        {
            scheme = "https".equalsIgnoreCase(scheme) ? "wss" : "ws";
        }

        String host = firstHeaderValue(request, "X-Forwarded-Host");
        if (host == null || host.isEmpty())
        {
            host = request.getHeader("Host");
        }
        if (host == null || host.isEmpty())
        {
            host = request.getServerName();
            int port = request.getServerPort();
            if (shouldAppendPort(scheme, port))
            {
                host = host + ":" + port;
            }
        }

        return scheme + "://" + host;
    }

    private static String firstHeaderValue(HttpServletRequest request, String headerName)
    {
        String value = request.getHeader(headerName);
        if (value == null || value.trim().isEmpty())
        {
            return null;
        }

        int commaIndex = value.indexOf(',');
        return (commaIndex >= 0 ? value.substring(0, commaIndex) : value).trim();
    }

    private static boolean shouldAppendPort(String scheme, int port)
    {
        if (port <= 0)
        {
            return false;
        }
        if (("http".equalsIgnoreCase(scheme) || "ws".equalsIgnoreCase(scheme)) && port == 80)
        {
            return false;
        }
        if (("https".equalsIgnoreCase(scheme) || "wss".equalsIgnoreCase(scheme)) && port == 443)
        {
            return false;
        }
        return true;
    }

    private static String trimTrailingSlash(String value)
    {
        while (value.endsWith("/"))
        {
            value = value.substring(0, value.length() - 1);
        }
        return value;
    }
}
