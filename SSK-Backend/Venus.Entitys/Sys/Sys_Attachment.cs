using System.ComponentModel.DataAnnotations;

namespace Venus.Entitys.Sys
{
    /// <summary>
    /// 附件表
    /// </summary>
    public class Sys_Attachment : DefaultBaseEntity
    {
        /// <summary>
        /// 附件ID
        /// </summary>
        [Key]
        public Guid ObjectId { get; set; }

        /// <summary>
        /// 业务表ID
        /// </summary>
        public string? BizObjectId { get; set; }
        /// <summary>
        /// 业务表名
        /// </summary>
        public string BizObjectTable { get; set; } = string.Empty;

        /// <summary>
        /// 业务字段名
        /// </summary>
        public string DataField { get; set; } = string.Empty;

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// 文件类型
        /// </summary>
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// 文件大小
        /// </summary>
        public long ContentLength { get; set; }

        /// <summary>
        /// 文件扩展名
        /// </summary>
        public string FileExt { get; set; } = string.Empty;

        /// <summary>
        /// MongoId
        /// </summary>
        public string MongoId { get; set; } = string.Empty;
    }
}

