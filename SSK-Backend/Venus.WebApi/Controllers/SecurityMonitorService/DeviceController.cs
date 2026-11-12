using Microsoft.AspNetCore.Mvc;
using Venus.Authentication;
using Venus.Authentication.User;
using Venus.Entitys;
using Venus.Entitys.SecurityMonitorService;
using Venus.Service.EfCore;
using Venus.Tools.Request;
using Venus.Tools.Response;
using Venus.WebApi.Services;

namespace Venus.WebApi.Controllers.SecurityMonitorService
{
    /// <summary>
    /// 设备信息
    /// </summary>
    public class DeviceController : VenusBaseController
    {

        private VenusDbContext _DbContext;
        private readonly WatchConnectionRegistry _watchConnectionRegistry;
        public DeviceController(VenusDbContext dbContext, WatchConnectionRegistry watchConnectionRegistry)
        {
            _DbContext = dbContext;
            _watchConnectionRegistry = watchConnectionRegistry;
        }
        /// <summary>
        /// 获取设备列表
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        [HttpPost]
        public WebResponseContent GetPageDataList([FromBody] PageDataOptions options)
        {
            var query = _DbContext.Set<SM_DeviceInfo>();
            var data = query.GetPageDataOptions(options);
            return WebResponseContent.Instance.OK("成功", data);
        }

        //保存设备信息
        public class SaveDto
        {
            public Guid? DeviceId { get; set; }
            public string DeviceName { get; set; } = string.Empty;
            public string DeviceNumber { get; set; } = string.Empty;
            public string DeviceType { get; set; } = string.Empty;
            public Guid? BelongUserId { get; set; }
        }

        /// <summary>
        /// 保存设备信息
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public WebResponseContent Save([FromBody] SaveDto dto)
        {
            SM_DeviceInfo? device = null;
            if (dto.DeviceId != null)
            {
                device = _DbContext.Set<SM_DeviceInfo>().FirstOrDefault(x => x.DeviceId == dto.DeviceId);
            }
            if (device == null)
            {
                //新增
                device = new SM_DeviceInfo();
                device.DeviceId = Guid.NewGuid();
                device.DeviceName = dto.DeviceName;
                device.DeviceNumber = dto.DeviceNumber;
                device.DeviceType = dto.DeviceType;
                device.BelongUserId = dto.BelongUserId;
                device.CreateID = UserContext.Current.UserId;
                device.CreateDate = DateTime.Now;
                _DbContext.Set<SM_DeviceInfo>().Add(device);
            }
            else
            {
                //编辑
                device.DeviceName = dto.DeviceName;
                device.DeviceNumber = dto.DeviceNumber;
                device.DeviceType = dto.DeviceType;
                device.BelongUserId = dto.BelongUserId;
                device.ModifyID = UserContext.Current.UserId;
                device.ModifyDate = DateTime.Now;
                _DbContext.Set<SM_DeviceInfo>().Update(device);
            }
            var i = _DbContext.SaveChanges();
            return WebResponseContent.Instance.OK(i > 0 ? "保存成功" : "保存失败");
        }

