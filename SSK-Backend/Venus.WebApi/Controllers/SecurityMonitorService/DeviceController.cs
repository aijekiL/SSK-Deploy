using Microsoft.AspNetCore.Mvc;
using Venus.Authentication;
using Venus.Authentication.User;
using Venus.Entitys;
using Venus.Entitys.SecurityMonitorService;
using Venus.Service.EfCore;
using Venus.Tools.Request;
using Venus.Tools.Response;

namespace Venus.WebApi.Controllers.SecurityMonitorService
{
    /// <summary>
    /// 设备信息
    /// </summary>
    public class DeviceController : VenusBaseController
    {

        private VenusDbContext _DbContext;
        public DeviceController(VenusDbContext dbContext)
        {
            _DbContext = dbContext;
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
    }
}

