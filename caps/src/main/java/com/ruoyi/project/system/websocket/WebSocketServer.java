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
import com.ruoyi.common.utils.SpringContextHolder;
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
import org.springframework.stereotype.Component;

/**
 * websocket 消息处理
 *
 * @author ruoyi
 */
@Component
@ServerEndpoint("/websocket/message")
@Slf4j
public class WebSocketServer
{
    /**
     * WebSocketServer 日志控制器
     */
    private static final Logger LOGGER = LoggerFactory.getLogger(WebSocketServer.class);

    /**
     * 默认最多允许同时在线人数100
     */
    public static int socketMaxOnlineCount = 100;

    private static Semaphore socketSemaphore = new Semaphore(socketMaxOnlineCount);

    private ICapsDeviceService capsDeviceService;
    private ICapsDeviceReportService capsDeviceReportService;
    private ICapsDeviceAlarmService capsDeviceAlarmService;

    private IAudibleVoiceAlarmService audibleVoiceAlarmService;
    public WebSocketServer() {
        // 手动从 Spring 上下文中获取 ICapsDeviceService 实例
        this.capsDeviceService = SpringContextHolder.getBean(ICapsDeviceService.class);
        this.capsDeviceReportService = SpringContextHolder.getBean(ICapsDeviceReportService.class);
        this.capsDeviceAlarmService = SpringContextHolder.getBean(ICapsDeviceAlarmService.class);
        this.audibleVoiceAlarmService = SpringContextHolder.getBean(IAudibleVoiceAlarmService.class);
    }

    /**
     * 连接建立成功调用的方法
     */
    @OnOpen
    public void onOpen(Session session) throws Exception
    {
        boolean semaphoreFlag = false;
        // 尝试获取信号量
        semaphoreFlag = SemaphoreUtils.tryAcquire(socketSemaphore);
        if (!semaphoreFlag)
        {
            // 未获取到信号量
            LOGGER.error("\n 当前在线人数超过限制数- {}", socketMaxOnlineCount);
            WebSocketUsers.sendMessageToUserByText(session, "当前在线人数超过限制数：" + socketMaxOnlineCount);
            session.close();
        }
        else
        {
            // 添加用户
            WebSocketUsers.put(session.getId(), session);
            LOGGER.info("\n 建立连接 - {}", session);
            LOGGER.info("\n 当前人数 - {}", WebSocketUsers.getUsers().size());
            WebSocketUsers.sendMessageToUserByText(session, "连接成功");
        }
    }

    /**
     * 连接关闭时处理
     */
    @OnClose
    public void onClose(Session session)
    {
        LOGGER.info("\n 关闭连接 - {}", session);
        // 移除用户
        boolean removeFlag = WebSocketUsers.remove(session.getId());
        if (!removeFlag)
        {
            // 获取到信号量则需释放
            SemaphoreUtils.release(socketSemaphore);
        }
    }

    /**
     * 抛出异常时处理
     */
    @OnError
    public void onError(Session session, Throwable exception) throws Exception
    {
        if (session.isOpen())
        {
            // 关闭连接
            session.close();
        }
        String sessionId = session.getId();
        LOGGER.info("\n 连接异常 - {}", sessionId);
        LOGGER.info("\n 异常信息 - {}", exception);
        // 移出用户
        WebSocketUsers.remove(sessionId);
        // 获取到信号量则需释放
        SemaphoreUtils.release(socketSemaphore);
    }

    private static List<String> WEB_ONE_SESSION_IDS = Lists.newArrayList();

    public static Map<String, Session> sessionMap = new ConcurrentHashMap<>(10);

