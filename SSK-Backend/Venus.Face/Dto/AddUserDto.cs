using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Venus.Face.Dto
{
    public class face_AddUserDto
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string user_id { get; set; } = string.Empty;

        /// <summary>
        /// 分组ID
        /// </summary>
        public string group_id { get; set; } = string.Empty;

        /// <summary>
        /// 照片路径
        /// </summary>
        public string? file_name { get; set; }

        /// <summary>
        /// 图片字节
        /// </summary>
        public byte[]? ImageBytes { get; set; }

        /// <summary>
        /// 用户信息
        /// </summary>
        public string user_info { get; set; } = string.Empty;
    }
}

