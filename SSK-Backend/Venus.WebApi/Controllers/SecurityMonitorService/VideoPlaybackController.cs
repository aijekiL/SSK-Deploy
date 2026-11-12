using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Venus.Authentication;
using Venus.Entitys;
using Venus.Entitys.SecurityMonitorService;
using Venus.Service.EfCore;
using Venus.Tools.Request;
using Venus.Tools.Response;
using Venus.WebApi.Services;

namespace Venus.WebApi.Controllers.SecurityMonitorService
{
    /// <summary>
    /// 视频回放（ZLM 录制片段查询/下载）
    /// </summary>
    public class VideoPlaybackController : VenusBaseController
    {
        private readonly VenusDbContext _dbContext;
        private readonly ZlmClient _zlmClient;
        private readonly ILogger<VideoPlaybackController> _logger;

        public VideoPlaybackController(VenusDbContext dbContext, ZlmClient zlmClient, ILogger<VideoPlaybackController> logger)
        {
            _dbContext = dbContext;
            _zlmClient = zlmClient;
            _logger = logger;
        }

        /// <summary>
        /// 分页查询视频回放片段（filter 支持 Name姓名/No工号/Department部门/Company单位/DeviceId设备，模糊匹配）
        /// </summary>
        [HttpPost]
        public WebResponseContent GetPageList([FromBody] PageDataOptions options)
        {
            options ??= new PageDataOptions();
            var page = options.Page <= 0 ? 1 : options.Page;
            var pageSize = options.pageSize <= 0 ? 10 : options.pageSize;

            var query = _dbContext.Set<SM_VideoClip>().AsNoTracking().Where(x => !x.Deleted);

            // 显式构建模糊查询条件（不依赖通用Filter机制）
            foreach (var item in options.Filter ?? new List<SearchParameters>())
            {
                if (string.IsNullOrWhiteSpace(item.Value))
                {
                    continue;
                }
                var value = item.Value.Trim();
                switch (item.Name?.Trim().ToLowerInvariant())
                {
                    case "name":
                        query = query.Where(x => x.Name.Contains(value));
                        break;
                    case "no":
                        query = query.Where(x => x.No.Contains(value));
                        break;
                    case "department":
                        query = query.Where(x => x.Department.Contains(value));
                        break;
                    case "company":
                        query = query.Where(x => x.Company.Contains(value));
                        break;
                    case "deviceid":
                        query = query.Where(x => x.DeviceId.Contains(value));
                        break;
                }
            }

            var total = query.Count();
            var rows = query
                .OrderByDescending(x => x.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return WebResponseContent.Instance.OK("获取成功", new PageGridData<SM_VideoClip>
            {
                page = page,
                pageSize = pageSize,
                total = total,
                rows = rows
            });
        }

        /// <summary>
        /// 下载视频片段（从 ZLM 拉流代理返回，带人员信息文件名）
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Download(Guid ClipId, CancellationToken cancellationToken)
        {
            var clip = await _dbContext.Set<SM_VideoClip>()
                .FirstOrDefaultAsync(x => x.ObjectId == ClipId && !x.Deleted, cancellationToken);
            if (clip == null)
            {
                return NotFound(new { status = false, message = "视频片段不存在或已过期" });
            }

            try
            {
                var stream = await _zlmClient.OpenRecordFileAsync(clip.RelPath, cancellationToken);
                var owner = string.IsNullOrWhiteSpace(clip.Name) ? clip.DeviceId : clip.Name;
                var fileName = $"{owner}-{clip.StartTime:yyyyMMddHHmmss}-{clip.DurationSeconds}s.mp4";
                Response.Headers.ContentLength = clip.FileSizeBytes > 0 ? clip.FileSizeBytes : null;
                return File(stream, "video/mp4", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "下载视频片段失败，ClipId={ClipId}, RelPath={RelPath}", clip.ObjectId, clip.RelPath);
                return NotFound(new { status = false, message = "录像文件读取失败，可能已被清理" });
            }
        }
    }
}
