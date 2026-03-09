using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Venus.Dto.User
{
    public class LoginInfoDto
    {
        /// <summary>
        /// 加密后的账号密码
        /// </summary>
        public string? encryUserInfo {  get; set; }

        public string passWord { get; set; } = string.Empty;

        public string loginName { get; set; } = string.Empty;
    }
}

