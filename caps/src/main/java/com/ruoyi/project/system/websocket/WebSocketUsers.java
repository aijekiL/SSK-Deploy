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
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Component;

/**
 * websocket client session registry
 */
@Component
public class WebSocketUsers
{
    private static final long PUSH_DETECT_TIMEOUT_MS = 15_000L;

    private static final Logger LOGGER = LoggerFactory.getLogger(WebSocketUsers.class);

    private static final Map<String, Session> USERS = new ConcurrentHashMap<String, Session>();

    private static final Map<String, PushCmdState> PUSH_CMD_STATES = new ConcurrentHashMap<String, PushCmdState>();

    public static final class PushCmdState
    {
        private final String pushUrl;
        private final String sourceRtmpUrl;
        private final long commandTimeMs;
        private final boolean streamActive;

        private PushCmdState(String pushUrl, String sourceRtmpUrl, long commandTimeMs, boolean streamActive)
        {
            this.pushUrl = pushUrl;
            this.sourceRtmpUrl = sourceRtmpUrl;
            this.commandTimeMs = commandTimeMs;
            this.streamActive = streamActive;
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
            return streamActive;
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
        return PUSH_DETECT_TIMEOUT_MS;
    }

    public static String buildPushUrl(String deviceId)
    {
        return CapsConstants.getRtmpAddress() + "/live/" + deviceId;
    }

    public static PushCmdState recordPushCmdState(String deviceId, String pushUrl, String sourceRtmpUrl)
    {
        if (deviceId == null || deviceId.isEmpty())
        {
            return null;
        }

        PushCmdState state = new PushCmdState(pushUrl, sourceRtmpUrl, System.currentTimeMillis(), false);
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

        PushCmdState state = PUSH_CMD_STATES.compute(deviceId, (key, existing) -> {
            String pushUrl = existing == null ? buildPushUrl(deviceId) : existing.getPushUrl();
            String sourceRtmpUrl = existing == null ? pushUrl : existing.getSourceRtmpUrl();
            long commandTimeMs = existing == null ? System.currentTimeMillis() : existing.getCommandTimeMs();
            return new PushCmdState(pushUrl, sourceRtmpUrl, commandTimeMs, true);
        });
        if (state != null)
        {
            CapsNetworkQualityTracker.recordPushStreamActive(deviceId, System.currentTimeMillis() - state.getCommandTimeMs());
        }
        return state;
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

        long elapsedMs = System.currentTimeMillis() - state.getCommandTimeMs();
        LOGGER.info("skip duplicate server_push_open_rtsp: deviceId={}, elapsed={}ms, streamActive={}, pushUrl={}",
                deviceId, elapsedMs, state.isStreamActive(), state.getPushUrl());
        return false;
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
                    if (shouldSendPushCommand(deviceId))
                    {
                        String pushUrl = buildPushUrl(deviceId);
                        JSONObject result = new JSONObject();
                        result.put("cmd", "server_push_open_rtsp");
                        result.put("push_url", pushUrl);
                        LOGGER.info("下发推流命令: deviceId={}, sessionId={}, wsOpen={}, payload={}",
                                deviceId, session.getId(), session.isOpen(), result);
                        session.getBasicRemote().sendText(result.toString());
                        recordPushCmdState(deviceId, pushUrl, pushUrl);
                    }
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
