package com.ruoyi.project.system.controller;

import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;
import com.ruoyi.common.constant.CapsConstants;
import com.ruoyi.common.utils.HuaheUtil;
import com.ruoyi.common.utils.MediaUtil;
import com.ruoyi.framework.web.domain.AjaxResultNew;
import com.ruoyi.project.system.domain.CapsDevice;
import com.ruoyi.project.system.domain.dto.CapsGasReportRequestDto;
import com.ruoyi.project.system.service.IAudibleVoiceAlarmService;
import com.ruoyi.project.system.service.ICapsDeviceReportService;
import com.ruoyi.project.system.service.ICapsDeviceService;
import com.ruoyi.project.system.websocket.WebSocketServer;
import com.ruoyi.project.system.websocket.WebSocketUsers;
import lombok.extern.slf4j.Slf4j;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

import javax.annotation.Resource;
import javax.servlet.http.HttpServletRequest;
import javax.websocket.Session;
import java.util.HashSet;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.Set;

@RestController
@RequestMapping("/caps/api")
@Slf4j
public class CapsConfigApi
{
    @Resource
    private ICapsDeviceService capsDeviceService;

    @Resource
    private ICapsDeviceReportService capsDeviceReportService;

    @Resource
    private IAudibleVoiceAlarmService audioAlarmService;

    @GetMapping("/index.php")
    public AjaxResultNew getDeviceConfig(HttpServletRequest request, String device_id, String ca_ver)
    {
        log.info("url:{}", request.getRequestURL());
        log.info("param:{}", request.getQueryString());

        JSONObject jsonObject = new JSONObject();
        jsonObject.put("account_id", "11444");
        jsonObject.put("config", getConfig().toJSONString());
        jsonObject.put("notice_switch", getNoticeSwitch().toJSONString());
        jsonObject.put("ai_broadcast", "[]");
        jsonObject.put("upgrade", getUpGradeJSON());
        jsonObject.put("fota_upgrade", getFotaUpgradeJSON());
        jsonObject.put("model_switch", getModelSwitchJSON().toJSONString());
        jsonObject.put("voice_pkg_md5", "");
        jsonObject.put("pkg_url", "");
        jsonObject.put("low_report_time", 450);
        jsonObject.put("ca_report_time", 0);
        jsonObject.put("extra", getExtraJSON());
        jsonObject.put("gj_ble_mac", "123");
        jsonObject.put("time", System.currentTimeMillis() / 1000);

        return AjaxResultNew.success("success", jsonObject);
    }

    @GetMapping("/openHuaheDevice/{personId}")
    public AjaxResultNew openHuaheDevice(@PathVariable("personId") String personId)
    {
        List<String> deviceIdsByPersonId = audioAlarmService.getDeviceIdsByPersonId(personId);
        if (deviceIdsByPersonId != null && !deviceIdsByPersonId.isEmpty()) {
            for (String deviceId : deviceIdsByPersonId) {
                HuaheUtil.openDevice(deviceId);
            }
        }
        return AjaxResultNew.success("success");
    }

    @GetMapping("/closeHuaheDevice/{personId}")
    public AjaxResultNew closeHuaheDevice(@PathVariable("personId") String personId)
    {
        List<String> deviceIdsByPersonId = audioAlarmService.getDeviceIdsByPersonId(personId);
        if (deviceIdsByPersonId != null && !deviceIdsByPersonId.isEmpty()) {
            for (String deviceId : deviceIdsByPersonId) {
                HuaheUtil.closeDevice(deviceId);
            }
        }
        return AjaxResultNew.success("success");
    }

