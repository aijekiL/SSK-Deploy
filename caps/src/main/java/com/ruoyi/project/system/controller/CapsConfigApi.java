package com.ruoyi.project.system.controller;


import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;
import com.ruoyi.common.constant.CapsConstants;
import com.ruoyi.common.utils.HuaheUtil;
import com.ruoyi.common.utils.MediaUtil;
import com.ruoyi.framework.web.domain.AjaxResultNew;
import com.ruoyi.project.system.domain.CapsDevice;
import com.ruoyi.project.system.service.IAudibleVoiceAlarmService;
import com.ruoyi.project.system.service.ICapsDeviceService;
import com.ruoyi.project.system.websocket.WebSocketServer;
import com.ruoyi.project.system.websocket.WebSocketUsers;

import lombok.extern.slf4j.Slf4j;

import org.springframework.web.bind.annotation.*;

import javax.annotation.Resource;
import javax.servlet.http.HttpServletRequest;
import javax.websocket.Session;

import java.util.*;


/**
 * 功能描述
 *
 * @author: gzh
 * @date: 2025年4月1日11:04
 */
@RestController
@RequestMapping("caps/api")
@Slf4j
public class CapsConfigApi {

    private static final String CAPS_API_URL = CapsConstants.IP_ADDRESS + "/api";
    private static final String CAPS_WS_URL = CapsConstants.SOCKET_ADDRESS + "/websocket/message";

    @Resource
    private ICapsDeviceService capsDeviceService;

    @Resource
    private IAudibleVoiceAlarmService audioAlarmService;
    /**
     * ctl=config&act=get_config
     * @param device_id
     * @param ca_ver
     * @return
     */
    @GetMapping("/index.php")
    public AjaxResultNew getDeviceConfig(HttpServletRequest request
            , String device_id, String ca_ver) {
        log.info("url:{}", request.getRequestURL());
        //打印request中的参数
        log.info("param:{}", request.getQueryString());

        JSONObject jsonObject = new JSONObject();
        jsonObject.put("account_id", "11444");

        // 返回json格式数据config
        jsonObject.put("config", getConfig().toJSONString());
        // 返回json格式数据notice_switch
        jsonObject.put("notice_switch", getNoticeSwitch().toJSONString());
        jsonObject.put("ai_broadcast", "[]");
        // 返回json格式数据upgrade
        jsonObject.put("upgrade", getUpGradeJSON());
        // 返回json格式数据fota_upgrade
        jsonObject.put("fota_upgrade", getFotaUpgradeJSON());
        // 返回json格式数据model_switch
        jsonObject.put("model_switch", getModelSwitchJSON().toJSONString());

        //设备语音升级包md5
        jsonObject.put("voice_pkg_md5", "");
        // 设备语音升级包下载地址
        jsonObject.put("pkg_url", "");
        // 低端帽子上报定位时间间隔
        jsonObject.put("low_report_time", 450);
        // 高端设备心跳时间间隔 
        jsonObject.put("ca_report_time", 0);

        // 返回json格式数据extra
        jsonObject.put("extra", getExtraJSON());

        // 挂钩mac地址
        jsonObject.put("gj_ble_mac", "123");
        // 当前秒级时间戳，设备时间校准
        Long time = System.currentTimeMillis() / 1000;
        jsonObject.put("time", time);

        return AjaxResultNew.success("获取成功", jsonObject);
    }

    @GetMapping("/openHuaheDevice/{personId}")
    public AjaxResultNew openHuaheDevice(@PathVariable("personId") String personId) {
        List<String> deviceIdsByPersonId = audioAlarmService.getDeviceIdsByPersonId(personId);
        if(deviceIdsByPersonId != null && !deviceIdsByPersonId.isEmpty()){
            for (String deviceId : deviceIdsByPersonId) {
                HuaheUtil.openDevice(deviceId);
            }
        }
        return AjaxResultNew.success("成功");
    }

