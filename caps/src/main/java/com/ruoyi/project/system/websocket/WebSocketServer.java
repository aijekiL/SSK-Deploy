package com.ruoyi.project.system.websocket;

import java.util.*;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.Semaphore;
import java.util.concurrent.atomic.AtomicBoolean;
import javax.websocket.OnClose;
import javax.websocket.OnError;
import javax.websocket.OnMessage;
import javax.websocket.OnOpen;
import javax.websocket.Session;
import javax.websocket.server.ServerEndpoint;

import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;
import com.ruoyi.common.constant.CapsConstants;
import com.ruoyi.common.enums.AlarmType;
import com.ruoyi.common.utils.HuaheUtil;
import com.ruoyi.project.system.controller.CapsConfigApi;
import com.ruoyi.project.system.domain.CapsDevice;
import com.ruoyi.project.system.domain.CapsDeviceAlarm;
import com.ruoyi.project.system.service.IAudibleVoiceAlarmService;
import com.ruoyi.project.system.service.ICapsDeviceAlarmService;
import com.ruoyi.project.system.service.ICapsDeviceReportService;
import com.ruoyi.project.system.service.ICapsDeviceService;
import lombok.extern.slf4j.Slf4j;
import org.apache.commons.compress.utils.Lists;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

/**
 * websocket message handler
 */
@Component
@ServerEndpoint("/websocket/message")
@Slf4j
public class WebSocketServer
{
    private static final Logger LOGGER = LoggerFactory.getLogger(WebSocketServer.class);

    public static int socketMaxOnlineCount = 100;

    private static final Semaphore socketSemaphore = new Semaphore(socketMaxOnlineCount);

    private static ICapsDeviceService capsDeviceService;
    private static ICapsDeviceReportService capsDeviceReportService;
    private static ICapsDeviceAlarmService capsDeviceAlarmService;
    private static IAudibleVoiceAlarmService audibleVoiceAlarmService;

    @Autowired
    public void setCapsDeviceService(ICapsDeviceService capsDeviceService)
    {
        WebSocketServer.capsDeviceService = capsDeviceService;
    }

    @Autowired
    public void setCapsDeviceReportService(ICapsDeviceReportService capsDeviceReportService)
    {
        WebSocketServer.capsDeviceReportService = capsDeviceReportService;
    }

    @Autowired
    public void setCapsDeviceAlarmService(ICapsDeviceAlarmService capsDeviceAlarmService)
    {
        WebSocketServer.capsDeviceAlarmService = capsDeviceAlarmService;
    }

    @Autowired
    public void setAudibleVoiceAlarmService(IAudibleVoiceAlarmService audibleVoiceAlarmService)
    {
        WebSocketServer.audibleVoiceAlarmService = audibleVoiceAlarmService;
    }

    @OnOpen
    public void onOpen(Session session) throws Exception
    {
        boolean semaphoreFlag = SemaphoreUtils.tryAcquire(socketSemaphore);
        if (!semaphoreFlag)
        {
            LOGGER.error("拒绝websocket连接: 在线数量超过限制 {}", socketMaxOnlineCount);
            WebSocketUsers.sendMessageToUserByText(session, "当前在线数量超过限制: " + socketMaxOnlineCount);
            session.close();
            return;
        }

        WebSocketUsers.put(session.getId(), session);
        LOGGER.info("websocket已连接: sessionId={}, currentUsers={}", session.getId(), WebSocketUsers.getUsers().size());
    }

    @OnClose
    public void onClose(Session session)
    {
        LOGGER.info("websocket关闭: sessionId={}", session.getId());
        cleanupSession(session, "websocket关闭");
        boolean removeFlag = WebSocketUsers.remove(session.getId());
        if (!removeFlag)
        {
            SemaphoreUtils.release(socketSemaphore);
        }
    }

    @OnError
    public void onError(Session session, Throwable exception) throws Exception
    {
        if (session != null && session.isOpen())
        {
            session.close();
        }
        String sessionId = session == null ? "null" : session.getId();
        LOGGER.error("websocket异常: sessionId={}", sessionId, exception);
        if (session != null)
        {
            cleanupSession(session, "websocket异常");
            WebSocketUsers.remove(sessionId);
        }
        SemaphoreUtils.release(socketSemaphore);
    }

    private static final List<String> WEB_ONE_SESSION_IDS = Lists.newArrayList();

    public static Map<String, Session> sessionMap = new ConcurrentHashMap<String, Session>(10);

    private final AtomicBoolean isRunning = new AtomicBoolean(true);

