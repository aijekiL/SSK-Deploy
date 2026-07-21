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
    }
}

