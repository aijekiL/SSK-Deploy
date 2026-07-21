package com.ruoyi.project.system.websocket;

import com.alibaba.fastjson2.JSONObject;
import com.ruoyi.common.utils.MediaUtil;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.ArrayDeque;
import java.util.ArrayList;
import java.util.Deque;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.concurrent.ConcurrentHashMap;

/**
 * Aggregates per-device websocket and media health signals into one log line.
 */
public final class CapsNetworkQualityTracker
{
    private static final Logger LOGGER = LoggerFactory.getLogger(CapsNetworkQualityTracker.class);

    private static final long WINDOW_MS = 5 * 60 * 1000L;
    private static final long MEDIA_PROBE_INTERVAL_MS = 10_000L;
    private static final long REPORT_GAP_FAIR_MS = 8_000L;
    private static final long REPORT_GAP_POOR_MS = 15_000L;
    private static final long PUBLISH_TIMEOUT_MS = 10_000L;

    private static final Map<String, DeviceState> DEVICE_STATES = new ConcurrentHashMap<String, DeviceState>();

    private CapsNetworkQualityTracker()
    {
    }

    public static void recordReport(String deviceId, String sessionId, JSONObject payload, WebSocketUsers.PushCmdState pushState)
    {
        if (deviceId == null || deviceId.isEmpty())
        {
            return;
        }

        long now = System.currentTimeMillis();
        DeviceState state = DEVICE_STATES.computeIfAbsent(deviceId, key -> new DeviceState());
        QualitySnapshot snapshot;

        synchronized (state)
        {
            long reportGapMs = state.lastReportTimeMs == 0L ? -1L : now - state.lastReportTimeMs;
            state.lastReportTimeMs = now;

            if (sessionId != null && !sessionId.isEmpty())
            {
                if (state.lastSessionId != null && !state.lastSessionId.equals(sessionId))
                {
                    state.websocketReconnectTimes.addLast(now);
                }
                state.lastSessionId = sessionId;
            }

            trimWindow(state.websocketReconnectTimes, now);
            trimWindow(state.streamInterruptTimes, now);

            if (shouldProbeMedia(state, pushState, now))
            {
                try
                {
                    state.lastStreamMetrics = MediaUtil.getStreamMetrics(deviceId);
                }
                catch (Exception e)
                {
                    LOGGER.warn("网络质量探测媒体流失败: deviceId={}, err={}", deviceId, e.getMessage());
                }
                state.lastMediaProbeTimeMs = now;
            }

            if (pushState != null && pushState.isStreamActive())
            {
                state.lastPublishDelayMs = now - pushState.getCommandTimeMs();
            }

            snapshot = buildSnapshot(deviceId, payload, pushState, state, reportGapMs, now);
        }

        LOGGER.info(
                "网络质量诊断: deviceId={}, sessionId={}, 网络类型={}, 信号强度={}, 在线类型={}, 上报间隔={}, 近5分钟WS重连次数={}, 推流状态={}, 推流等待时长={}, 近5分钟断流次数={}, 是否有流={}, 流存活秒数={}, 字节速率={}, 观看人数={}/{}, 视频轨={}, 音频轨={}, 协议列表={}, 控制链路={}, 推流网络支撑={}, 诊断结论={}, 综合质量={}, 原因={}",
                deviceId,
                sessionId,
                snapshot.netType,
                snapshot.netStrength,
                snapshot.onlineType,
                snapshot.reportGapMs < 0 ? "首次上报" : snapshot.reportGapMs + "ms",
                snapshot.websocketReconnects5m,
                toPushStateText(snapshot.pushState),
                snapshot.publishDelayMs == null ? "无" : snapshot.publishDelayMs + "ms",
                snapshot.streamInterrupts5m,
                yesNo(snapshot.streamPresent),
                snapshot.aliveSecond,
                snapshot.bytesSpeed,
                snapshot.readerCount,
                snapshot.totalReaderCount,
                yesNo(snapshot.hasVideo),
                yesNo(snapshot.hasAudio),
                snapshot.schemas,
                toControlLinkText(snapshot.controlLink),
                toPushNetworkSupportText(snapshot.pushNetworkSupport),
                toDiagnosisText(snapshot.diagnosis),
                toQualityText(snapshot.quality),
                toReasonTexts(snapshot.reasons)
        );
    }