    @OnMessage
    public void onMessage(String message, Session session)
    {
        LOGGER.info("收到websocket消息: sessionId={}, payload={}", session.getId(), message);
        JSONObject jsonObject = JSONObject.parseObject(message);
        String act = jsonObject.getString("act");
        String event = "";
        String deviceId = "";
        JSONObject result = new JSONObject();

        if (CapsConstants.SOCKET_EVENT_LOGIN.equals(act)) {
            deviceId = jsonObject.getString("device_id");
            Session previousSession = sessionMap.remove(deviceId);
            LOGGER.info("收到设备登录: deviceId={}, sessionId={}, previousSessionId={}, previousSessionOpen={}",
                    deviceId, session.getId(), sessionIdOf(previousSession), previousSession != null && previousSession.isOpen());
            try {
                if (deviceId != null && !deviceId.isEmpty()) {
                    sessionMap.put(deviceId, session);
                    LOGGER.info("设备会话绑定: source={}, deviceId={}, sessionId={}, sessionKeys={}",
                            "登录",
                            deviceId, session.getId(), sessionMap.keySet());
                }
                JSONObject loginData = capsDeviceService.buildLoginResponse(deviceId);
                result.put("msg", "login success");
                result.put("status", true);
                result.put("data", loginData);
                event = CapsConstants.SOCKET_EVENT_LOGIN;
                LOGGER.info("设备登录登记完成: deviceId={}, sessionId={}, sessionKeys={}",
                        deviceId, session.getId(), sessionMap.keySet());
            } catch (Exception e) {
                LOGGER.error("设备登录处理失败: deviceId={}, sessionId={}", deviceId, session.getId(), e);
                result.put("msg", "login failed:" + e.getMessage());
                result.put("status", false);
                result.put("data", "");
            }
            result.put("sip_info", new JSONObject());
            result.put("cmd", "ca_login");
        } else if (CapsConstants.SOCKET_EVENT_REPORT.equals(act)) {
            try {
                deviceId = capsDeviceReportService.parseCapsDeviceReport(jsonObject);
                result.put("cmd", "ca_report_location");
                result.put("status", true);
                result.put("msg", "report success");
                result.put("f_id", "0");
                result.put("f_key", "d751713988987e9331980363e24189ce");
                result.put("bt_beacon", "");
                result.put("report_time", 0);
                event = CapsConstants.SOCKET_EVENT_REPORT;
                if (deviceId != null && !deviceId.isEmpty()) {
                    Session mappedSession = sessionMap.get(deviceId);
                    if (mappedSession == null) {
                        sessionMap.put(deviceId, session);
                        LOGGER.info("设备会话绑定: source={}, deviceId={}, reportSessionId={}, sessionKeys={}",
                                "上报首次发现", deviceId, session.getId(), sessionMap.keySet());
                    } else if (!mappedSession.getId().equals(session.getId())) {
                        LOGGER.warn("设备会话不一致: deviceId={}, reportSessionId={}, mappedSessionId={}, mappedSessionOpen={}, sessionKeys={}",
                                deviceId, session.getId(), mappedSession.getId(), mappedSession.isOpen(), sessionMap.keySet());
                    }
                }
                LOGGER.info("设备上报登记完成: deviceId={}, userId={}, sessionId={}, onlineType={}, inUse={}, jkVideoSize={}, notuploadedVideoCount={}, sessionKeys={}",
                        deviceId,
                        jsonObject.getString("user_id"),
                        session.getId(),
                        jsonObject.getString("online_type"),
                        jsonObject.getString("in_use"),
                        jsonObject.getString("jkVideoSize"),
                        jsonObject.getString("notuploaded_video_count"),
                        sessionMap.keySet());
                CapsNetworkQualityTracker.recordReport(deviceId, session.getId(), jsonObject, WebSocketUsers.getPushCmdState(deviceId));
            } catch (Exception e) {
                LOGGER.error("设备上报处理失败: sessionId={}, payload={}", session.getId(), message, e);
                result.put("cmd", "ca_report_location");
                result.put("msg", "report failed:" + e.getMessage());
                result.put("status", false);
                result.put("data", "");
            }
        } else if (CapsConstants.SOCKET_EVENT_SOS.equals(act) || CapsConstants.SOCKET_EVENT_SIP_SOS.equals(act)) {
            deviceId = jsonObject.getString("device_id");
            Integer type = jsonObject.getInteger("type");
            String xPoint = jsonObject.getString("x_point");
            String yPoint = jsonObject.getString("y_point");
            CapsDeviceAlarm capsDeviceAlarm = new CapsDeviceAlarm();
            capsDeviceAlarm.setDeviceId(deviceId);
            capsDeviceAlarm.setType(type);
            capsDeviceAlarm.setTypeInfo(AlarmType.getValueByCode(type));
            capsDeviceAlarm.setxPoint(xPoint);
            capsDeviceAlarm.setyPoint(yPoint);
            capsDeviceAlarm.setCreateTime(new Date());
            capsDeviceAlarmService.insertCapsDeviceAlarm(capsDeviceAlarm);
            String jsonString = JSONObject.toJSONString(capsDeviceAlarm);
            LOGGER.info("收到设备SOS: deviceId={}, type={}, sessionId={}", deviceId, type, session.getId());
            String personId = null;
            List<String> alarmDeviceCodes = new ArrayList<>();
            CapsDevice capsDevice = capsDeviceService.selectCapsDeviceByDeviceId(deviceId);
            if (capsDevice != null) {
                personId = capsDevice.getPersonId();
                alarmDeviceCodes = audibleVoiceAlarmService.getDeviceIdsByPersonId(personId);
            }

            if (!WEB_ONE_SESSION_IDS.isEmpty() && personId != null) {
                for (String webOneSessionId : WEB_ONE_SESSION_IDS) {
                    if (webOneSessionId.contains(personId)) {
                        Map<String, Session> users = WebSocketUsers.getUsers();
                        if (users != null) {
                            String sessionId = webOneSessionId.substring(0, webOneSessionId.indexOf("-"));
                            Session session1 = users.get(sessionId);
                            if (session1 != null) {
                                WebSocketUsers.sendMessageToUserByText(session1, jsonString);
                            }
                        }
                    }
                }
            }
            result.put("cmd", "ca_sos");
            result.put("status", true);
            result.put("msg", "sos success");
            event = CapsConstants.SOCKET_EVENT_SOS;

            if (type == 0 || type == 4 || type == 13 || type == 15 || type == 7 || type == 19 || type == 23 || type == 41) {
                if (alarmDeviceCodes != null && !alarmDeviceCodes.isEmpty()) {
                    for (String alarmDeviceCode : alarmDeviceCodes) {
                        HuaheUtil.openDevice(alarmDeviceCode);
                    }
                }
            }
        } else if ("web_one_register".equals(act)) {
            String personId = jsonObject.getString("person_id");
            LOGGER.info("网页端注册: personId={}, sessionId={}", personId, session.getId());
            WEB_ONE_SESSION_IDS.add(session.getId() + "-" + personId);
            isRunning.set(true);
        } else if ("web_one_stop".equals(act)) {
            String personId = jsonObject.getString("person_id");
            LOGGER.info("网页端停止监听: personId={}, sessionId={}", personId, session.getId());
            WEB_ONE_SESSION_IDS.removeIf(s -> s.contains(personId));
        } else if ("get_webrtc_video_list".equals(act)) {
            Set<String> dialogMap = CapsConfigApi.dialogMap;
            String roomId = jsonObject.getString("room_id");
            LOGGER.info("收到获取WebRTC视频列表请求: roomId={}, sessionId={}, dialogMap={}",
                    roomId, session.getId(), dialogMap);
            String[] split = roomId.split("-");
            deviceId = split[0];
            String code = split[1];
            String stream = split[2];
            if (getActiveSession(deviceId) == null) {
                LOGGER.warn("获取WebRTC视频列表被拒绝: deviceId={} 未在sessionMap中找到. sessionKeys={}",
                        deviceId, sessionMap.keySet());
                result.put("cmd", "server_push_stop_webrtc_call");
                result.put("status", true);
                result.put("msg", "stop webrtc call");
                result.put("room_id", roomId);
                result.put("msg_code", "StopWebrtcCall");
            } else if (dialogMap.contains(roomId)) {
                result.put("cmd", "get_webrtc_video_list");
                result.put("status", "true");
                result.put("msg", "success");
                result.put("room_id", roomId);

                JSONObject data = new JSONObject();
                data.put("room_id", roomId);
                JSONArray jsonArray = new JSONArray();
                JSONObject memberStarter = new JSONObject();
                memberStarter.put("id", code);
                memberStarter.put("is_caps", "0");
                memberStarter.put("type", "1");
                JSONArray url = new JSONArray();
                url.add(CapsConstants.getWebrtcAddress() + "/index/api/webrtc?app=live&stream=" + stream + "&type=play");
                url.add(CapsConstants.getWebrtcAddress() + "/index/api/webrtc?app=live&stream=" + stream + "&type=push");

                memberStarter.put("rtc_url", url);
                memberStarter.put("status", "1");
                memberStarter.put("stream", stream);
                memberStarter.put("udid", code);
                memberStarter.put("room_id", roomId);

                JSONObject memberDevice = new JSONObject();
                memberDevice.put("id", deviceId);
                memberDevice.put("is_caps", "1");
                memberDevice.put("type", "1");
                JSONArray url2 = new JSONArray();
                url2.add(CapsConstants.getWebrtcAddress() + "/index/api/webrtc?app=live&stream=" + deviceId + "&type=play");
                url2.add(CapsConstants.getWebrtcAddress() + "/index/api/webrtc?app=live&stream=" + deviceId + "&type=push");
                url2.add(CapsConstants.getRtmpAddress() + "/live/" + deviceId);

                memberDevice.put("rtc_url", url2);
                memberDevice.put("user_id", "2");
                memberDevice.put("status", "1");
                memberDevice.put("stream", deviceId);
                memberDevice.put("udid", "2");
                memberDevice.put("room_id", roomId);
                jsonArray.add(memberStarter);
                jsonArray.add(memberDevice);
                data.put("members", jsonArray);
                data.put("type", "1");
                result.put("data", data);
                LOGGER.info("返回WebRTC视频列表: roomId={}, payload={}", roomId, result.toJSONString());
            } else {
                LOGGER.warn("跳过获取WebRTC视频列表: roomId={} 未在dialogMap中找到, dialogMap={}", roomId, dialogMap);
            }
        } else if ("ping".equals(jsonObject.getString("type"))) {
            LOGGER.debug("收到websocket心跳: sessionId={}", session.getId());
        } else {
            LOGGER.info("忽略未识别的websocket消息: sessionId={}, act={}, payload={}",
                    session.getId(), act, message);
        }

        if (result.isEmpty()) {
            return;
        }

        String msg = result.toJSONString();
        if (event != null && !event.isEmpty()) {
            WebSocketUsers.sendMessageToUserByText(session, msg, event, deviceId);
        } else {
            WebSocketUsers.sendMessageToUserByText(session, msg);
        }
    }