        /// <summary>
        /// 获取人员最新手表健康数据
        /// </summary>
        /// <param name="staffId"></param>
        /// <param name="watchImei"></param>
        /// <returns></returns>
        [HttpGet]
        public WebResponseContent WatchLatest(Guid? staffId, string? watchImei)
        {
            var normalizedWatchImei = string.IsNullOrWhiteSpace(watchImei) ? null : watchImei.Trim();
            SM_StaffInfo? staff = null;

            if (staffId.HasValue && staffId.Value != Guid.Empty)
            {
                staff = _DbContext.Set<SM_StaffInfo>().FirstOrDefault(x => x.ObjectId == staffId.Value);
                normalizedWatchImei ??= staff?.WatchImei;
            }

            if (string.IsNullOrWhiteSpace(normalizedWatchImei))
            {
                return WebResponseContent.Instance.OK("未绑定手表", null);
            }

            var report = _DbContext.Set<SM_WatchHealthReport>()
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

        /// <summary>
        /// 获取手表TCP连接状态
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public WebResponseContent WatchConnectionStatus()
        {
            return WebResponseContent.Instance.OK("获取成功", _watchConnectionRegistry.GetAll());
        }

        /// <summary>
        /// 设置手表上报频率下行指令参数
        /// </summary>
        public class SetWatchPeriodDto
        {
            /// <summary>
            /// 手表IMEI，与 staffId 二选一
            /// </summary>
            public string? WatchImei { get; set; }

            /// <summary>
            /// 人员Id，未传 WatchImei 时按人员档案中绑定的手表IMEI下发
            /// </summary>
            public Guid? StaffId { get; set; }

            /// <summary>
            /// 上报/检测周期，单位：分钟
            /// </summary>
            public int Minutes { get; set; } = 5;
        }

        /// <summary>
        /// 下发手表健康采样上报频率指令(0x1C，W300PGW V2.0 文档 5.1.4)，设备在线立即下发，离线则缓存待其登录后补发
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<WebResponseContent> SetWatchReportPeriod([FromBody] SetWatchPeriodDto dto)
        {
            var (imei, error) = ResolveWatchImei(dto.WatchImei, dto.StaffId);
            if (error != null)
            {
                return error;
            }

            if (dto.Minutes < 2 || dto.Minutes > 1440)
            {
                return WebResponseContent.Instance.Error("周期需在 2~1440 分钟之间（设备最高支持 2 分钟）");
            }

            var frame = WatchDownlinkCommands.SetHealthSamplePeriod((ushort)dto.Minutes);
            var (sent, queued) = await _watchConnectionRegistry.SendOrQueueAsync(imei!, frame);

            return WebResponseContent.Instance.OK(sent ? "指令已下发" : "手表当前不在线，指令已缓存，待其上线后自动补发", new
            {
                watchImei = imei,
                minutes = dto.Minutes,
                online = sent,
                queued,
                frameHex = Convert.ToHexString(frame)
            });
        }

        /// <summary>
        /// 设置心率报警门限下行指令参数
        /// </summary>
        public class SetWatchHeartRateThresholdDto
        {
            public string? WatchImei { get; set; }

            public Guid? StaffId { get; set; }

            /// <summary>
            /// 心率上限
            /// </summary>
            public int Upper { get; set; } = 120;

            /// <summary>
            /// 心率下限
            /// </summary>
            public int Lower { get; set; } = 50;
        }

        /// <summary>
        /// 下发手表心率报警门限指令(0x19)
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<WebResponseContent> SetWatchHeartRateThreshold([FromBody] SetWatchHeartRateThresholdDto dto)
        {
            var (imei, error) = ResolveWatchImei(dto.WatchImei, dto.StaffId);
            if (error != null)
            {
                return error;
            }

            if (dto.Upper < 1 || dto.Upper > 250 || dto.Lower < 1 || dto.Lower > 250 || dto.Lower >= dto.Upper)
            {
                return WebResponseContent.Instance.Error("心率门限不合法，需满足 1≤下限<上限≤250");
            }

            var frame = WatchDownlinkCommands.SetHeartRateThreshold((ushort)dto.Upper, (ushort)dto.Lower);
            var (sent, queued) = await _watchConnectionRegistry.SendOrQueueAsync(imei!, frame);

            return WebResponseContent.Instance.OK(sent ? "指令已下发" : "手表当前不在线，指令已缓存，待其上线后自动补发", new
            {
                watchImei = imei,
                upper = dto.Upper,
                lower = dto.Lower,
                online = sent,
                queued,
                frameHex = Convert.ToHexString(frame)
            });
        }

        private (string? Imei, WebResponseContent? Error) ResolveWatchImei(string? watchImei, Guid? staffId)
        {
            var normalized = string.IsNullOrWhiteSpace(watchImei) ? null : watchImei.Trim();

            if (string.IsNullOrEmpty(normalized) && staffId.HasValue && staffId.Value != Guid.Empty)
            {
                normalized = _DbContext.Set<SM_StaffInfo>()
                    .FirstOrDefault(x => x.ObjectId == staffId.Value)?.WatchImei;
            }

            if (string.IsNullOrWhiteSpace(normalized))
            {
                return (null, WebResponseContent.Instance.Error("未找到手表IMEI，请传入 watchImei 或已绑定手表的人员 staffId"));
            }

            return (normalized, null);
        }
    }
}

