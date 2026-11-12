using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Venus.Entitys;
using Venus.Entitys.SecurityMonitorService;

namespace Venus.WebApi.Services
{
    /// <summary>
    /// 视频回放清理配置
    /// </summary>
    public sealed class VideoPlaybackOptions
    {
        /// <summary>
        /// 录像保留天数（1=仅保留当天，即每天零点前的片段全部清理）
        /// </summary>
        public int RetainDays { get; set; } = 1;

        /// <summary>
        /// 清理检查间隔（分钟）
        /// </summary>
        public int CleanupIntervalMinutes { get; set; } = 5;
    }

    /// <summary>
    /// 视频回放片段清理服务：定时删除超过保留时长的片段（调用 ZLM 删除录像文件并移除元数据）
    /// </summary>
    public class VideoPlaybackCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly VideoPlaybackOptions _options;
        private readonly ILogger<VideoPlaybackCleanupService> _logger;

        public VideoPlaybackCleanupService(IServiceScopeFactory scopeFactory, IOptions<VideoPlaybackOptions> options, ILogger<VideoPlaybackCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options.Value ?? new VideoPlaybackOptions();
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 启动延迟，等待数据库/依赖就绪
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            var interval = TimeSpan.FromMinutes(Math.Max(1, _options.CleanupIntervalMinutes));
            using var timer = new PeriodicTimer(interval);
            do
            {
                try
                {
                    await CleanupAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "视频回放清理任务执行失败");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task CleanupAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<VenusDbContext>();
            var zlmClient = scope.ServiceProvider.GetRequiredService<ZlmClient>();

            // 保留最近 N 天（1=仅保留当天）：清理阈值 = 今天零点往前推 N-1 天
            var today = DateTime.Today;
            var expireBefore = today.AddDays(-(Math.Max(1, _options.RetainDays) - 1));
            var expiredClips = await dbContext.Set<SM_VideoClip>()
                .Where(x => !x.Deleted && x.EndTime < expireBefore)
                .OrderBy(x => x.EndTime)
                .Take(200)
                .ToListAsync(cancellationToken);

            if (expiredClips.Count == 0)
            {
                return;
            }

            var deleted = 0;
            foreach (var clip in expiredClips)
            {
                var startUnix = new DateTimeOffset(clip.StartTime).ToUnixTimeSeconds();
                var endUnix = new DateTimeOffset(clip.EndTime).ToUnixTimeSeconds();
                // 先调 ZLM 删除文件，失败也移除记录（避免记录无限堆积，文件由运维排查）
                var fileDeleted = await zlmClient.DeleteRecordMp4Async(clip.App, clip.DeviceId, startUnix, endUnix, cancellationToken);
                if (!fileDeleted)
                {
                    _logger.LogWarning("ZLM 删除录像文件未确认成功：DeviceId={DeviceId}, Period={Start}_{End}", clip.DeviceId, startUnix, endUnix);
                }
                dbContext.Set<SM_VideoClip>().Remove(clip);
                deleted++;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("视频回放清理完成：过期阈值 {ExpireBefore:yyyy-MM-dd HH:mm:ss}，删除片段 {Deleted} 个", expireBefore, deleted);
        }
    }
}