    public static void recordPushStreamActive(String deviceId, long publishDelayMs)
    {
        if (deviceId == null || deviceId.isEmpty())
        {
            return;
        }

        DeviceState state = DEVICE_STATES.computeIfAbsent(deviceId, key -> new DeviceState());
        synchronized (state)
        {
            state.lastPublishDelayMs = publishDelayMs;
        }
    }

    public static void recordStreamInterrupted(String deviceId, String reason)
    {
        if (deviceId == null || deviceId.isEmpty())
        {
            return;
        }

        long now = System.currentTimeMillis();
        DeviceState state = DEVICE_STATES.computeIfAbsent(deviceId, key -> new DeviceState());
        synchronized (state)
        {
            state.streamInterruptTimes.addLast(now);
            trimWindow(state.streamInterruptTimes, now);
        }
        LOGGER.warn("网络质量告警: 设备流中断, deviceId={}, reason={}", deviceId, toReasonText(reason));
    }

    public static void recordWebsocketDisconnect(String deviceId, String reason)
    {
        if (deviceId == null || deviceId.isEmpty())
        {
            return;
        }
        LOGGER.warn("网络质量告警: websocket断开, deviceId={}, reason={}", deviceId, reason);
    }

    private static boolean shouldProbeMedia(DeviceState state, WebSocketUsers.PushCmdState pushState, long now)
    {
        if (state.lastStreamMetrics == null)
        {
            return true;
        }
        if (now - state.lastMediaProbeTimeMs >= MEDIA_PROBE_INTERVAL_MS)
        {
            return true;
        }
        return pushState != null && pushState.isStreamActive() && !state.lastStreamMetrics.isStreamPresent();
    }

    private static QualitySnapshot buildSnapshot(String deviceId, JSONObject payload, WebSocketUsers.PushCmdState pushState,
                                                 DeviceState state, long reportGapMs, long now)
    {
        MediaUtil.StreamMetrics metrics = state.lastStreamMetrics == null ? MediaUtil.StreamMetrics.absent() : state.lastStreamMetrics;

        String netType = mapNetType(payload == null ? null : payload.getString("net_type"));
        String netStrength = payload == null ? "" : payload.getString("net_strenth");
        String onlineType = payload == null ? "" : payload.getString("online_type");
        String pushStatus = resolvePushState(pushState);

        int reconnects5m = state.websocketReconnectTimes.size();
        int streamInterrupts5m = state.streamInterruptTimes.size();
        Long publishDelayMs = pushState == null ? state.lastPublishDelayMs
                : now - pushState.getCommandTimeMs();
        String quality = "GOOD";
        List<String> reasons = new ArrayList<String>();

        Integer netStrengthValue = parseInt(netStrength);
        String controlLink = resolveControlLink(reportGapMs, reconnects5m, netStrengthValue);
        if (netStrengthValue != null && netStrengthValue <= 1)
        {
            quality = downgrade(quality, "POOR");
            reasons.add("weak_signal");
        }
        else if (netStrengthValue != null && netStrengthValue <= 2)
        {
            quality = downgrade(quality, "FAIR");
            reasons.add("medium_signal");
        }

        if (reportGapMs > REPORT_GAP_POOR_MS)
        {
            quality = downgrade(quality, "POOR");
            reasons.add("report_gap_gt_15s");
        }
        else if (reportGapMs > REPORT_GAP_FAIR_MS)
        {
            quality = downgrade(quality, "FAIR");
            reasons.add("report_gap_gt_8s");
        }

        if (reconnects5m >= 3)
        {
            quality = downgrade(quality, "POOR");
            reasons.add("frequent_ws_reconnect");
        }
        else if (reconnects5m > 0)
        {
            quality = downgrade(quality, "FAIR");
            reasons.add("ws_reconnect_detected");
        }

        if (streamInterrupts5m >= 2)
        {
            quality = downgrade(quality, "POOR");
            reasons.add("frequent_stream_interrupt");
        }
        else if (streamInterrupts5m == 1)
        {
            quality = downgrade(quality, "FAIR");
            reasons.add("stream_interrupt_detected");
        }

        if (metrics.isStreamPresent())
        {
            if (!metrics.hasVideo())
            {
                quality = downgrade(quality, "POOR");
                reasons.add("missing_video_track");
            }
            if (!metrics.hasAudio())
            {
                quality = downgrade(quality, "FAIR");
                reasons.add("missing_audio_track");
            }
        }
        else if ("ACTIVE".equals(pushStatus))
        {
            quality = downgrade(quality, "POOR");
            reasons.add("stream_missing_while_active");
        }

        if ("AWAITING_PUBLISH".equals(pushStatus) && publishDelayMs != null && publishDelayMs > PUBLISH_TIMEOUT_MS)
        {
            quality = downgrade(quality, "FAIR");
            reasons.add("publish_delay_gt_10s");
        }

        if (reasons.isEmpty())
        {
            reasons.add("stable");
        }

        String pushNetworkSupport = resolvePushNetworkSupport(controlLink, pushStatus, publishDelayMs, metrics);
        String diagnosis = resolveDiagnosis(controlLink, pushStatus, publishDelayMs, metrics);

        return new QualitySnapshot(
                netType,
                netStrength == null ? "" : netStrength,
                onlineType == null ? "" : onlineType,
                reportGapMs,
                reconnects5m,
                pushStatus,
                publishDelayMs,
                streamInterrupts5m,
                metrics.isStreamPresent(),
                metrics.getAliveSecond(),
                metrics.getBytesSpeed(),
                metrics.getReaderCount(),
                metrics.getTotalReaderCount(),
                metrics.hasVideo(),
                metrics.hasAudio(),
                metrics.getSchemas(),
                controlLink,
                pushNetworkSupport,
                diagnosis,
                quality,
                reasons
        );
    }

