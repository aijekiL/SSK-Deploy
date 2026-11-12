using System.ComponentModel.DataAnnotations;

namespace Venus.Entitys.SecurityMonitorService
{
    /// <summary>
    /// 视频回放片段（ZLMediaKit on_record_mp4 回调登记，按流/设备ID存储，关联人员快照）
    /// </summary>
    public class SM_VideoClip : DefaultBaseEntity
    {
        [Key]
        public Guid ObjectId { get; set; }

        /// <summary>
        /// 设备ID（ZLM streamId，即智能安全帽设备号）
        /// </summary>
        [MaxLength(64)]
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>
        /// ZLM 应用名（如 live）
        /// </summary>
        [MaxLength(64)]
        public string App { get; set; } = string.Empty;

        /// <summary>
        /// 相对 ZLM www 根目录的文件路径（用于拼接播放/下载URL）
        /// </summary>
        [MaxLength(500)]
        public string RelPath { get; set; } = string.Empty;

        /// <summary>
        /// 文件名
        /// </summary>
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// 片段开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 片段结束时间
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 时长（秒）
        /// </summary>
        public int DurationSeconds { get; set; }

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// 关联工程ID（录制时人员所在工程）
        /// </summary>
        public Guid? ProjectId { get; set; }

        /// <summary>
        /// 关联人员入场登记ID
        /// </summary>
        public Guid? StaffInfoId { get; set; }

        /// <summary>
        /// 人员姓名（快照）
        /// </summary>
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 人员证件号/工号（快照）
        /// </summary>
        [MaxLength(50)]
        public string No { get; set; } = string.Empty;

        /// <summary>
        /// 联系电话（快照）
        /// </summary>
        [MaxLength(50)]
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// 工作单位（快照）
        /// </summary>
        [MaxLength(100)]
        public string Company { get; set; } = string.Empty;

        /// <summary>
        /// 工作部门（快照）
        /// </summary>
        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;
    }
}
