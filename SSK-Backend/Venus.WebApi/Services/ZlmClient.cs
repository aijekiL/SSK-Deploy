using Microsoft.Extensions.Options;

namespace Venus.WebApi.Services
{
    /// <summary>
    /// ZLMediaKit 配置
    /// </summary>
    public sealed class ZlmOptions
    {
        /// <summary>
        /// ZLM HTTP API 基础地址（容器内网地址）
        /// </summary>
        public string HttpBaseUrl { get; set; } = "http://zlmediakit:80";

        /// <summary>
        /// ZLM API 密钥（与 zlm config.ini [api] secret 一致，hook 回调校验也用它）
        /// </summary>
        public string Secret { get; set; } = string.Empty;
    }

    /// <summary>
    /// ZLMediaKit 客户端（API 调用与录制文件流代理）
    /// </summary>
    public class ZlmClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ZlmOptions _options;

        public ZlmClient(IHttpClientFactory httpClientFactory, IOptions<ZlmOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value ?? new ZlmOptions();
        }

        /// <summary>
        /// 删除指定流一段时间内的 MP4 录制文件
        /// </summary>
        public async Task<bool> DeleteRecordMp4Async(string app, string stream, long startUnixSeconds, long endUnixSeconds, CancellationToken cancellationToken)
        {
            try
            {
                var url = $"{_options.HttpBaseUrl.TrimEnd('/')}/index/api/deleteRecordMp4" +
                          $"?secret={Uri.EscapeDataString(_options.Secret)}" +
                          $"&vhost=__defaultVhost__&app={Uri.EscapeDataString(app)}&stream={Uri.EscapeDataString(stream)}" +
                          $"&period={startUnixSeconds}_{endUnixSeconds}";
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(15);
                using var response = await client.GetAsync(url, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                if (!response.IsSuccessStatusCode || !body.Contains("\"code\":0") && !body.Contains("\"code\": 0"))
                {
                    // 部分版本按 period 无匹配文件时返回非0，文件可能已被清理，视为可接受
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 从 ZLM HTTP 文件服务拉取录制文件流（用于后端代理下载）
        /// </summary>
        public async Task<Stream> OpenRecordFileAsync(string relPath, CancellationToken cancellationToken)
        {
            var url = BuildFileUrl(relPath);
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(10);
            var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                response.Dispose();
                throw new InvalidOperationException($"读取录像文件失败({(int)response.StatusCode})：{url}");
            }
            return await response.Content.ReadAsStreamAsync(cancellationToken);
        }

        public string BuildFileUrl(string relPath)
        {
            return $"{_options.HttpBaseUrl.TrimEnd('/')}/{relPath.TrimStart('/')}";
        }
    }
}
