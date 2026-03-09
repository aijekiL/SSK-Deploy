package com.ruoyi.project.system.websocket;

import java.io.IOException;
import java.util.Collection;
import java.util.HashMap;
import java.util.Map;
import java.util.Set;
import java.util.concurrent.ConcurrentHashMap;
import javax.annotation.PostConstruct;
import javax.websocket.Session;

import com.alibaba.fastjson2.JSONObject;
import com.ruoyi.common.constant.CapsConstants;
import com.ruoyi.project.system.controller.CapsConfigApi;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;

/**
 * websocket 客户端用户集
 *
 * @author ruoyi
 */
@Component
public class WebSocketUsers
{
    /**
     * WebSocketUsers 日志控制器
     */
    private static final Logger LOGGER = LoggerFactory.getLogger(WebSocketUsers.class);

    /**
     * 用户集
     */
    private static Map<String, Session> USERS = new ConcurrentHashMap<String, Session>();

    /**
     * 存储用户
     *
     * @param key 唯一键
     * @param session 用户信息
     */
    public static void put(String key, Session session)
    {
        USERS.put(key, session);
    }

    @Value("${rtmp.url}")
    private String rtmpUrl;

    public static String staticRtmpUrl;

    @PostConstruct
    public void init() {
        staticRtmpUrl = rtmpUrl; // 初始化静态字段
    }
    /**
     * 移除用户
     *
     * @param session 用户信息
     *
     * @return 移除结果
     */
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

    /**
     * 移出用户
     *
     * @param key 键
     */
    public static boolean remove(String key)
    {
        LOGGER.info("\n 正在移出用户 - {}", key);
        Session remove = USERS.remove(key);
        if (remove != null)
        {
            boolean containsValue = USERS.containsValue(remove);
            LOGGER.info("\n 移出结果 - {}", containsValue ? "失败" : "成功");
            return containsValue;
        }
        else
        {
            return true;
        }
    }

    /**
     * 获取在线用户列表
     *
     * @return 返回用户集合
     */
    public static Map<String, Session> getUsers()
    {
        return USERS;
    }

    /**
     * 群发消息文本消息
     *
     * @param message 消息内容
     */
    public static void sendMessageToUsersByText(String message)
    {
        Collection<Session> values = USERS.values();
        for (Session value : values)
        {
            sendMessageToUserByText(value, message);
        }
    }

    /**
     * 发送文本消息
     *
     * @param session 自己的用户名
     * @param message 消息内容
     */
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
                LOGGER.error("\n[发送消息异常]", e);
            }
        }
        else
        {
            LOGGER.info("\n[你已离线]");
        }
    }
    private static String isPush = "not_push";



    public static void sendMessageToUserByText(Session session, String message,
                                               String event, String deviceId)
    {
        if (session != null && event != null && !event.isEmpty())
        {
            try
            {
                if(CapsConstants.SOCKET_EVENT_REPORT.equals(event)&& deviceId != null && !deviceId.isEmpty()){
//                    System.out.println("-------------isPush:"+isPush);
                    if(isPush.equals("not_push")){
                        JSONObject result = new JSONObject();
                        result.put("cmd", "server_push_open_rtsp");
                        result.put("push_url", CapsConstants.RTMP_ADDRESS+"/live/"+deviceId);
                        //上报数据后 发送指令给设备开启推流
                        sendMessageToUserByText(session, result.toString());
//                        System.out.println("下发推流指令----------------");
                        //isPush = "push_open";
                    }
                }else{
                    session.getBasicRemote().sendText(message);
                }
            }
            catch (IOException e)
            {
                LOGGER.error("\n[发送消息异常]", e);
            }
        }
        else
        {
            LOGGER.info("\n[你已离线]");
        }
    }
}
