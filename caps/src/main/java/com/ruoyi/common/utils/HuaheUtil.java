package com.ruoyi.common.utils;

import cn.hutool.core.util.RandomUtil;
import cn.hutool.http.HttpRequest;
import cn.hutool.http.HttpResponse;
import com.alibaba.fastjson2.JSONObject;
import org.springframework.stereotype.Component;
import org.springframework.util.DigestUtils;

/**
 * 功能描述
 * 华和 声光报警设备对接
 *
 * @author: gzh
 * @date: 2025年05月26日 15:55
 */
@Component
public class HuaheUtil {
    private static final String API_KEY = "HcpvRAnDvzwA7UpKoav4C8oPfZAaqHOKFLlfL/67wHE=";
    private static final String SECRET_KEY = "oQT0nFC1AMfH3giCI6yZkRFr6HRCBw/8fzRBVyRMP9A=";
    public static final String COMPANY_TOKEN = "KJSDoa4wIK+BEa/ua4agtXjMyixXtUBDf21256jDrA4=";
//    public static final String DEVICE_CODE = "09-240823012";
    public static final String DEVICE_CODE = "09-250406007";
    public static final String SERVER_IP = "http://safe.hezhiyun.cn:50000";
    public static final String OPEN_URL = "/api/open/start_acousto_alarm";
    public static final String CLOSE_URL = "/api/open/stop_acousto_alarm";

    /**
     * 发送http请求，开启声光报警
     * header:
     * api_key	string	是	申请获得的apiKey
     * timestamp	int	是	秒级时间戳
     * nonce	string	是	32位随机字符串
     * sign	string	是	32位签名token
     *
     * body:
     * company_token	string	是	公司token（需向平台方申请获得）
     * device_code	string	是	设备编号
     */
    public static  void  openDevice(String deviceCode){
        //秒级时间戳
        String timestamp = String.valueOf(System.currentTimeMillis() / 1000);;
        //32位随机字符串
        String nonce = RandomUtil.randomString(32);

        JSONObject jsonObject = new JSONObject();
        jsonObject.put("company_token", COMPANY_TOKEN);
        jsonObject.put("device_code", deviceCode);
        String body = HttpRequest.post(SERVER_IP+OPEN_URL)
                .header("api_key", API_KEY)
                .header("timestamp", timestamp)
                .header("nonce", nonce)
                .header("sign", getSign(timestamp, nonce))
                .body(jsonObject.toJSONString())
                .execute().body();
        System.out.println("开启声光报警："+body);
    }

    public static  void  closeDevice(String deviceCode){
        //秒级时间戳
        String timestamp = String.valueOf(System.currentTimeMillis() / 1000);;
        //32位随机字符串
        String nonce = RandomUtil.randomString(32);

        JSONObject jsonObject = new JSONObject();
        jsonObject.put("company_token", COMPANY_TOKEN);
        jsonObject.put("device_code", deviceCode);
        String body = HttpRequest.post(SERVER_IP+CLOSE_URL)
                .header("api_key", API_KEY)
                .header("timestamp", timestamp)
                .header("nonce", nonce)
                .header("sign", getSign(timestamp, nonce))
                .body(jsonObject.toJSONString())
                .execute().body();
        System.out.println("关闭声光报警："+body);
    }

    private static String getSign(String timestamp, String nonce) {
        String signString = API_KEY+timestamp+nonce+SECRET_KEY;
        //用md5加密成32位字符串
        return DigestUtils.md5DigestAsHex(signString.getBytes());
    }
}
