package com.ruoyi.project.system.websocket;

import java.io.IOException;
import java.util.Collection;
import java.util.Map;
import java.util.Set;
import java.util.concurrent.ConcurrentHashMap;
import javax.websocket.Session;

import com.alibaba.fastjson2.JSONObject;
import com.ruoyi.common.constant.CapsConstants;
import com.ruoyi.common.utils.StringUtils;
import org.springframework.beans.factory.annotation.Value;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Component;

/**
 * websocket client session registry
 */
@Component
public class WebSocketUsers
{
    private static long startingGraceMs = 40_000L;
    private static long streamLostGraceMs = 40_000L;
    private static long openRtspMinIntervalMs = 60_000L;
    private static long failedRestartDelayMs = 5_000L;
    private static long autoRestartResetMs = 5 * 60_000L;
    private static int autoRestartLimit = 1;

    private static final Logger LOGGER = LoggerFactory.getLogger(WebSocketUsers.class);

    private static final Map<String, Session> USERS = new ConcurrentHashMap<String, Session>();

    private static final Map<String, PushCmdState> PUSH_CMD_STATES = new ConcurrentHashMap<String, PushCmdState>();

    @Value("${caps.stream.starting-grace-ms:40000}")
    public void setStartingGraceMs(long startingGraceMs)
    {
        WebSocketUsers.startingGraceMs = startingGraceMs;
    }

    @Value("${caps.stream.lost-grace-ms:40000}")
    public void setStreamLostGraceMs(long streamLostGraceMs)
    {
        WebSocketUsers.streamLostGraceMs = streamLostGraceMs;
    }

    @Value("${caps.stream.open-rtsp-min-interval-ms:60000}")
    public void setOpenRtspMinIntervalMs(long openRtspMinIntervalMs)
    {
        WebSocketUsers.openRtspMinIntervalMs = openRtspMinIntervalMs;
    }

    @Value("${caps.stream.failed-restart-delay-ms:5000}")
    public void setFailedRestartDelayMs(long failedRestartDelayMs)
    {
        WebSocketUsers.failedRestartDelayMs = failedRestartDelayMs;
    }

    @Value("${caps.stream.auto-restart-reset-ms:300000}")
    public void setAutoRestartResetMs(long autoRestartResetMs)
    {
        WebSocketUsers.autoRestartResetMs = autoRestartResetMs;
    }

    @Value("${caps.stream.auto-restart-limit:1}")
    public void setAutoRestartLimit(int autoRestartLimit)
    {
        WebSocketUsers.autoRestartLimit = autoRestartLimit;
    }

    public enum PushStatus
    {
        STARTING,
        STREAMING,
        LOST_GRACE,
        FAILED
    }

    public static final class PushCmdState
    {
        private final String pushUrl;
        private final String sourceRtmpUrl;
        private final long commandTimeMs;
        private final PushStatus status;
        private final long lastStreamSeenTimeMs;
        private final long lostSinceTimeMs;
        private final long failedTimeMs;
        private final int autoRestartCount;

        private PushCmdState(String pushUrl, String sourceRtmpUrl, long commandTimeMs, PushStatus status,
                             long lastStreamSeenTimeMs, long lostSinceTimeMs, long failedTimeMs,
                             int autoRestartCount)
        {
            this.pushUrl = pushUrl;
            this.sourceRtmpUrl = sourceRtmpUrl;
            this.commandTimeMs = commandTimeMs;
            this.status = status;
            this.lastStreamSeenTimeMs = lastStreamSeenTimeMs;
            this.lostSinceTimeMs = lostSinceTimeMs;
            this.failedTimeMs = failedTimeMs;
            this.autoRestartCount = autoRestartCount;
        }

        public String getPushUrl()
        {
            return pushUrl;
        }

        public String getSourceRtmpUrl()
        {
            return sourceRtmpUrl;
        }

        public long getCommandTimeMs()
        {
            return commandTimeMs;
        }

        public boolean isStreamActive()
        {
            return PushStatus.STREAMING.equals(status) || PushStatus.LOST_GRACE.equals(status);
        }

        public PushStatus getStatus()
        {
            return status;
        }

        public long getLastStreamSeenTimeMs()
        {
            return lastStreamSeenTimeMs;
        }

        public long getLostSinceTimeMs()
        {
            return lostSinceTimeMs;
        }

        public long getFailedTimeMs()
        {
            return failedTimeMs;
        }

        public int getAutoRestartCount()
        {
            return autoRestartCount;
        }
    }

    public static final class CheckStreamDecision
    {
        private final boolean push;
        private final boolean actualPush;
        private final String state;
        private final String reason;
        private final long graceRemainingMs;

