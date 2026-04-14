package com.ruoyi.project.system.websocket;

import java.io.IOException;
import java.util.Collection;
import java.util.Map;
import java.util.Set;
import java.util.concurrent.ConcurrentHashMap;
import javax.websocket.Session;

import com.alibaba.fastjson2.JSONObject;
import com.ruoyi.common.constant.CapsConstants;
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

    private static String isPush = "not_push";

    public static final class PushCmdState
    {
        private final String pushUrl;
        private final String sourceRtmpUrl;
        private final long commandTimeMs;

        private PushCmdState(String pushUrl, String sourceRtmpUrl, long commandTimeMs)
        {
            this.pushUrl = pushUrl;
            this.sourceRtmpUrl = sourceRtmpUrl;
            this.commandTimeMs = commandTimeMs;
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
        LOGGER.info("\n removing websocket user - {}", key);
        Session remove = USERS.remove(key);
        if (remove != null)
        {
            boolean containsValue = USERS.containsValue(remove);
            LOGGER.info("\n remove result - {}", containsValue ? "failed" : "success");
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

        PushCmdState state = PUSH_CMD_STATES.compute(deviceId, (key, existing) -> {
            if (existing != null
                    && pushUrl.equals(existing.getPushUrl())
                    && sourceRtmpUrl.equals(existing.getSourceRtmpUrl()))
            {
                return existing;
            }
            return new PushCmdState(pushUrl, sourceRtmpUrl, System.currentTimeMillis());
        });
        LOGGER.info("send server_push_open_rtsp: deviceId={}, pushUrl={}, sourceRtmpUrl={}",
                deviceId, pushUrl, sourceRtmpUrl);
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
            LOGGER.info("push command state cleared: deviceId={}", deviceId);
        }
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
                session.getBasicRemote().sendText(message);
            }
            catch (IOException e)
            {
                LOGGER.error("\n[send message error]", e);
            }
        }
        else
        {
            LOGGER.info("\n[session offline]");
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
                    if (isPush.equals("not_push"))
                    {
                        String pushUrl = buildPushUrl(deviceId);
                        JSONObject result = new JSONObject();
                        result.put("cmd", "server_push_open_rtsp");
                        result.put("push_url", pushUrl);
                        recordPushCmdState(deviceId, pushUrl, pushUrl);
                        sendMessageToUserByText(session, result.toString());
                    }
                }
                else
                {
                    session.getBasicRemote().sendText(message);
                }
            }
            catch (IOException e)
            {
                LOGGER.error("\n[send message error]", e);
            }
        }
        else
        {
            LOGGER.info("\n[session offline]");
        }
    }
}
