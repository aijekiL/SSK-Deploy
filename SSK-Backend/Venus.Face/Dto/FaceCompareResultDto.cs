using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Venus.Face.Dto
{
    public class FaceCompareResultDto
    {
        public string group_id {  get; set; } = string.Empty;

        /// <summary>
        /// 人脸置信度
        /// </summary>
        public decimal? score { get; set; }

        public string user_id { get; set; } = string.Empty;

        public string? user_info { get; set; }
    }
}

