namespace Venus.WebApi.Services
{
    /// <summary>
    /// 第三方开放接口（场站对接）配置，对应 appsettings.json OpenApi 节点
    /// </summary>
    public sealed class OpenApiOptions
    {
        /// <summary>对接密钥；为空时所有开放接口拒绝访问</summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>生成播放地址用的对外主机（IP 或域名）；为空时取请求 Host</summary>
        public string? PublicHost { get; set; }

        /// <summary>RTSP 拉流端口（宿主机映射，docker 8554:554）</summary>
        public int RtspPort { get; set; } = 8554;

        /// <summary>RTMP 拉流端口（docker 1935:1935）</summary>
        public int RtmpPort { get; set; } = 1935;

        /// <summary>HTTP-FLV 端口（ZLM 80 映射，docker 8081:80）</summary>
        public int HttpFlvPort { get; set; } = 8081;

        /// <summary>WebRTC 信令端口（前端 nginx HTTPS 映射，docker 3000:443）</summary>
        public int WebrtcPort { get; set; } = 3000;
    }
}