    @GetMapping("/closeHuaheDevice/{personId}")
    public AjaxResultNew closeHuaheDevice(@PathVariable("personId") String personId) {
        List<String> deviceIdsByPersonId = audioAlarmService.getDeviceIdsByPersonId(personId);
        if(deviceIdsByPersonId != null && !deviceIdsByPersonId.isEmpty()){
            for (String deviceId : deviceIdsByPersonId) {
                HuaheUtil.closeDevice(deviceId);
            }
        }
        return AjaxResultNew.success("成功");
    }

    @PostMapping("/index.php")
    public AjaxResultNew getDeviceConfig(HttpServletRequest request
            , String ca_ver, String act, String ctl,String user_id) {
        if("device".equals(ctl) && "check_stream".equals(act)){
            CapsDevice capsDevice = capsDeviceService.selectCapsDeviceByUserId(user_id);
            if(capsDevice != null){
                JSONObject jsonObject = new JSONObject();
                String device_id = capsDevice.getDeviceId();
                //查询流媒体，判断设备是否成功推流
                jsonObject.put("push", MediaUtil.getMediaList(device_id));
                return AjaxResultNew.success("正在推流", jsonObject);
            }

        }
        return AjaxResultNew.success("获取成功", "");
    }


    //保存正在通话的map信息
    public static Set<String> dialogMap = new HashSet<>();
    /**
     * 开启对�?
     * stream 服务端推流stream
     * code 通话�?
     * deviceId 设备id
     */
    @PostMapping("/openDialog")
    public AjaxResultNew openDialog(@RequestParam(value = "stream") String stream,
                                 @RequestParam(value = "code") String code,
                                 @RequestParam(value = "deviceId") String deviceId) {

        //判断deviceId 在不在sessionMap�?
        Map<String, Session> sessionMap = WebSocketServer.sessionMap;
        Session targetSession = sessionMap.get(deviceId);
        if (targetSession == null || !targetSession.isOpen()) {
            return AjaxResultNew.error("设备未连接");
        }
        System.out.println("sessionMap:"+sessionMap);
        System.out.println("dialogMap---current--:"+dialogMap);
        String roomId = deviceId + "-" + code + "-" + stream;


        WebSocketServer.sessionMap.forEach((key, session) -> {
            //根据deviceID获取session
            if (session != null && session.isOpen() && key.equals(deviceId)) {

                JSONObject jsonObject = new JSONObject();
                jsonObject.put("cmd", "server_push_webrtc_call");
                jsonObject.put("room_id", roomId);

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
                memberDevice.put("status", "1");
                memberDevice.put("user_id", "2");
                memberDevice.put("stream", deviceId);
                memberDevice.put("udid", code);
                memberDevice.put("room_id", roomId);
                jsonArray.add(memberStarter);
                jsonArray.add(memberDevice);
                jsonObject.put("members", jsonArray);
                jsonObject.put("type", "1");
                jsonObject.put("status", "true");
                jsonObject.put("msg", "开启单人通话");
                jsonObject.put("msg_code", jsonArray);
                String msg = jsonObject.toJSONString();
                System.out.println("msg:"+msg);
                WebSocketUsers.sendMessageToUserByText(targetSession,msg);

            }
        });
        //将设备id和通话码添加到map
        dialogMap.add(roomId);
        System.out.println("dialogMap-----add----:"+dialogMap);
        return AjaxResultNew.success("成功");
    }


