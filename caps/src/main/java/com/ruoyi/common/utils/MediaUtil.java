package com.ruoyi.common.utils;

import cn.hutool.http.HttpUtil;
import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;
import com.ruoyi.common.constant.CapsConstants;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Component;

import java.util.HashMap;
import java.util.HashSet;
import java.util.Map;
import java.util.Set;

/**
 * 功能描述 : 流媒体工具类
 *
 * @author: gzh
 * @date: 2025年05月28日 19:28
 */
@Component
public class MediaUtil {

    private static final Logger LOGGER = LoggerFactory.getLogger(MediaUtil.class);

    public static final String SECRET = "035c73f7-bb6b-4889-a715-d9eb2d1925cc";
    public static final String URL = "/index/api/getMediaList";

    public static final class StreamMetrics
    {
        private final boolean streamPresent;
        private final boolean hasVideo;
        private final boolean hasAudio;
        private final int bytesSpeed;
        private final int readerCount;
        private final int totalReaderCount;
        private final long aliveSecond;
        private final String originType;
        private final Set<String> schemas;

        private StreamMetrics(boolean streamPresent, boolean hasVideo, boolean hasAudio, int bytesSpeed,
                              int readerCount, int totalReaderCount, long aliveSecond, String originType,
                              Set<String> schemas)
        {
            this.streamPresent = streamPresent;
            this.hasVideo = hasVideo;
            this.hasAudio = hasAudio;
            this.bytesSpeed = bytesSpeed;
            this.readerCount = readerCount;
            this.totalReaderCount = totalReaderCount;
            this.aliveSecond = aliveSecond;
            this.originType = originType;
            this.schemas = schemas;
        }

        public static StreamMetrics absent()
        {
            return new StreamMetrics(false, false, false, 0, 0, 0, 0L, "", new HashSet<String>());
        }

        public boolean isStreamPresent()
        {
            return streamPresent;
        }

        public boolean hasVideo()
        {
            return hasVideo;
        }

        public boolean hasAudio()
        {
            return hasAudio;
        }

        public int getBytesSpeed()
        {
            return bytesSpeed;
        }

        public int getReaderCount()
        {
            return readerCount;
        }

        public int getTotalReaderCount()
        {
            return totalReaderCount;
        }

        public long getAliveSecond()
        {
            return aliveSecond;
        }

        public String getOriginType()
        {
            return originType;
        }

        public Set<String> getSchemas()
        {
            return schemas;
        }
    }

    public static boolean getMediaList(String deviceId) {
        return getStreamMetrics(deviceId).isStreamPresent();
    }

    public static String describeStreamMetrics(StreamMetrics metrics)
    {
        if (metrics == null)
        {
            return "是否有流=否, 视频轨=否, 音频轨=否, 字节速率=0, 观看人数=0/0, 流存活秒数=0, 来源类型=, 协议列表=[]";
        }
        return "是否有流=" + (metrics.isStreamPresent() ? "是" : "否")
                + ", 视频轨=" + (metrics.hasVideo() ? "是" : "否")
                + ", 音频轨=" + (metrics.hasAudio() ? "是" : "否")
                + ", 字节速率=" + metrics.getBytesSpeed()
                + ", 观看人数=" + metrics.getReaderCount() + "/" + metrics.getTotalReaderCount()
                + ", 流存活秒数=" + metrics.getAliveSecond()
                + ", 来源类型=" + metrics.getOriginType()
                + ", 协议列表=" + metrics.getSchemas();
    }

    public static StreamMetrics getStreamMetrics(String deviceId) {
        String url = CapsConstants.getWebrtcAddress() + URL;
        Map<String,Object> param = new HashMap<>();
        param.put("stream",deviceId);
        param.put("secret",SECRET);
        String result = HttpUtil.get(url, param);
        if(StringUtils.isEmpty(result)){
            LOGGER.warn("ZLM流列表返回为空: deviceId={}, url={}, params={}", deviceId, url, param);
            return StreamMetrics.absent();
        }
        JSONObject jsonObject = JSONObject.parseObject(result);
        if (jsonObject == null)
        {
            LOGGER.warn("ZLM流列表解析失败: deviceId={}, url={}, raw={}", deviceId, url, result);
            return StreamMetrics.absent();
        }
        if (jsonObject.getIntValue("code") != 0)
        {
            LOGGER.warn("ZLM流列表返回异常状态码: deviceId={}, url={}, code={}, raw={}",
                    deviceId, url, jsonObject.getIntValue("code"), result);
        }
        JSONArray data = jsonObject.getJSONArray("data");
        if(data == null){
            return StreamMetrics.absent();
        }
        if (data.isEmpty()) {
            return StreamMetrics.absent();
        }

        boolean hasVideo = false;
        boolean hasAudio = false;
        int bytesSpeed = 0;
        int readerCount = 0;
        int totalReaderCount = 0;
        long aliveSecond = 0L;
        String originType = "";
        Set<String> schemas = new HashSet<String>();

        for (int i = 0; i < data.size(); i++) {
            JSONObject item = data.getJSONObject(i);
            if (item == null) {
                continue;
            }

            schemas.add(item.getString("schema"));
            bytesSpeed = Math.max(bytesSpeed, item.getIntValue("bytesSpeed"));
            readerCount = Math.max(readerCount, item.getIntValue("readerCount"));
            totalReaderCount = Math.max(totalReaderCount, item.getIntValue("totalReaderCount"));
            aliveSecond = Math.max(aliveSecond, item.getLongValue("aliveSecond"));
            if (StringUtils.isEmpty(originType) && StringUtils.isNotEmpty(item.getString("originTypeStr"))) {
                originType = item.getString("originTypeStr");
            }

            JSONArray tracks = item.getJSONArray("tracks");
            if (tracks == null) {
                continue;
            }

            for (int j = 0; j < tracks.size(); j++) {
                JSONObject track = tracks.getJSONObject(j);
                if (track == null) {
                    continue;
                }
                Integer codecType = track.getInteger("codec_type");
                if (codecType == null) {
                    continue;
                }
                if (codecType == 0) {
                    hasVideo = true;
                } else if (codecType == 1) {
                    hasAudio = true;
                }
            }
        }

        return new StreamMetrics(true, hasVideo, hasAudio, bytesSpeed, readerCount, totalReaderCount,
                aliveSecond, originType, schemas);
    }
}