    private final AtomicBoolean isRunning = new AtomicBoolean(true);
    /**
     * 服务器接收到客户端消息时调用的方法
     */
    @OnMessage
    public void onMessage(String message, Session session)
    {
        //解析消息
        LOGGER.info("\n 接收到客户端消息 - {}", message);
        JSONObject jsonObject = JSONObject.parseObject(message);
        String act = jsonObject.getString("act");
        String event = "";
        String deviceId = "";
        JSONObject result = new JSONObject();
        if(CapsConstants.SOCKET_EVENT_LOGIN.equals(act)){
            // 登录

            deviceId = jsonObject.getString("device_id");
            //初始登录时根据deviceId移除map中已经存在的session
            sessionMap.remove(deviceId);
            try{
                JSONObject jsonObject1 = capsDeviceService.buildLoginResponse(deviceId);

                result.put("msg", "登陆成功");
                result.put("status", true);
                result.put("data", jsonObject1);
                event = CapsConstants.SOCKET_EVENT_LOGIN;
            }catch(Exception e){
                result.put("msg", "登录失败:"+e.getMessage());
                result.put("status", false);
                result.put("data", "");
            }
            result.put("sip_info", new JSONObject());
            result.put("cmd", "ca_login");
        } else if(CapsConstants.SOCKET_EVENT_REPORT.equals(act)){
            // 实时数据
            try{
                deviceId = capsDeviceReportService.parseCapsDeviceReport(jsonObject);
                if (deviceId == null || deviceId.trim().isEmpty()) {
                    log.warn("report failed: cannot map device, user_id={}, payload.device_id={}",
                            jsonObject.getString("user_id"), jsonObject.getString("device_id"));
                    result.put("cmd", "ca_report_location");
                    result.put("msg", "report failed: cannot map device");
                    result.put("status", false);
                    result.put("data", "");
                } else {
                    result.put("cmd", "ca_report_location");
                    result.put("status", true);
                    result.put("msg", "report success");
                    result.put("f_id", "0");
                    result.put("f_key", "d751713988987e9331980363e24189ce");
                    result.put("bt_beacon", "");
                    result.put("report_time", 0);
                    event = CapsConstants.SOCKET_EVENT_REPORT;
                    // save device session for call control
                    if(!sessionMap.containsKey(deviceId)){
                        System.out.println("save device call session");
                        sessionMap.put(deviceId, session);
                    }
                }
            }catch(Exception e){
                log.error("report failed, payload={}", jsonObject, e);
                String errMsg = e.getMessage() == null ? e.getClass().getSimpleName() : e.getMessage();
                result.put("cmd", "ca_report_location");
                result.put("msg", "report failed:"+errMsg);
                result.put("status", false);
                result.put("data", "");
            }
        } else if(CapsConstants.SOCKET_EVENT_SOS.equals(act) || CapsConstants.SOCKET_EVENT_SIP_SOS.equals(act)){
            // 无线声光报警
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
            //capsDeviceAlarm 转换为jsonString
            String jsonString = JSONObject.toJSONString(capsDeviceAlarm);
            System.out.println("SOS报警信息:"+jsonString);
            //根据deviceId获取对应的personId和声光报警设备编码
            String personId = null;
            List<String> alarmDeviceCodes = new ArrayList<>();
            CapsDevice capsDevice = capsDeviceService.selectCapsDeviceByDeviceId(deviceId);
            if(capsDevice != null){
                personId = capsDevice.getPersonId();
                alarmDeviceCodes = audibleVoiceAlarmService.getDeviceIdsByPersonId(personId);
            }


            if(!WEB_ONE_SESSION_IDS.isEmpty() && personId != null){
                for (String WEB_ONE_SESSION_ID : WEB_ONE_SESSION_IDS) {
                    //判断 WEB_ONE_SESSION_ID 是否包含personId
                    if(WEB_ONE_SESSION_ID.contains(personId)){
                        Map<String, Session> users = WebSocketUsers.getUsers();
                        if(users != null){
                            //从 WEB_ONE_SESSION_ID 提取出session_id
                            String session_id = WEB_ONE_SESSION_ID.substring(0,WEB_ONE_SESSION_ID.indexOf("-"));
                            Session session1 = users.get(session_id);
                            if(session1 != null){
                                WebSocketUsers.sendMessageToUserByText(session1, jsonString);
                            }
                        }
                    }
                }
            }
            result.put("cmd", "ca_sos");
            result.put("status", true);
            result.put("msg", "报警成功");
            event = CapsConstants.SOCKET_EVENT_SOS;

            //发起声光报警设备请求 当type为 0.手动报警  4跌落  13氧气-过低，15氧气-过高 7.SOS紧急报警 19一氧化碳报警  23硫化氢报警 41苯浓度超标
            if(type == 0 || type == 4 || type == 13 || type == 15 || type == 7 || type == 19 || type == 23 || type == 41){
                if(alarmDeviceCodes != null && !alarmDeviceCodes.isEmpty()){
                    for (String alarmDeviceCode : alarmDeviceCodes) {
                        HuaheUtil.openDevice(alarmDeviceCode);
                    }
                }
            }
            //todo 手动sos需返回room_id
        } else if("web_one_register".equals(act)){
            System.out.println("web_one_register:"+session.getId());
            String personId = jsonObject.getString("person_id");

            WEB_ONE_SESSION_IDS.add(session.getId()+"-"+personId);
            isRunning.set(true);
        } else if("web_one_stop".equals(act)){
            String personId = jsonObject.getString("person_id");
            //清空集合 中包含personId的对象
            WEB_ONE_SESSION_IDS.removeIf(s -> s.contains(personId));
//            WEB_ONE_SESSION_IDS.clear();
        } else if("get_webrtc_video_list".equals(act)){
            //从dialogMap中获取信息，判断设备是否在通话中

            Set<String> dialogMap = CapsConfigApi.dialogMap;
            System.out.println("dialogMap---check-----:"+dialogMap);
            String roomId = jsonObject.getString("room_id");
            //从map中获取code以及stream
            String[] split = roomId.split("-");
            deviceId = split[0];
            String code = split[1];
            String stream = split[2];
            // 判断sessionMap 有没有设备信息
            if(!sessionMap.containsKey(deviceId)){
                result.put("cmd", "server_push_stop_webrtc_call");
                result.put("status", true);
                result.put("msg", "停止webrtc通话");
                result.put("room_id", roomId);
                result.put("msg_code", "StopWebrtcCall");
                String msg = result.toJSONString();
            }else{
                if(dialogMap.contains(roomId)){
                    //从map中获取code以及stream
                    //返回房间的member信息
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
                    url.add(CapsConstants.WEBRTC_ADDRESS+"/index/api/webrtc?app=live&stream="+ stream +"&type=play");
                    url.add(CapsConstants.WEBRTC_ADDRESS+"/index/api/webrtc?app=live&stream="+ stream +"&type=push");

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
                    url2.add(CapsConstants.WEBRTC_ADDRESS+"/index/api/webrtc?app=live&stream="+deviceId+"&type=play");
                    url2.add(CapsConstants.WEBRTC_ADDRESS+"/index/api/webrtc?app=live&stream="+deviceId+"&type=push");
                    url2.add(CapsConstants.RTMP_ADDRESS+"/live/"+deviceId);

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
                    System.out.println("get_webrtc_video_list----:"+result.toJSONString());
                }
            }
        }
        String msg = result.toJSONString();
        WebSocketUsers.sendMessageToUserByText(session, msg, event, deviceId);
    }
}