    @PostMapping("/closeDialog")
    public void closeDialog(@RequestParam(value = "deviceId") String deviceId,
                            @RequestParam(value = "code") String code,
                            @RequestParam(value = "stream") String stream
                            ) {

        String roomId = deviceId + "-" + code + "-" + stream;
        WebSocketServer.sessionMap.forEach((key, session) -> {
            if (session != null && session.isOpen() && key.equals(deviceId)) {
                /**
                 * {
                 * "cmd": "server_push_stop_webrtc_call",
                 * "status": true,
                 * "msg": "停止webrtc通话",
                 * "room_id": "webrtc_room:single_866652021639308",
                 * "msg_code": "StopWebrtcCall"
                 * }
                 */
                JSONObject jsonObject = new JSONObject();
                jsonObject.put("cmd", "server_push_stop_webrtc_call");
                jsonObject.put("status", true);
                jsonObject.put("msg", "停止webrtc通话");
                jsonObject.put("room_id", roomId);
                jsonObject.put("msg_code", "StopWebrtcCall");
                String msg = jsonObject.toJSONString();
                WebSocketUsers.sendMessageToUserByText(session,msg);
            }
        });
        //将设备信息从map中删�?
        if (dialogMap != null) {
            dialogMap.remove(roomId);
        }
    }
    /**
     * 配置config
     * @return json
     */
    private JSONObject getConfig() {
        JSONObject config = new JSONObject();
        //设备发送http请求会基于这个地址发送，例如文件上传等操�? 建议以正斜杠结尾
        config.put("api_url", CAPS_API_URL);
        //设备登录、上报实时定位等数据和接受服务端指令报文
        config.put("ws_url", CAPS_WS_URL);
        //报警温度 单位：摄氏度
        config.put("sos_tmpt", "70");
        //关机温度 单位：摄氏度
        config.put("power_off_tmpt","95");
        //脱帽延迟报警时间 �?
        config.put("off_hat_time","10");
        //脱帽报警间隔时长间隔 分钟
        config.put("offhat_time_interval","45");
        //人脸监测方式  1-关闭  2-库对�?3-人脸验证
        config.put("face_switch","1");
        //静默报警间隔时长
        config.put("nomove_time_interval","45");
        //开机后是否自动开机本地录�?0-开 1-�?
        config.put("local_video_record","1");
        //静电报警单位 1-220v  2-10000v  3-35000v  4-110000v  5-220000v
        config.put("electrostatic","2");
        //监控画质  1-高清  2-超清
        config.put("picture_quality","2");
        //挂钩播报语言  1-中文 2-英文 3-日文
        config.put("hanger_language","1");
        //挂钩播报次数
        config.put("hanger_boradcast","1");
        //本地视频上传配置 1-不自动上�? 2-网络自动上传  3-WiFi自动上传
        config.put("upload_video","1");
        //蓝牙信标扫描开�?0-�?1-开
        config.put("fence_bluetooth_switch","0");
        //是否开启视频分�? 0-�? 1-开
        config.put("open_video_segmentation","0");
        //本地录制时长 单位：秒
        config.put("local_record_time","60");
        //进入盛典模式 分钟   0-关闭 10�?0�?0
        config.put("power_save_interval_time","0");
        //隐私模式 手动设置-0  平台设置-1
        config.put("private_switch","0");
        //定位类型 0-GPS+北斗  1-GPS  2-北斗
        config.put("location_type","0");
        //弃用
        config.put("ftp_host","47.104.100.134");
        //弃用
        config.put("ftp_user","uftp");
        //弃用
        config.put("ftp_pwd","uftp123");
        //DMR模式 1-数字直�? 2-模拟直�? 3-数字中继
        config.put("dmr_model","");
        //频点Mhz
        config.put("dmr_val","");
        //省电模式开�?0-�? 1-开
        config.put("power_save_status","1");
        config.put("privacy_switch","0");
        //本地录制视频前缀
        config.put("video_pre",null);
        return config;
    }