    private static String resolvePushState(WebSocketUsers.PushCmdState pushState)
    {
        if (pushState == null)
        {
            return "IDLE";
        }
        if (WebSocketUsers.PushStatus.STREAMING.equals(pushState.getStatus()))
        {
            return "ACTIVE";
        }
        if (WebSocketUsers.PushStatus.LOST_GRACE.equals(pushState.getStatus()))
        {
            return "LOST_GRACE";
        }
        if (WebSocketUsers.PushStatus.FAILED.equals(pushState.getStatus()))
        {
            return "FAILED";
        }
        return "AWAITING_PUBLISH";
    }

    private static void trimWindow(Deque<Long> timestamps, long now)
    {
        while (!timestamps.isEmpty() && now - timestamps.peekFirst() > WINDOW_MS)
        {
            timestamps.removeFirst();
        }
    }

    private static Integer parseInt(String value)
    {
        try
        {
            return value == null || value.isEmpty() ? null : Integer.parseInt(value);
        }
        catch (NumberFormatException e)
        {
            return null;
        }
    }

    private static String mapNetType(String netType)
    {
        if ("1".equals(netType))
        {
            return "4G";
        }
        if ("2".equals(netType))
        {
            return "WIFI";
        }
        return netType == null || netType.isEmpty() ? "UNKNOWN" : netType;
    }

    private static String resolveControlLink(long reportGapMs, int reconnects5m, Integer netStrengthValue)
    {
        if ((netStrengthValue != null && netStrengthValue <= 1)
                || reportGapMs > REPORT_GAP_POOR_MS
                || reconnects5m >= 3)
        {
            return "UNSTABLE";
        }
        if ((netStrengthValue != null && netStrengthValue == 2)
                || reportGapMs > REPORT_GAP_FAIR_MS
                || reconnects5m > 0)
        {
            return "DEGRADED";
        }
        return "STABLE";
    }

    private static String resolvePushNetworkSupport(String controlLink, String pushStatus, Long publishDelayMs,
                                                    MediaUtil.StreamMetrics metrics)
    {
        if (metrics.isStreamPresent() && metrics.hasVideo() && metrics.getBytesSpeed() > 0)
        {
            return "YES";
        }
        if ("UNSTABLE".equals(controlLink))
        {
            return "NO";
        }
        if ("DEGRADED".equals(controlLink)
                && (("AWAITING_PUBLISH".equals(pushStatus) && publishDelayMs != null && publishDelayMs > PUBLISH_TIMEOUT_MS)
                || "ACTIVE".equals(pushStatus)
                || "LOST_GRACE".equals(pushStatus)))
        {
            return "RISKY";
        }
        if ("FAILED".equals(pushStatus))
        {
            return "NO";
        }
        if ("AWAITING_PUBLISH".equals(pushStatus) || "ACTIVE".equals(pushStatus) || "LOST_GRACE".equals(pushStatus))
        {
            return "UNKNOWN";
        }
        return "UNKNOWN";
    }