        private CheckStreamDecision(boolean push, boolean actualPush, String state, String reason, long graceRemainingMs)
        {
            this.push = push;
            this.actualPush = actualPush;
            this.state = state;
            this.reason = reason;
            this.graceRemainingMs = graceRemainingMs;
        }

        public boolean isPush()
        {
            return push;
        }

        public boolean isActualPush()
        {
            return actualPush;
        }

        public String getState()
        {
            return state;
        }

        public String getReason()
        {
            return reason;
        }

        public long getGraceRemainingMs()
        {
            return graceRemainingMs;
        }
    }

    public static void put(String key, Session session)
    {
        USERS.put(key, session);
    }

    public static boolean remove(Session session)
    {
        String key = null;
        boolean flag = USERS.containsValue(session);
        if (flag)
        {
            Set<Map.Entry<String, Session>> entries = USERS.entrySet();
            for (Map.Entry<String, Session> entry : entries)
            {
                Session value = entry.getValue();
                if (value.equals(session))
                {
                    key = entry.getKey();
                    break;
                }
            }
        }
        else
        {
            return true;
        }
        return remove(key);
    }

    public static boolean remove(String key)
    {
        LOGGER.info("\n 移除websocket用户 - {}", key);
        Session remove = USERS.remove(key);
        if (remove != null)
        {
            boolean containsValue = USERS.containsValue(remove);
            LOGGER.info("\n 移除结果 - {}", containsValue ? "失败" : "成功");
            return containsValue;
        }
        else
        {
            return true;
        }
    }

    public static Map<String, Session> getUsers()
    {
        return USERS;
    }

    public static long getPushDetectTimeoutMs()
    {
        return startingGraceMs;
    }

    public static String buildPushUrl(String deviceId)
    {
        return CapsConstants.getRtmpAddress() + "/live/" + deviceId;
    }

    public static String buildWebrtcPushUrl(String deviceId)
    {
        return CapsConstants.getWebrtcAddress() + "/index/api/webrtc?app=live&stream=" + deviceId + "&type=push";
    }

    public static PushCmdState recordPushCmdState(String deviceId, String pushUrl, String sourceRtmpUrl)
    {
        if (deviceId == null || deviceId.isEmpty())
        {
            return null;
        }

        PushCmdState state = new PushCmdState(pushUrl, sourceRtmpUrl, System.currentTimeMillis(),
                PushStatus.STARTING, 0L, 0L, 0L, 0);
        PUSH_CMD_STATES.put(deviceId, state);
        LOGGER.info("记录推流命令状态: deviceId={}, pushUrl={}, sourceRtmpUrl={}, commandTimeMs={}",
                deviceId, pushUrl, sourceRtmpUrl, state.getCommandTimeMs());
        return state;
    }

    public static PushCmdState markPushStreamActive(String deviceId)
    {
        if (deviceId == null || deviceId.isEmpty())
        {
            return null;
        }

        long now = System.currentTimeMillis();
        PushCmdState state = PUSH_CMD_STATES.compute(deviceId, (key, existing) -> {
            String pushUrl = existing == null ? buildPushUrl(deviceId) : existing.getPushUrl();
            String sourceRtmpUrl = existing == null ? pushUrl : existing.getSourceRtmpUrl();
            long commandTimeMs = existing == null ? now : existing.getCommandTimeMs();
            int autoRestartCount = existing == null ? 0 : existing.getAutoRestartCount();
            return new PushCmdState(pushUrl, sourceRtmpUrl, commandTimeMs, PushStatus.STREAMING,
                    now, 0L, 0L, autoRestartCount);
        });
        if (state != null)
        {
            CapsNetworkQualityTracker.recordPushStreamActive(deviceId, now - state.getCommandTimeMs());
        }
        return state;
    }