    /**
     * "notice_switch": "
     * {\"isWear\":\"0\",\"fenceAlarm\":\"0\",\"temperature\":\"0\",\"fallAlarm\":\"0\",\"c
     * ollisionAlarm\":\"0\",\"silentAlarm\":\"0\",\"nearpowerAlarm\":\"0\",\"ascendAlarm\"
     * :\"0\",\"lowBattery\":\"0\",\"reminder\":\"0\",\"sos_switch\":\"0\"}",
     *
     * 报警音效  所有均�?0开  1-�?
     * @return json
     */
    private JSONObject getNoticeSwitch() {
        JSONObject noticeSwitch = new JSONObject();
        // 是否佩戴
        noticeSwitch.put("isWear", "0");
        // 围栏报警
        noticeSwitch.put("fenceAlarm", "0");
        // 温度报警
        noticeSwitch.put("temperature", "0");
        // 跌落报警
        noticeSwitch.put("fallAlarm", "0");
        // 碰撞报警
        noticeSwitch.put("collisionAlarm", "0");
        // 静默报警
        noticeSwitch.put("silentAlarm", "0");
        // 近电报警
        noticeSwitch.put("nearpowerAlarm", "1");
        // 登高报警
        noticeSwitch.put("ascendAlarm", "0");
        // 低电报警
        noticeSwitch.put("lowBattery", "0");
        // 本地录制提醒
        noticeSwitch.put("reminder", "0");
        // sos 报警
        noticeSwitch.put("sos_switch", "0");
        return noticeSwitch;
    }

    /**
     * "upgrade": {
     * "check_upgrade": 0,  是否开启检测APK升级  0-�? 1-开
     * "new_apk_version": 0, 新apk版本�?
     * "new_apk_url": "",  新apk下载地址
     * "file_md5": ""
     * }
     *
     * apk 升级配置信息
     * @return json
     */
    private JSONObject getUpGradeJSON() {
        JSONObject upgrade = new JSONObject();
        upgrade.put("check_upgrade", 0);
        upgrade.put("new_apk_version", 0);
        upgrade.put("new_apk_url", "");
        upgrade.put("file_md5", "");
        return upgrade;
    }

    /**
     * "fota_upgrade": {
     * "check_upgrade": 0,  是否开启检测系统升�? 0-�? 1-开
     * "new_apk_url": "",  新版系统下载地址
     * "file_md5": ""  md5
     * } 系统升级配置信息
     * @return json
     */
    private JSONObject getFotaUpgradeJSON() {
        JSONObject fotaUpgrade = new JSONObject();
        fotaUpgrade.put("check_upgrade", 0);
        fotaUpgrade.put("new_apk_url", "");
        fotaUpgrade.put("file_md5", "");
        return fotaUpgrade;
    }

    /**
     * "model_switch": "
     * {\"bodycheck_switch\":\"1\",\"heightcheck_switch\":\"1\",\"electriccheck_switch\":\"
     * 0\",\"audiocheck_switch\":\"1\",\"gascheck_switch\":\"1\",\"tk_switch\":\"0\",\"sos_
     * switch\":1,\"private_switch\":\"0\"}"
     * 模块开�? 所有均�? 0-开  1-�?
     * @return json
     */
    private JSONObject getModelSwitchJSON() {
        JSONObject modelSwitch = new JSONObject();
        // 是否开启人体特征检�?
        modelSwitch.put("bodycheck_switch", "1");
        // 是否开启身高检�?
        modelSwitch.put("heightcheck_switch", "1");
        // 是否开启电量检�?
        modelSwitch.put("electriccheck_switch", "1");
        // 是否开启语音识�?
        modelSwitch.put("audiocheck_switch", "1");
        // 是否开启气体检�?
        modelSwitch.put("gascheck_switch", "1");
        // 是否开启对讲模�?
        modelSwitch.put("tk_switch", "0");
        // 是否开启sos检测模�?
        modelSwitch.put("sos_switch", 1);
        // 是否开启隐私模�?
        modelSwitch.put("private_switch", "0");
        return modelSwitch;
    }

    /**
     * "extra": {
     * "watch_mac": "",   手环mac�?
     * "headset_mac": "",  蓝牙耳机mac�?
     * "gas_mac": "",  气体检测仪mac地址
     * "watch_imei": ""  手环imei�?
     * }
     * @return json
     */
    private JSONObject getExtraJSON() {
        JSONObject extra = new JSONObject();
        extra.put("watch_mac", "");
        extra.put("headset_mac", "");
        extra.put("gas_mac", "");
        extra.put("watch_imei", "");
        return extra;
    }
}


