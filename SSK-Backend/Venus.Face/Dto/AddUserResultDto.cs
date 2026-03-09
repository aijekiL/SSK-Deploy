using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Venus.Face.Dto
{
    public class AddUserResultDto
    {
        /// <summary>
        /// 
        /// </summary>
        public string create_time { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string face_token { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string group_id { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string user_info { get; set; } = string.Empty;
    }
}