    private static String resolveDiagnosis(String controlLink, String pushStatus, Long publishDelayMs,
                                           MediaUtil.StreamMetrics metrics)
    {
        if (metrics.isStreamPresent())
        {
            if (metrics.hasVideo() && metrics.hasAudio() && metrics.getBytesSpeed() > 0)
            {
                return "STREAMING_OK";
            }
            if (metrics.hasVideo() && !metrics.hasAudio())
            {
                return "STREAMING_WITHOUT_AUDIO";
            }
            if (!metrics.hasVideo())
            {
                return "STREAMING_WITHOUT_VIDEO";
            }
            return "STREAM_PRESENT_BUT_IDLE";
        }

        if ("ACTIVE".equals(pushStatus) || "LOST_GRACE".equals(pushStatus))
        {
            return "STREAM_DROPPED_AFTER_ACTIVE";
        }

        if ("FAILED".equals(pushStatus))
        {
            return "STREAM_PUSH_FAILED";
        }

        if ("AWAITING_PUBLISH".equals(pushStatus) && publishDelayMs != null && publishDelayMs > PUBLISH_TIMEOUT_MS)
        {
            if ("UNSTABLE".equals(controlLink) || "DEGRADED".equals(controlLink))
            {
                return "NETWORK_NOT_STABLE_FOR_PUSH";
            }
            return "CONTROL_OK_BUT_NO_MEDIA_AFTER_PUSH";
        }

        if ("AWAITING_PUBLISH".equals(pushStatus))
        {
            return "WAITING_DEVICE_TO_PUBLISH";
        }

        return "IDLE";
    }

    private static String downgrade(String current, String target)
    {
        if ("POOR".equals(current) || "POOR".equals(target))
        {
            return "POOR";
        }
        if ("FAIR".equals(current) || "FAIR".equals(target))
        {
            return "FAIR";
        }
        return "GOOD";
    }

    private static String yesNo(boolean value)
    {
        return value ? "是" : "否";
    }

    private static String toPushStateText(String value)
    {
        if ("ACTIVE".equals(value))
        {
            return "推流中";
        }
        if ("AWAITING_PUBLISH".equals(value))
        {
            return "等待设备推流";
        }
        if ("LOST_GRACE".equals(value))
        {
            return "断流宽限期";
        }
        if ("FAILED".equals(value))
        {
            return "推流失败";
        }
        if ("IDLE".equals(value))
        {
            return "空闲";
        }
        return value;
    }

    private static String toControlLinkText(String value)
    {
        if ("STABLE".equals(value))
        {
            return "稳定";
        }
        if ("DEGRADED".equals(value))
        {
            return "轻度退化";
        }
        if ("UNSTABLE".equals(value))
        {
            return "不稳定";
        }
        return value;
    }

    private static String toPushNetworkSupportText(String value)
    {
        if ("YES".equals(value))
        {
            return "支持";
        }
        if ("NO".equals(value))
        {
            return "不支持";
        }
        if ("RISKY".equals(value))
        {
            return "有风险";
        }
        if ("UNKNOWN".equals(value))
        {
            return "待确认";
        }
        return value;
    }

    private static String toDiagnosisText(String value)
    {
        if ("STREAMING_OK".equals(value))
        {
            return "推流正常";
        }
        if ("STREAMING_WITHOUT_AUDIO".equals(value))
        {
            return "有视频无音频";
        }
        if ("STREAMING_WITHOUT_VIDEO".equals(value))
        {
            return "有音频无视频";
        }
        if ("STREAM_PRESENT_BUT_IDLE".equals(value))
        {
            return "已有流但流量空闲";
        }
        if ("STREAM_DROPPED_AFTER_ACTIVE".equals(value))
        {
            return "推流成功后又断流";
        }
        if ("NETWORK_NOT_STABLE_FOR_PUSH".equals(value))
        {
            return "网络条件不足以稳定支撑推流";
        }
        if ("CONTROL_OK_BUT_NO_MEDIA_AFTER_PUSH".equals(value))
        {
            return "控制链路正常但设备未推出媒体流";
        }
        if ("WAITING_DEVICE_TO_PUBLISH".equals(value))
        {
            return "等待设备开始推流";
        }
        if ("STREAM_PUSH_FAILED".equals(value))
        {
            return "本次推流失败";
        }
        if ("IDLE".equals(value))
        {
            return "空闲";
        }
        return value;
    }