    @PostMapping("/index.php")
    public AjaxResultNew getDeviceConfig(HttpServletRequest request, String ca_ver, String act, String ctl,
                                         String user_id)
    {
        if ("device".equals(ctl) && "check_stream".equals(act)) {
            CapsDevice capsDevice = capsDeviceService.selectCapsDeviceByUserId(user_id);
            if (capsDevice != null) {
                JSONObject jsonObject = new JSONObject();
                String deviceId = capsDevice.getDeviceId();
                WebSocketUsers.PushCmdState pushCmdState = WebSocketUsers.getPushCmdState(deviceId);
                long now = System.currentTimeMillis();
                try {
                    boolean push = MediaUtil.getMediaList(deviceId);
                    jsonObject.put("push", push);

                    if (push) {
                        String rtmpUrl = pushCmdState == null ? WebSocketUsers.buildPushUrl(deviceId) : pushCmdState.getSourceRtmpUrl();
                        String elapsed = formatElapsedSeconds(pushCmdState == null ? 0L : now - pushCmdState.getCommandTimeMs());
                        log.info("stream publish detected: deviceId={}, elapsed={}s, rtmp={}",
                                deviceId, elapsed, rtmpUrl);
                        WebSocketUsers.clearPushCmdState(deviceId);
                    } else if (pushCmdState != null) {
                        long elapsedMs = now - pushCmdState.getCommandTimeMs();
                        String elapsed = formatElapsedSeconds(elapsedMs);
                        if (elapsedMs >= WebSocketUsers.getPushDetectTimeoutMs()) {
                            log.warn("stream not detected after push command: deviceId={}, elapsed={}s, expect_rtmp={}",
                                    deviceId, elapsed, pushCmdState.getSourceRtmpUrl());
                            WebSocketUsers.clearPushCmdState(deviceId);
                        } else {
                            log.info("waiting stream publish: deviceId={}, elapsed={}s, expect_rtmp={}",
                                    deviceId, elapsed, pushCmdState.getSourceRtmpUrl());
                        }
                    } else {
                        log.info("stream check without push state: deviceId={}, userId={}, expect_rtmp={}",
                                deviceId, user_id, WebSocketUsers.buildPushUrl(deviceId));
                    }
                } catch (Exception e) {
                    jsonObject.put("push", false);
                    log.warn("stream check failed: deviceId={}, err={}", deviceId, e.getMessage(), e);
                }
                return AjaxResultNew.success("success", jsonObject);
            }
            log.warn("stream check skipped: userId={} not bound to device", user_id);
        }
        return AjaxResultNew.success("success", "");
    }

    @PostMapping("/gas/report")
    public AjaxResultNew reportGas(@RequestBody CapsGasReportRequestDto gasReportRequest)
    {
        try {
            int updatedCount = capsDeviceReportService.saveGasReport(gasReportRequest);
            return AjaxResultNew.success("gas report received", updatedCount);
        } catch (IllegalArgumentException e) {
            return AjaxResultNew.error(e.getMessage());
        } catch (Exception e) {
            log.error("save gas report error, request:{}", JSONObject.toJSONString(gasReportRequest), e);
            return AjaxResultNew.error("save gas report failed");
        }
    }

    public static Set<String> dialogMap = new HashSet<>();

    @PostMapping("/openDialog")
    public AjaxResultNew openDialog(@RequestParam(value = "stream") String stream,
                                    @RequestParam(value = "code") String code,
                                    @RequestParam(value = "deviceId") String deviceId)
    {
        Map<String, Session> sessionMap = WebSocketServer.sessionMap;
        log.info("openDialog request: deviceId={}, code={}, stream={}, sessionKeys={}, dialogMap={}",
                deviceId, code, stream, sessionMap.keySet(), dialogMap);
        Session session = WebSocketServer.getActiveSession(deviceId);
        if (session == null) {
            log.warn("openDialog rejected: deviceId={} not found in sessionMap. sessionKeys={}",
                    deviceId, sessionMap.keySet());
            return AjaxResultNew.error("device websocket session not found");
        }

        String roomId = deviceId + "-" + code + "-" + stream;
        log.info("openDialog dispatch: roomId={}, deviceId={}, sessionId={}",
                roomId, deviceId, session.getId());
        JSONObject jsonObject = new JSONObject();
        jsonObject.put("cmd", "server_push_webrtc_call");
        jsonObject.put("room_id", roomId);

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
        jsonObject.put("msg", "success");
        jsonObject.put("msg_code", jsonArray);
        String msg = jsonObject.toJSONString();
        log.info("openDialog payload: {}", msg);
        WebSocketUsers.sendMessageToUserByText(session, msg);
        dialogMap.add(roomId);
        log.info("openDialog accepted: roomId={}, dialogMap={}", roomId, dialogMap);
        return AjaxResultNew.success("openDialog success");
    }

