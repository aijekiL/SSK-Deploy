using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Venus.Entitys.SecurityMonitorService
{
    /// <summary>
    /// 员工档案
    /// </summary>
    public class SM_EmployeeProfile : DefaultBaseEntity
    {
        [Key]
        public Guid ObjectId { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string name { get; set; } = string.Empty;

        /// <summary>
        /// 性别
        /// </summary>
        public string gender { get; set; } = string.Empty;

        /// <summary>
        /// 身份证号
        /// </summary>
        public string id { get; set; } = string.Empty;

        /// <summary>
        /// 联系方式
        /// </summary>
        public string phone { get; set; } = string.Empty;

        /// <summary>
        /// 工作单位
        /// </summary>
        public string company { get; set; } = string.Empty;

        /// <summary>
        /// 工作部门
        /// </summary>
        public string department { get; set; } = string.Empty;

        /// <summary>
        /// 所在项目id
        /// </summary>
        public Guid? projectId { get; set; }

        /// <summary>
        /// 设备Id（智能安全帽）
        /// </summary>
        [MaxLength(50)]
        public string? Device1 { get; set; }

        /// <summary>
        /// 手表IMEI
        /// </summary>
        [MaxLength(32)]
        public string? WatchImei { get; set; }
        /// <summary>
        /// CompreFace中的当前人脸图片ID
        /// </summary>
        [MaxLength(100)]
        public string? FaceImageId { get; set; }

        /// <summary>
        /// 人脸同步状态：0=未同步，1=成功，2=失败
        /// </summary>
        public int FaceSyncStatus { get; set; }

        /// <summary>
        /// 最近一次人脸同步结果
        /// </summary>
        [MaxLength(500)]
        public string? FaceSyncMessage { get; set; }


    }
}

