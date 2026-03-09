using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Venus.Face.Dto
{
    public class UserInfoDto
    {
        public string create_time {  get; set; } = string.Empty;

        public string face_token { get; set; } = string.Empty;

        public string group_id { get; set; } = string.Empty;

        public string user_info { get; set; } = string.Empty;
    }
}

