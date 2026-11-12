using System.ComponentModel.DataAnnotations;

namespace Venus.Entitys.SecurityMonitorService
{
    /// <summary>
    /// 人员进出登记日志（作业台账-有限空间作业进出登记表数据源，退场后SM_StaffInfo会被删除，故单独留痕）
    /// </summary>
    public class SM_StaffInOutLog : DefaultBaseEntity
    {
        [Key]
        public Guid ObjectId { get; set; }

        /// <summary>
        /// 项目ID-SM_ProjectInfo-ObjectId
        /// </summary>
        public Guid ProjectId { get; set; }

        /// <summary>
        /// 人员入场登记ID-SM_StaffInfo-ObjectId
        /// </summary>
        public Guid StaffInfoId { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 联系电话
        /// </summary>
        [MaxLength(50)]
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// 工作单位
        /// </summary>
        [MaxLength(100)]
        public string Company { get; set; } = string.Empty;

        /// <summary>
        /// 工作部门
        /// </summary>
        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;

        /// <summary>
        /// 携带工器具及数量汇总
        /// </summary>
        [MaxLength(500)]
        public string Tools { get; set; } = string.Empty;

        /// <summary>
        /// 绑定的智能安全帽设备ID（入场时）
        /// </summary>
        [MaxLength(50)]
        public string? Device1 { get; set; }

        /// <summary>
        /// 入场时间
        /// </summary>
        public DateTime EnterTime { get; set; }

        /// <summary>
        /// 退场时间（null=尚未退场）
        /// </summary>
        public DateTime? ExitTime { get; set; }
    }
}