    public static Session getActiveSession(String deviceId)
    {
        if (deviceId == null || deviceId.isEmpty())
        {
            return null;
        }

        Session session = sessionMap.get(deviceId);
        if (session == null)
        {
            return null;
        }

        if (session.isOpen())
        {
            LOGGER.debug("已找到可用设备会话: deviceId={}, sessionId={}", deviceId, session.getId());
            return session;
        }

        cleanupDeviceSession(deviceId, "websocket会话已失效");
        return null;
    }

    public static void cleanupSession(Session session, String reason)
    {
        if (session == null)
        {
            return;
        }

        List<String> deviceIds = new ArrayList<>();
        for (Map.Entry<String, Session> entry : sessionMap.entrySet())
        {
            if (session.equals(entry.getValue()))
            {
                deviceIds.add(entry.getKey());
            }
        }

        for (String deviceId : deviceIds)
        {
            cleanupDeviceSession(deviceId, reason);
        }
    }

    public static void cleanupDeviceSession(String deviceId, String reason)
    {
        if (deviceId == null || deviceId.isEmpty())
        {
            return;
        }

        Session removedSession = sessionMap.remove(deviceId);
        boolean dialogRemoved = CapsConfigApi.dialogMap.removeIf(roomId -> roomId.startsWith(deviceId + "-"));
        WebSocketUsers.PushCmdState pushCmdState = WebSocketUsers.getPushCmdState(deviceId);
        if (pushCmdState != null && pushCmdState.isStreamActive())
        {
            CapsNetworkQualityTracker.recordStreamInterrupted(deviceId, "websocket_session_cleanup");
        }
        CapsNetworkQualityTracker.recordWebsocketDisconnect(deviceId, reason);
        WebSocketUsers.clearPushCmdState(deviceId);
        LOGGER.info(
                "设备会话已清理: deviceId={}, reason={}, sessionId={}, dialogRemoved={}, sessionKeys={}, dialogMap={}",
                deviceId,
                reason,
                removedSession == null ? "null" : removedSession.getId(),
                dialogRemoved,
                sessionMap.keySet(),
                CapsConfigApi.dialogMap
        );
    }

    private static String sessionIdOf(Session session)
    {
        return session == null ? "null" : session.getId();
    }
}
