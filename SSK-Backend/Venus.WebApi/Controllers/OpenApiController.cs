using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Venus.Entitys;
using Venus.Entitys.SecurityMonitorService;
using Venus.Tools.Response;
using Venus.WebApi.Common;
using Venus.WebApi.Services;

namespace Venus.WebApi.Controllers
{
    /// <summary>
    /// 第三方对接开放接口（场站内网系统）：在场人员、手表健康、气体检测、安全帽告警、在线视频流。
    /// 鉴权：Header X-Api-Key（或 URL 参数 apiKey），密钥见 appsettings.json OpenApi:ApiKey，与登录体系无关。
    /// 所有接口均为只读，轮询建议间隔 5~10 秒。
    /// </summary>
    [Route("api/OpenApi/[action]")]
    [ApiController]
    [AllowAnonymous]
    [OpenApiKey]
    public class OpenApiController : ControllerBase
    {
        private readonly VenusDbContext _dbContext;
        private readonly CapsLedgerOptions _capsOptions;
        private readonly ZlmOptions _zlmOptions;
        private readonly OpenApiOptions _openApiOptions;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OpenApiController> _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        public OpenApiController(
            VenusDbContext dbContext,
            IOptions<CapsLedgerOptions> capsOptions,
            IOptions<ZlmOptions> zlmOptions,
            IOptions<OpenApiOptions> openApiOptions,
            IHttpClientFactory httpClientFactory,
            ILogger<OpenApiController> logger)
        {
            _dbContext = dbContext;
            _capsOptions = capsOptions.Value ?? new CapsLedgerOptions();
            _zlmOptions = zlmOptions.Value ?? new ZlmOptions();
            _openApiOptions = openApiOptions.Value ?? new OpenApiOptions();
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        #region 人员监测

        /// <summary>
        /// 当前工程信息（未传 projectId 时取 IsCurrent 工程）
        /// </summary>
        [HttpGet]
        public WebResponseContent CurrentProject()
        {
            var project = ResolveProject(null);
            if (project == null)
            {
                return WebResponseContent.Instance.Error("未找到工程");
            }
            return WebResponseContent.Instance.OK("获取成功", new
            {
                projectId = project.ObjectId,
                projectName = project.ProjectName,
                unitName = project.UnitName,
                managerName = project.ManagerName,
                managerPhone = project.ManagerPhone,
                guardianName = project.GuardianName,
                administratorName = project.AdministratorName,
                startWorkDate = project.StartWorkDate,
                status = project.Status
            });
        }

        /// <summary>
        /// 人员列表（含在场状态、绑定安全帽/手表、入场时间；includeWatch=true 时附带手表最新健康数据）
        /// </summary>
        /// <param name="projectId">工程ID，不传取当前工程</param>
        /// <param name="onSiteOnly">true=仅返回在场人员（Status=2）</param>
        /// <param name="includeWatch">是否附带手表最新健康数据</param>
        [HttpGet]
        public WebResponseContent StaffList(Guid? projectId, bool onSiteOnly = false, bool includeWatch = true)
        {
            var project = ResolveProject(projectId);
            if (project == null)
            {
                return WebResponseContent.Instance.Error("未找到工程");
            }

            var query = _dbContext.Set<SM_StaffInfo>().Where(x => !x.Deleted && x.ProjectId == project.ObjectId);
            if (onSiteOnly)
            {
                query = query.Where(x => x.Status == 2);
            }
            var staffs = query.OrderBy(x => x.name).ToList();

            // 未退场的进出记录 → 入场时间
            var enterTimes = _dbContext.Set<SM_StaffInOutLog>()
                .Where(x => x.ProjectId == project.ObjectId && x.ExitTime == null)
                .ToList()
                .GroupBy(x => x.StaffInfoId)
                .ToDictionary(g => g.Key, g => g.Max(x => x.EnterTime));

            // 手表最新健康数据（按 IMEI 逐个取最新一条，绑表人数有限可接受）
            var watchLatest = new Dictionary<string, SM_WatchHealthReport>();
            if (includeWatch)
            {
                var imeis = staffs
                    .Where(x => !string.IsNullOrWhiteSpace(x.WatchImei))
                    .Select(x => x.WatchImei!.Trim())
                    .Distinct()
                    .ToList();
                foreach (var imei in imeis)
                {
                    var report = _dbContext.Set<SM_WatchHealthReport>()
                        .Where(x => x.WatchImei == imei)
                        .OrderByDescending(x => x.ReportTime)
                        .FirstOrDefault();
                    if (report != null)
                    {
                        watchLatest[imei] = report;
                    }
                }
            }

            var list = staffs.Select(x =>
            {
                enterTimes.TryGetValue(x.ObjectId, out var enterTime);
                watchLatest.TryGetValue(x.WatchImei?.Trim() ?? string.Empty, out var watch);
                return new
                {
                    staffId = x.ObjectId,
                    name = x.name,
                    gender = x.gender,
                    company = x.company,
                    department = x.department,
                    phone = x.phone,
                    status = x.Status,
                    statusText = x.Status == 2 ? "在场" : x.Status == 1 ? "登记中" : "离场",
                    enterTime = enterTime,
                    device1 = x.Device1,
                    watchImei = x.WatchImei,
                    watch = watch == null ? null : new
                    {
                        heartRate = watch.HeartRate,
                        spo2 = watch.Spo2,
                        systolic = watch.Systolic,
                        diastolic = watch.Diastolic,
                        bloodPressure = watch.Systolic.HasValue && watch.Diastolic.HasValue
                            ? $"{watch.Systolic}/{watch.Diastolic}"
                            : "",
                        updateTime = watch.ReportTime
                    }
                };
            }).ToList();

            return WebResponseContent.Instance.OK("获取成功", new
            {
                projectId = project.ObjectId,
                projectName = project.ProjectName,
                total = list.Count,
                onSite = list.Count(o => o.status == 2),
                list
            });
        }

        /// <summary>
        /// 人员最新手表健康数据（staffId 与 watchImei 二选一）
        /// </summary>
        [HttpGet]
        public WebResponseContent WatchLatest(Guid? staffId, string? watchImei)
        {
            var normalizedWatchImei = string.IsNullOrWhiteSpace(watchImei) ? null : watchImei.Trim();
            SM_StaffInfo? staff = null;

            if (staffId.HasValue && staffId.Value != Guid.Empty)
            {
                staff = _dbContext.Set<SM_StaffInfo>().FirstOrDefault(x => x.ObjectId == staffId.Value);
                normalizedWatchImei ??= staff?.WatchImei;
            }

            if (string.IsNullOrWhiteSpace(normalizedWatchImei))
            {
                return WebResponseContent.Instance.OK("未绑定手表", null!);
            }

            var report = _dbContext.Set<SM_WatchHealthReport>()
                .Where(x => x.WatchImei == normalizedWatchImei)
                .OrderByDescending(x => x.ReportTime)
                .FirstOrDefault();

            if (report == null)
            {
                return WebResponseContent.Instance.OK("暂无手表数据", new
                {
                    watchImei = normalizedWatchImei
                });
            }

            return WebResponseContent.Instance.OK("获取成功", new
            {
                deviceId = report.WatchImei,
                watchImei = report.WatchImei,
                staffId = report.StaffId ?? staff?.ObjectId,
                heartRate = report.HeartRate,
                spo2 = report.Spo2,
                systolic = report.Systolic,
                diastolic = report.Diastolic,
                bloodPressure = report.Systolic.HasValue && report.Diastolic.HasValue
                    ? $"{report.Systolic}/{report.Diastolic}"
                    : "",
                updateTime = report.ReportTime
            });
        }

        #endregion

        #region 气体检测 / 告警（caps 安全帽库）

        /// <summary>
        /// 在场人员安全帽气体检测最新值（取各设备近 recentMinutes 分钟内的最新上报）
        /// </summary>
        /// <param name="projectId">工程ID，不传取当前工程</param>
        /// <param name="recentMinutes">回看窗口（分钟），默认10</param>
        /// <param name="cancellationToken"></param>
        [HttpGet]
        public async Task<WebResponseContent> GasLatest(Guid? projectId, int recentMinutes = 10, CancellationToken cancellationToken = default)
        {
            var project = ResolveProject(projectId);
            if (project == null)
            {
                return WebResponseContent.Instance.Error("未找到工程");
            }

            var onsite = _dbContext.Set<SM_StaffInfo>()
                .Where(x => !x.Deleted && x.ProjectId == project.ObjectId && x.Status == 2 && x.Device1 != null && x.Device1 != "")
                .ToList();
            if (onsite.Count == 0)
            {
                return WebResponseContent.Instance.OK("当前无在场人员", new { list = Array.Empty<object>() });
            }

            var deviceNames = onsite
                .GroupBy(x => x.Device1!.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().name, StringComparer.OrdinalIgnoreCase);
            var since = DateTime.Now.AddMinutes(-Math.Max(recentMinutes, 1));

            try
            {
                var rows = await QueryGasHistoryAsync(deviceNames.Keys.ToList(), since, DateTime.Now, cancellationToken);
                var list = rows
                    .GroupBy(x => x.DeviceId, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.OrderByDescending(x => x.CreateTime).First())
                    .OrderBy(x => x.DeviceId)
                    .Select(x => new
                    {
                        deviceId = x.DeviceId,
                        name = deviceNames.TryGetValue(x.DeviceId, out var name) ? name : "",
                        o2 = x.O2,          // %VOL，正常 19.5~21
                        ch4 = x.Ch4,        // %LEL，正常 <10
                        h2s = x.H2s,        // ppm，正常 <7
                        co = x.Co,          // ppm，正常 <25
                        abnormal = IsGasAbnormal(x),
                        updateTime = x.CreateTime
                    })
                    .ToList();
                return WebResponseContent.Instance.OK("获取成功", new { list });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenApi 查询气体数据失败");
                return WebResponseContent.Instance.Error("查询气体数据失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 安全帽告警记录（近 minutes 分钟，默认24小时；仅统计该工程场内有进出记录的设备）
        /// </summary>
        /// <param name="projectId">工程ID，不传取当前工程</param>
        /// <param name="minutes">回看窗口（分钟），默认1440</param>
        /// <param name="cancellationToken"></param>
        [HttpGet]
        public async Task<WebResponseContent> Alarms(Guid? projectId, int minutes = 1440, CancellationToken cancellationToken = default)
        {
            var project = ResolveProject(projectId);
            if (project == null)
            {
                return WebResponseContent.Instance.Error("未找到工程");
            }

            var since = DateTime.Now.AddMinutes(-Math.Clamp(minutes, 1, 24 * 30 * 60));
            var logs = _dbContext.Set<SM_StaffInOutLog>()
                .Where(x => x.ProjectId == project.ObjectId && (x.EnterTime >= since || x.ExitTime == null))
                .OrderByDescending(x => x.EnterTime)
                .ToList();
            var deviceIds = logs
                .Where(x => !string.IsNullOrWhiteSpace(x.Device1))
                .Select(x => x.Device1!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (deviceIds.Count == 0)
            {
                return WebResponseContent.Instance.OK("获取成功", new { list = Array.Empty<object>() });
            }

            // 设备 → 最近一次携带人信息
            var owners = logs
                .Where(x => !string.IsNullOrWhiteSpace(x.Device1))
                .GroupBy(x => x.Device1!.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            try
            {
                var alarms = await QueryAlarmHistoryAsync(deviceIds, since, DateTime.Now, cancellationToken);
                var list = alarms
                    .OrderByDescending(x => x.CreateTime)
                    .Take(500)
                    .Select(x =>
                    {
                        owners.TryGetValue(x.DeviceId, out var log);
                        return new
                        {
                            deviceId = x.DeviceId,
                            name = log?.Name ?? "",
                            company = log?.Company ?? "",
                            department = log?.Department ?? "",
                            phone = log?.Phone ?? "",
                            type = x.Type,
                            typeInfo = x.TypeInfo,
                            time = x.CreateTime
                        };
                    })
                    .ToList();
                return WebResponseContent.Instance.OK("获取成功", new { list });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenApi 查询告警数据失败");
                return WebResponseContent.Instance.Error("查询告警数据失败：" + ex.Message);
            }
        }

        #endregion

        #region 视频流

        /// <summary>
        /// 当前在线视频流列表及各协议播放地址（数据来源 ZLMediaKit getMediaList）
        /// </summary>
        [HttpGet]
        public async Task<WebResponseContent> LiveStreams(CancellationToken cancellationToken = default)
        {
            var url = $"{_zlmOptions.HttpBaseUrl.TrimEnd('/')}/index/api/getMediaList?secret={Uri.EscapeDataString(_zlmOptions.Secret)}";
            string body;
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(10);
                body = await client.GetStringAsync(url, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenApi 调用 ZLM getMediaList 失败");
                return WebResponseContent.Instance.Error("获取视频流列表失败：" + ex.Message);
            }

            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenApi 解析 ZLM getMediaList 响应失败：{Body}", body);
                return WebResponseContent.Instance.Error("解析视频流列表失败");
            }

            using (doc)
            {
                var root = doc.RootElement;
                if (!root.TryGetProperty("code", out var codeEl) || codeEl.GetInt32() != 0
                    || !root.TryGetProperty("data", out var dataEl) || dataEl.ValueKind != JsonValueKind.Array)
                {
                    return WebResponseContent.Instance.Error("ZLM 返回异常：" + root.GetRawText());
                }

                // 在场人员 → 设备（用于标注流当前归属人）
                var holders = _dbContext.Set<SM_StaffInfo>()
                    .Where(x => !x.Deleted && x.Status == 2 && x.Device1 != null && x.Device1 != "")
                    .ToList()
                    .GroupBy(x => x.Device1!.Trim(), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First().name, StringComparer.OrdinalIgnoreCase);

                var host = string.IsNullOrWhiteSpace(_openApiOptions.PublicHost)
                    ? Request.Host.Host
                    : _openApiOptions.PublicHost.Trim();

                // 同一流会按 schema（rtsp/rtmp/hls/fmp4）返回多条，按 app/stream 去重
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var list = new List<object>();
                foreach (var item in dataEl.EnumerateArray())
                {
                    var app = item.TryGetProperty("app", out var appEl) && !string.IsNullOrWhiteSpace(appEl.GetString())
                        ? appEl.GetString()!
                        : "live";
                    var stream = item.TryGetProperty("stream", out var streamEl) ? streamEl.GetString() ?? "" : "";
                    if (string.IsNullOrWhiteSpace(stream) || !seen.Add($"{app}/{stream}"))
                    {
                        continue;
                    }

                    holders.TryGetValue(stream.Trim(), out var holder);
                    list.Add(new
                    {
                        streamId = stream,
                        app,
                        holder = holder ?? "",
                        playUrls = new
                        {
                            rtsp = $"rtsp://{host}:{_openApiOptions.RtspPort}/{app}/{stream}",
                            rtmp = $"rtmp://{host}:{_openApiOptions.RtmpPort}/{app}/{stream}",
                            httpFlv = $"http://{host}:{_openApiOptions.HttpFlvPort}/{app}/{stream}.live.flv",
                            webrtc = $"https://{host}:{_openApiOptions.WebrtcPort}/zlm/index/api/webrtc?app={app}&stream={stream}&type=play"
                        }
                    });
                }
                return WebResponseContent.Instance.OK("获取成功", new { list });
            }
        }

        #endregion

        #region 私有方法

        /// <summary>解析工程：优先指定ID，否则取 IsCurrent 优先、创建时间最新的工程</summary>
        private SM_ProjectInfo? ResolveProject(Guid? projectId)
        {
            if (projectId.HasValue && projectId.Value != Guid.Empty)
            {
                return _dbContext.Set<SM_ProjectInfo>().FirstOrDefault(x => x.ObjectId == projectId.Value && !x.Deleted);
            }
            return _dbContext.Set<SM_ProjectInfo>()
                .Where(x => !x.Deleted)
                .OrderByDescending(x => x.IsCurrent)
                .ThenByDescending(x => x.CreateDate)
                .FirstOrDefault();
        }

        /// <summary>气体是否越限：O2 19.5~21%VOL、CH4 &lt;10%LEL、H2S &lt;7ppm、CO &lt;25ppm（与作业台账判定一致）</summary>
        private static bool IsGasAbnormal(GasItem x)
        {
            return (x.O2.HasValue && (x.O2.Value < 19.5m || x.O2.Value > 21m))
                || (x.Ch4.HasValue && x.Ch4.Value >= 10m)
                || (x.H2s.HasValue && x.H2s.Value >= 7m)
                || (x.Co.HasValue && x.Co.Value >= 25m);
        }

        private sealed class GasItem
        {
            public string DeviceId { get; set; } = string.Empty;
            public DateTime CreateTime { get; set; }
            public decimal? O2 { get; set; }
            public decimal? Ch4 { get; set; }
            public decimal? H2s { get; set; }
            public decimal? Co { get; set; }
        }

        private sealed class AlarmItem
        {
            public string DeviceId { get; set; } = string.Empty;
            public int Type { get; set; }
            public string TypeInfo { get; set; } = string.Empty;
            public DateTime CreateTime { get; set; }
        }

        /// <summary>跨库查询 caps_device_report_history 气体上报历史（同实例 MySQL）</summary>
        private async Task<List<GasItem>> QueryGasHistoryAsync(List<string> deviceIds, DateTime start, DateTime end, CancellationToken cancellationToken)
        {
            var result = new List<GasItem>();
            var connection = _dbContext.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken);
            }

            var dbName = _capsOptions.Database?.Replace("`", string.Empty) ?? "ry-zhdj";
            var paramNames = deviceIds.Select((_, i) => $"@d{i}").ToList();
            var sql = new StringBuilder()
                .Append($"SELECT device_id, o2, co, h2s, ch4, create_time FROM `{dbName}`.caps_device_report_history ")
                .Append("WHERE create_time >= @start AND create_time <= @end AND device_id IN (")
                .Append(string.Join(",", paramNames))
                .Append(")");

            await using var command = connection.CreateCommand();
            command.CommandText = sql.ToString();
            AddParam(command, "@start", start);
            AddParam(command, "@end", end);
            for (var i = 0; i < deviceIds.Count; i++)
            {
                AddParam(command, paramNames[i], deviceIds[i]);
            }

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(new GasItem
                {
                    DeviceId = reader["device_id"]?.ToString() ?? string.Empty,
                    CreateTime = reader["create_time"] as DateTime? ?? DateTime.MinValue,
                    O2 = ParseGas(reader["o2"]),
                    Ch4 = ParseGas(reader["ch4"]),
                    H2s = ParseGas(reader["h2s"]),
                    Co = ParseGas(reader["co"])
                });
            }
            return result;
        }

        /// <summary>跨库查询 caps_device_alarm 告警上报历史（同实例 MySQL）</summary>
        private async Task<List<AlarmItem>> QueryAlarmHistoryAsync(List<string> deviceIds, DateTime start, DateTime end, CancellationToken cancellationToken)
        {
            var result = new List<AlarmItem>();
            var connection = _dbContext.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken);
            }

            var dbName = _capsOptions.Database?.Replace("`", string.Empty) ?? "ry-zhdj";
            var paramNames = deviceIds.Select((_, i) => $"@d{i}").ToList();
            var sql = new StringBuilder()
                .Append($"SELECT device_id, type, type_info, create_time FROM `{dbName}`.caps_device_alarm ")
                .Append("WHERE create_time >= @start AND create_time < @end AND device_id IN (")
                .Append(string.Join(",", paramNames))
                .Append(")");

            await using var command = connection.CreateCommand();
            command.CommandText = sql.ToString();
            AddParam(command, "@start", start);
            AddParam(command, "@end", end);
            for (var i = 0; i < deviceIds.Count; i++)
            {
                AddParam(command, paramNames[i], deviceIds[i]);
            }

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(new AlarmItem
                {
                    DeviceId = reader["device_id"]?.ToString() ?? string.Empty,
                    Type = reader["type"] == DBNull.Value ? 0 : Convert.ToInt32(reader["type"]),
                    TypeInfo = reader["type_info"]?.ToString() ?? string.Empty,
                    CreateTime = reader["create_time"] as DateTime? ?? DateTime.MinValue
                });
            }
            return result;
        }

        private static decimal? ParseGas(object? value)
        {
            var text = value?.ToString();
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }
            return decimal.TryParse(text.Trim(), out var parsed) ? parsed : null;
        }

        private static void AddParam(System.Data.Common.DbCommand command, string name, object value)
        {
            var p = command.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            command.Parameters.Add(p);
        }

        #endregion
    }
}
