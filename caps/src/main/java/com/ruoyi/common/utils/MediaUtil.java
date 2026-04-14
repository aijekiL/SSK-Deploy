package com.ruoyi.common.utils;

import cn.hutool.http.HttpUtil;
import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;
import com.ruoyi.common.constant.CapsConstants;
import org.springframework.stereotype.Component;

import java.util.HashMap;
import java.util.Map;

/**
 * 功能描述 : 流媒体工具类
 *
 * @author: gzh
 * @date: 2025年05月28日 19:28
 */
@Component
public class MediaUtil {

    public static final String SECRET = "035c73f7-bb6b-4889-a715-d9eb2d1925cc";
    public static final String URL = "/index/api/getMediaList";

    public static boolean getMediaList(String deviceId) {
        String url = CapsConstants.getWebrtcAddress() + URL;
        Map<String,Object> param = new HashMap<>();
        param.put("stream",deviceId);
        param.put("secret",SECRET);
        String result = HttpUtil.get(url, param);
        if(StringUtils.isEmpty(result)){
            return false;
        }
        JSONObject jsonObject = JSONObject.parseObject(result);
        JSONArray data = jsonObject.getJSONArray("data");
        if(data == null){
            return false;
        }
        return !data.isEmpty();
    }
}