    private static String toQualityText(String value)
    {
        if ("GOOD".equals(value))
        {
            return "良好";
        }
        if ("FAIR".equals(value))
        {
            return "一般";
        }
        if ("POOR".equals(value))
        {
            return "较差";
        }
        return value;
    }

    private static String toReasonTexts(List<String> reasons)
    {
        if (reasons == null || reasons.isEmpty())
        {
            return "[]";
        }
        List<String> result = new ArrayList<String>();
        for (String reason : reasons)
        {
            result.add(toReasonText(reason));
        }
        return result.toString();
    }

    private static String toReasonText(String reason)
    {
        if ("weak_signal".equals(reason))
        {
            return "信号弱";
        }
        if ("medium_signal".equals(reason))
        {
            return "信号一般";
        }
        if ("report_gap_gt_15s".equals(reason))
        {
            return "设备上报间隔超过15秒";
        }
        if ("report_gap_gt_8s".equals(reason))
        {
            return "设备上报间隔超过8秒";
        }
        if ("frequent_ws_reconnect".equals(reason))
        {
            return "近5分钟websocket频繁重连";
        }
        if ("ws_reconnect_detected".equals(reason))
        {
            return "检测到websocket重连";
        }
        if ("frequent_stream_interrupt".equals(reason))
        {
            return "近5分钟频繁断流";
        }
        if ("stream_interrupt_detected".equals(reason))
        {
            return "检测到断流";
        }
        if ("missing_video_track".equals(reason))
        {
            return "缺少视频轨";
        }
        if ("missing_audio_track".equals(reason))
        {
            return "缺少音频轨";
        }
        if ("stream_missing_while_active".equals(reason))
        {
            return "推流状态活跃但未检测到流";
        }
        if ("publish_delay_gt_10s".equals(reason))
        {
            return "推流等待超过10秒";
        }
        if ("stable".equals(reason))
        {
            return "整体稳定";
        }
        if ("websocket_session_cleanup".equals(reason))
        {
            return "websocket会话清理导致断流";
        }
        return reason;
    }

    private static final class DeviceState
    {
        private long lastReportTimeMs;
        private long lastMediaProbeTimeMs;
        private String lastSessionId;
        private Long lastPublishDelayMs;
        private MediaUtil.StreamMetrics lastStreamMetrics;
        private final Deque<Long> websocketReconnectTimes = new ArrayDeque<Long>();
        private final Deque<Long> streamInterruptTimes = new ArrayDeque<Long>();
    }

    private static final class QualitySnapshot
    {
        private final String netType;
        private final String netStrength;
        private final String onlineType;
        private final long reportGapMs;
        private final int websocketReconnects5m;
        private final String pushState;
        private final Long publishDelayMs;
        private final int streamInterrupts5m;
        private final boolean streamPresent;
        private final long aliveSecond;
        private final int bytesSpeed;
        private final int readerCount;
        private final int totalReaderCount;
        private final boolean hasVideo;
        private final boolean hasAudio;
        private final Set<String> schemas;
        private final String controlLink;
        private final String pushNetworkSupport;
        private final String diagnosis;
        private final String quality;
        private final List<String> reasons;

        private QualitySnapshot(String netType, String netStrength, String onlineType, long reportGapMs,
                                int websocketReconnects5m, String pushState, Long publishDelayMs,
                                int streamInterrupts5m, boolean streamPresent, long aliveSecond, int bytesSpeed,
                                int readerCount, int totalReaderCount, boolean hasVideo, boolean hasAudio,
                                Set<String> schemas, String controlLink, String pushNetworkSupport,
                                String diagnosis, String quality, List<String> reasons)
        {
            this.netType = netType;
            this.netStrength = netStrength;
            this.onlineType = onlineType;
            this.reportGapMs = reportGapMs;
            this.websocketReconnects5m = websocketReconnects5m;
            this.pushState = pushState;
            this.publishDelayMs = publishDelayMs;
            this.streamInterrupts5m = streamInterrupts5m;
            this.streamPresent = streamPresent;
            this.aliveSecond = aliveSecond;
            this.bytesSpeed = bytesSpeed;
            this.readerCount = readerCount;
            this.totalReaderCount = totalReaderCount;
            this.hasVideo = hasVideo;
            this.hasAudio = hasAudio;
            this.schemas = schemas;
            this.controlLink = controlLink;
            this.pushNetworkSupport = pushNetworkSupport;
            this.diagnosis = diagnosis;
            this.quality = quality;
            this.reasons = reasons;
        }
    }
}
