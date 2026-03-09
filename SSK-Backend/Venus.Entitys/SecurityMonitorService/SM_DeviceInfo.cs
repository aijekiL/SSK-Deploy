using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Venus.Entitys.SecurityMonitorService
{
    /// <summary>
    /// 设备信息表
    /// </summary>
    [Table("SM_DeviceInfo")]
    public class SM_DeviceInfo : DefaultBaseEntity
    {
        /// <summary>
        /// 设备ID
        /// </summary>
        [Key]
        public Guid DeviceId { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        [MaxLength(50)]
        public string DeviceName { get; set; } = string.Empty;

        /// <summary>
        /// 设备编号
        /// </summary>
        [MaxLength(50)]
        public string DeviceNumber { get; set; } = string.Empty;

        /// <summary>
        /// 设备类型
        /// </summary>
        [MaxLength(50)]
        public string DeviceType { get; set; } = string.Empty;

        /// <summary>
        /// 所属用户
        /// </summary>
        public Guid? BelongUserId { get; set; }
    }
}