    public static CheckStreamDecision evaluateCheckStream(String deviceId, boolean actualPush)
    {
        long now = System.currentTimeMillis();
        if (actualPush)
        {
            PushCmdState state = markPushStreamActive(deviceId);
            return new CheckStreamDecision(true, true, stateName(state), "stream_present", 0L);
        }

        PushCmdState state = getPushCmdState(deviceId);
        if (state == null)
        {
            LOGGER.info("check_stream no push state: deviceId={}, actualPush=false", deviceId);
            return new CheckStreamDecision(false, false, "IDLE", "no_push_state", 0L);
        }

        if (PushStatus.STARTING.equals(state.getStatus()))
        {
            long elapsedMs = now - state.getCommandTimeMs();
            long remainingMs = startingGraceMs - elapsedMs;
            if (remainingMs > 0)
            {
                LOGGER.info("check_stream keeps device pushing during startup grace: deviceId={}, elapsed={}ms, remaining={}ms",
                        deviceId, elapsedMs, remainingMs);
                return new CheckStreamDecision(true, false, stateName(state), "starting_grace", remainingMs);
            }
            PushCmdState failedState = markPushFailed(deviceId, state, now, "starting_timeout");
            return new CheckStreamDecision(false, false, stateName(failedState), "starting_timeout", 0L);
        }

        if (PushStatus.STREAMING.equals(state.getStatus()))
        {
            PushCmdState lostState = markPushLost(deviceId, state, now);
            return new CheckStreamDecision(true, false, stateName(lostState), "stream_lost_grace", streamLostGraceMs);
        }

        if (PushStatus.LOST_GRACE.equals(state.getStatus()))
        {
            long lostSinceTimeMs = state.getLostSinceTimeMs() == 0L ? now : state.getLostSinceTimeMs();
            long elapsedMs = now - lostSinceTimeMs;
            long remainingMs = streamLostGraceMs - elapsedMs;
            if (remainingMs > 0)
            {
                LOGGER.info("check_stream keeps device pushing during stream lost grace: deviceId={}, elapsed={}ms, remaining={}ms",
                        deviceId, elapsedMs, remainingMs);
                return new CheckStreamDecision(true, false, stateName(state), "stream_lost_grace", remainingMs);
            }
            PushCmdState failedState = markPushFailed(deviceId, state, now, "stream_lost_timeout");
            CapsNetworkQualityTracker.recordStreamInterrupted(deviceId, "stream_lost_after_grace");
            return new CheckStreamDecision(false, false, stateName(failedState), "stream_lost_timeout", 0L);
        }

        return new CheckStreamDecision(false, false, stateName(state), "push_failed", 0L);
    }

    public static PushCmdState getPushCmdState(String deviceId)
    {
        if (deviceId == null || deviceId.isEmpty())
        {
            return null;
        }
        return PUSH_CMD_STATES.get(deviceId);
    }

    public static void clearPushCmdState(String deviceId)
    {
        if (deviceId == null || deviceId.isEmpty())
        {
            return;
        }

        PushCmdState removed = PUSH_CMD_STATES.remove(deviceId);
        if (removed != null)
        {
            LOGGER.info("清除推流命令状态: deviceId={}", deviceId);
        }
    }

    public static boolean shouldSendPushCommand(String deviceId)
    {
        PushCmdState state = getPushCmdState(deviceId);
        if (state == null)
        {
            return true;
        }

        long now = System.currentTimeMillis();
        boolean allowed = canSendPushCommand(state, now);
        if (!allowed)
        {
            LOGGER.info("skip duplicate server_push_open_rtsp: deviceId={}, elapsed={}ms, state={}, autoRestartCount={}, pushUrl={}",
                    deviceId, now - state.getCommandTimeMs(), state.getStatus(), state.getAutoRestartCount(), state.getPushUrl());
        }
        return allowed;
    }

    public static boolean trySendPushOpenRtsp(Session session, String deviceId, String reason) throws IOException
    {
        if (session == null || deviceId == null || deviceId.isEmpty())
        {
            return false;
        }

        long now = System.currentTimeMillis();
        PushCmdState existing = getPushCmdState(deviceId);
        if (!canSendPushCommand(existing, now))
        {
            LOGGER.info("skip server_push_open_rtsp: reason={}, deviceId={}, state={}, elapsed={}ms, autoRestartCount={}",
                    reason,
                    deviceId,
                    stateName(existing),
                    existing == null ? 0L : now - existing.getCommandTimeMs(),
                    existing == null ? 0 : existing.getAutoRestartCount());
            return false;
        }

        String pushUrl = buildPushUrl(deviceId);
        String webrtcPushUrl = buildWebrtcPushUrl(deviceId);
        JSONObject result = new JSONObject();
        result.put("cmd", "server_push_open_rtsp");
        result.put("push_url", pushUrl);
        result.put("webrtc_push_url", webrtcPushUrl);
        LOGGER.info("下发推流命令: reason={}, deviceId={}, sessionId={}, wsOpen={}, payload={}",
                reason, deviceId, session.getId(), session.isOpen(), result);
        session.getBasicRemote().sendText(result.toString());

        int autoRestartCount = nextAutoRestartCount(existing, now);
        PushCmdState state = new PushCmdState(pushUrl, pushUrl, now, PushStatus.STARTING,
                0L, 0L, 0L, autoRestartCount);
        PUSH_CMD_STATES.put(deviceId, state);
        LOGGER.info("记录推流命令状态: deviceId={}, state={}, autoRestartCount={}, commandTimeMs={}",
                deviceId, state.getStatus(), state.getAutoRestartCount(), state.getCommandTimeMs());
        return true;
    }

