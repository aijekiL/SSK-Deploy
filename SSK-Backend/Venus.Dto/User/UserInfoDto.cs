using System.ComponentModel.DataAnnotations;

namespace Venus.Dto.User
{
    public class UserInfoDto
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public Guid userId { get; set; }

        /// <summary>
        /// 登录名
        /// </summary>
        public string loginName { get; set; } = string.Empty;

        /// <summary>
        /// 用户名
        /// </summary>
        public string Name { get; set; } = string.Empty;

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
        /// 头像ID CF_Avatar.Id
        /// </summary>
        public Guid AvatarID { get; set; }

        /// <summary>
        /// 最后登录时间
        /// </summary>
        public DateTime LastLoginDate { get; set; }
    }
}

