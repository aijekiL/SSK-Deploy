using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Venus.Entitys
{
    /// <summary>
    /// 用户表
    /// </summary>
    [Table("Sys_User")]
    public class Sys_User : DefaultBaseEntity
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [Key]
        public Guid UserId { get; set; }

        /// <summary>
        /// 登录名
        /// </summary>
        [StringLength(100)]
        public string loginName { get; set; } = string.Empty;

        /// <summary>
        /// 用户名
        /// </summary>
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 密码
        /// </summary>
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 性别
        /// </summary>
        public int Sex { get; set; }

        /// <summary>
        /// 登录次数
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 是否为系统用户
        /// </summary>
        public bool SystemUser { get; set; }

        /// <summary>
        /// 最后登录时间
        /// </summary>
        public DateTime LastLoginDate { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int RecordStatus { get; set; }
    }
}

