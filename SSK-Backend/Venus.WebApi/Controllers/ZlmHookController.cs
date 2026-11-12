using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Venus.Entitys;
using Venus.Entitys.SecurityMonitorService;
using Venus.WebApi.Services;

namespace Venus.WebApi.Controllers
{
    /// <summary>
    /// ZLMediaKit Hook 回调接收（匿名访问，按 secret 校验）
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [AllowAnonymous]
    public class ZlmHookController : ControllerBase
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        private readonly VenusDbContext _dbContext;
        private readonly ZlmOptions _zlmOptions;
        private readonly ILogger<ZlmHookController> _logger;

        public ZlmHookController(VenusDbContext dbContext, IOptions<ZlmOptions> zlmOptions, ILogger<ZlmHookController> logger)
        {
            _dbContext = dbContext;
            _zlmOptions = zlmOptions.Value ?? new ZlmOptions();
            _logger = logger;
        }

        /// <summary>
        /// MP4 录制完成回调：登记视频回放片段元数据（含人员快照）
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> OnRecordMp4(CancellationToken cancellationToken)
        {
            // ZLM on_record_mp4 事件回调不会附加 admin_params（secret）；
            // 仅在显式携带 secret 时才校验，避免把正常回调误拒。
            var secret = Request.Query["secret"].ToString();
            if (!string.IsNullOrWhiteSpace(secret) && secret != _zlmOptions.Secret)
            {
                _logger.LogWarning("ZLM on_record_mp4 回调 secret 校验失败，收到 {Secret}", secret);
                return Ok(new { code = -1, msg = "secret校验失败" });
            }

            JsonDocument payload;
            try
            {
                payload = await JsonDocument.ParseAsync(Request.Body, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ZLM on_record_mp4 回调参数解析失败");
                return Ok(new { code = 0, msg = "success" });
            }

            using (payload)
            {
                var root = payload.RootElement;
                var app = GetString(root, "app");
                var stream = GetString(root, "stream");
                var filePath = GetString(root, "file_path");
                var fileName = GetString(root, "file_name", GetString(root, "file_base_name"));
                if (string.IsNullOrWhiteSpace(stream) || string.IsNullOrWhiteSpace(filePath))
                {
                    return Ok(new { code = 0, msg = "success" });
                }

                var startUnix = GetLong(root, "start_time");
                var duration = GetLong(root, "file_time");
                if (duration <= 0)
                {
                    duration = GetLong(root, "time_len");
                }
                var fileSize = GetLong(root, "file_size");
                var startTime = startUnix > 0
                    ? DateTimeOffset.FromUnixTimeSeconds(startUnix).LocalDateTime
                    : DateTime.Now.AddSeconds(-Math.Max(duration, 0));
                if (duration <= 0)
                {
                    duration = Math.Max(0, (long)(DateTime.Now - startTime).TotalSeconds);
                }
                var endTime = startTime.AddSeconds(duration);

                var relPath = ToRelPath(filePath);
                if (string.IsNullOrWhiteSpace(relPath))
                {
                    _logger.LogWarning("无法从录像路径解析相对路径：{FilePath}", filePath);
                    return Ok(new { code = 0, msg = "success" });
                }

                // 同一文件去重（ZLM 重试可能重复回调）
                var exists = await _dbContext.Set<SM_VideoClip>()
                    .AnyAsync(x => x.DeviceId == stream && x.StartTime == startTime, cancellationToken);
                if (!exists)
                {
                    var staff = await ResolveStaffAsync(stream, cancellationToken);
                    _dbContext.Set<SM_VideoClip>().Add(new SM_VideoClip
                    {
                        ObjectId = Guid.NewGuid(),
                        DeviceId = stream,
                        App = string.IsNullOrWhiteSpace(app) ? "live" : app,
                        RelPath = relPath,
                        FileName = string.IsNullOrWhiteSpace(fileName) ? Path.GetFileName(relPath) : fileName,
                        StartTime = startTime,
                        EndTime = endTime,
                        DurationSeconds = (int)duration,
                        FileSizeBytes = fileSize,
                        ProjectId = staff?.ProjectId,
                        StaffInfoId = staff?.ObjectId,
                        Name = staff?.name ?? string.Empty,
                        No = staff?.id ?? string.Empty,
                        Phone = staff?.phone ?? string.Empty,
                        Company = staff?.company ?? string.Empty,
                        Department = staff?.department ?? string.Empty,
                        CreateID = Guid.Empty,
                        CreateDate = DateTime.Now
                    });
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }

                return Ok(new { code = 0, msg = "success" });
            }
        }

        /// <summary>
        /// 按设备ID解析人员：优先在场人员，其次最近一次进出登记
        /// </summary>
        private async Task<SM_StaffInfo?> ResolveStaffAsync(string deviceId, CancellationToken cancellationToken)
        {
            var normalized = deviceId.Trim();
            var staff = await _dbContext.Set<SM_StaffInfo>()
                .FirstOrDefaultAsync(x => x.Device1 == normalized && x.Status == 2, cancellationToken);
            if (staff != null)
            {
                return staff;
            }

            var recentLog = await _dbContext.Set<SM_StaffInOutLog>()
                .Where(x => x.Device1 == normalized)
                .OrderByDescending(x => x.EnterTime)
                .FirstOrDefaultAsync(cancellationToken);
            if (recentLog != null)
            {
                return new SM_StaffInfo
                {
                    ObjectId = recentLog.StaffInfoId,
                    ProjectId = recentLog.ProjectId,
                    name = recentLog.Name,
                    id = string.Empty,
                    phone = recentLog.Phone,
                    company = recentLog.Company,
                    department = recentLog.Department
                };
            }
            return null;
        }

        /// <summary>从绝对路径提取相对 ZLM www 根目录的路径</summary>
        private static string ToRelPath(string filePath)
        {
            var normalized = filePath.Replace('\\', '/');
            var marker = "/www/";
            var index = normalized.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                return normalized[(index + marker.Length)..];
            }
            // 已是相对路径
            if (!normalized.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return normalized.TrimStart('/');
            }
            return string.Empty;
        }

        private static string GetString(JsonElement root, params string[] names)
        {
            foreach (var name in names)
            {
                if (root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
                {
                    var text = value.GetString();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        return text;
                    }
                }
                else if (root.TryGetProperty(name, out value) && value.ValueKind == JsonValueKind.Number)
                {
                    return value.ToString();
                }
            }
            return string.Empty;
        }

        private static long GetLong(JsonElement root, params string[] names)
        {
            foreach (var name in names)
            {
                if (!root.TryGetProperty(name, out var value))
                {
                    continue;
                }
                if (value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var number))
                {
                    return number;
                }
                if (value.ValueKind == JsonValueKind.String && long.TryParse(value.GetString(), out var parsed))
                {
                    return parsed;
                }
            }
            return 0;
        }
    }
}