    private static boolean canSendPushCommand(PushCmdState state, long now)
    {
        if (state == null)
        {
            return true;
        }

        if (!PushStatus.FAILED.equals(state.getStatus()))
        {
            return false;
        }

        if (now - state.getFailedTimeMs() < failedRestartDelayMs)
        {
            return false;
        }

        if (now - state.getCommandTimeMs() < openRtspMinIntervalMs)
        {
            return false;
        }

        return restartCountForWindow(state, now) < autoRestartLimit;
    }

    private static int nextAutoRestartCount(PushCmdState state, long now)
    {
        if (state == null)
        {
            return 0;
        }
        return restartCountForWindow(state, now) + 1;
    }

    private static int restartCountForWindow(PushCmdState state, long now)
    {
        if (state == null)
        {
            return 0;
        }
        if (now - state.getCommandTimeMs() >= autoRestartResetMs)
        {
            return 0;
        }
        return state.getAutoRestartCount();
    }

    private static PushCmdState markPushLost(String deviceId, PushCmdState state, long now)
    {
        PushCmdState newState = new PushCmdState(state.getPushUrl(), state.getSourceRtmpUrl(),
                state.getCommandTimeMs(), PushStatus.LOST_GRACE, state.getLastStreamSeenTimeMs(),
                now, 0L, state.getAutoRestartCount());
        PUSH_CMD_STATES.put(deviceId, newState);
        LOGGER.warn("stream missing, enter grace period: deviceId={}, lastStreamSeenTimeMs={}, graceMs={}",
                deviceId, state.getLastStreamSeenTimeMs(), streamLostGraceMs);
        return newState;
    }

    private static PushCmdState markPushFailed(String deviceId, PushCmdState state, long now, String reason)
    {
        PushCmdState newState = new PushCmdState(state.getPushUrl(), state.getSourceRtmpUrl(),
                state.getCommandTimeMs(), PushStatus.FAILED, state.getLastStreamSeenTimeMs(),
                state.getLostSinceTimeMs(), now, state.getAutoRestartCount());
        PUSH_CMD_STATES.put(deviceId, newState);
        LOGGER.warn("stream push failed: deviceId={}, reason={}, state={}, autoRestartCount={}",
                deviceId, reason, newState.getStatus(), newState.getAutoRestartCount());
        return newState;
    }

    private static String stateName(PushCmdState state)
    {
        if (state == null)
        {
            return "IDLE";
        }
        return state.getStatus().name();
    }

    public static void sendMessageToUsersByText(String message)
    {
        Collection<Session> values = USERS.values();
        for (Session value : values)
        {
            sendMessageToUserByText(value, message);
        }
    }

    public static void sendMessageToUserByText(Session session, String message)
    {
        if (session != null)
        {
            try
            {
                logCommandDispatch(session, message, "direct");
                session.getBasicRemote().sendText(message);
            }
            catch (IOException e)
            {
                LOGGER.error("\n[发送消息失败]", e);
            }
        }
        else
        {
            LOGGER.info("\n[会话离线]");
        }
    }

    public static void sendMessageToUserByText(Session session, String message,
                                               String event, String deviceId)
    {
        if (session != null && event != null && !event.isEmpty())
        {
            try
            {
                if (CapsConstants.SOCKET_EVENT_REPORT.equals(event) && deviceId != null && !deviceId.isEmpty())
                {
                    trySendPushOpenRtsp(session, deviceId, "device_report");
                }
                else
                {
                    logCommandDispatch(session, message, event);
                    session.getBasicRemote().sendText(message);
                }
            }
            catch (IOException e)
            {
                LOGGER.error("\n[发送消息失败]", e);
            }
        }
        else
        {
            LOGGER.info("\n[会话离线]");
        }
    }

    private static void logCommandDispatch(Session session, String message, String source)
    {
        if (session == null || StringUtils.isEmpty(message))
        {
            return;
        }

        try
        {
            JSONObject payload = JSONObject.parseObject(message);
            if (payload == null)
            {
                return;
            }
            String cmd = payload.getString("cmd");
            if (StringUtils.isEmpty(cmd))
            {
                return;
            }
            if (cmd.startsWith("server_push_") || "get_webrtc_video_list".equals(cmd))
            {
                LOGGER.info("websocket命令已发送: source={}, sessionId={}, wsOpen={}, cmd={}, payload={}",
                        "direct".equals(source) ? "直接发送" : source, session.getId(), session.isOpen(), cmd, message);
            }
        }
        catch (Exception ignored)
        {
            // ignore non-json payload
        }
    }
}