    @PostMapping("/closeDialog")
    public AjaxResultNew closeDialog(@RequestParam(value = "deviceId") String deviceId,
                                     @RequestParam(value = "code") String code,
                                     @RequestParam(value = "stream") String stream)
    {
        String roomId = deviceId + "-" + code + "-" + stream;
        Map<String, Session> sessionMap = WebSocketServer.sessionMap;
        log.info("closeDialog request: deviceId={}, code={}, stream={}, roomId={}, sessionKeys={}, dialogMap={}",
                deviceId, code, stream, roomId, sessionMap.keySet(), dialogMap);
        Session session = WebSocketServer.getActiveSession(deviceId);
        if (session == null) {
            log.warn("closeDialog rejected: deviceId={} not found in sessionMap. sessionKeys={}",
                    deviceId, sessionMap.keySet());
            return AjaxResultNew.error("device websocket session not found");
        }

        log.info("closeDialog dispatch: roomId={}, deviceId={}, sessionId={}",
                roomId, deviceId, session.getId());
        JSONObject jsonObject = new JSONObject();
        jsonObject.put("cmd", "server_push_stop_webrtc_call");
        jsonObject.put("status", true);
        jsonObject.put("msg", "StopWebrtcCall");
        jsonObject.put("room_id", roomId);
        jsonObject.put("msg_code", "StopWebrtcCall");
        String msg = jsonObject.toJSONString();
        WebSocketUsers.sendMessageToUserByText(session, msg);
        dialogMap.remove(roomId);
        log.info("closeDialog completed: roomId={}, dialogMap={}", roomId, dialogMap);
        return AjaxResultNew.success("closeDialog success");
    }

    private JSONObject getConfig()
    {
        JSONObject config = new JSONObject();
        config.put("api_url", CapsConstants.getIpAddress() + "/caps/api");
        config.put("ws_url", CapsConstants.getSocketAddress() + "/websocket/message");
        config.put("sos_tmpt", "70");
        config.put("power_off_tmpt", "95");
        config.put("off_hat_time", "10");
        config.put("offhat_time_interval", "45");
        config.put("face_switch", "1");
        config.put("nomove_time_interval", "45");
        config.put("local_video_record", "1");
        config.put("electrostatic", "2");
        config.put("picture_quality", "2");
        config.put("hanger_language", "1");
        config.put("hanger_boradcast", "1");
        config.put("upload_video", "1");
        config.put("fence_bluetooth_switch", "0");
        config.put("open_video_segmentation", "0");
        config.put("local_record_time", "60");
        config.put("power_save_interval_time", "0");
        config.put("private_switch", "0");
        config.put("location_type", "0");
        config.put("ftp_host", "47.104.100.134");
        config.put("ftp_user", "uftp");
        config.put("ftp_pwd", "uftp123");
        config.put("dmr_model", "");
        config.put("dmr_val", "");
        config.put("power_save_status", "1");
        config.put("privacy_switch", "0");
        config.put("video_pre", null);
        return config;
    }

    private JSONObject getNoticeSwitch()
    {
        JSONObject noticeSwitch = new JSONObject();
        noticeSwitch.put("isWear", "0");
        noticeSwitch.put("fenceAlarm", "0");
        noticeSwitch.put("temperature", "0");
        noticeSwitch.put("fallAlarm", "0");
        noticeSwitch.put("collisionAlarm", "0");
        noticeSwitch.put("silentAlarm", "0");
        noticeSwitch.put("nearpowerAlarm", "1");
        noticeSwitch.put("ascendAlarm", "0");
        noticeSwitch.put("lowBattery", "0");
        noticeSwitch.put("reminder", "0");
        noticeSwitch.put("sos_switch", "0");
        return noticeSwitch;
    }

    private JSONObject getUpGradeJSON()
    {
        JSONObject upgrade = new JSONObject();
        upgrade.put("check_upgrade", 0);
        upgrade.put("new_apk_version", 0);
        upgrade.put("new_apk_url", "");
        upgrade.put("file_md5", "");
        return upgrade;
    }

    private JSONObject getFotaUpgradeJSON()
    {
        JSONObject fotaUpgrade = new JSONObject();
        fotaUpgrade.put("check_upgrade", 0);
        fotaUpgrade.put("new_apk_url", "");
        fotaUpgrade.put("file_md5", "");
        return fotaUpgrade;
    }

    private JSONObject getModelSwitchJSON()
    {
        JSONObject modelSwitch = new JSONObject();
        modelSwitch.put("bodycheck_switch", "1");
        modelSwitch.put("heightcheck_switch", "1");
        modelSwitch.put("electriccheck_switch", "1");
        modelSwitch.put("audiocheck_switch", "1");
        modelSwitch.put("gascheck_switch", "1");
        modelSwitch.put("tk_switch", "0");
        modelSwitch.put("sos_switch", 1);
        modelSwitch.put("private_switch", "0");
        return modelSwitch;
    }

    private JSONObject getExtraJSON()
    {
        JSONObject extra = new JSONObject();
        extra.put("watch_mac", "");
        extra.put("headset_mac", "");
        extra.put("gas_mac", "");
        extra.put("watch_imei", "");
        return extra;
    }

    private static String formatElapsedSeconds(long elapsedMs)
    {
        return String.format(Locale.ROOT, "%.1f", elapsedMs / 1000.0);
    }
}
