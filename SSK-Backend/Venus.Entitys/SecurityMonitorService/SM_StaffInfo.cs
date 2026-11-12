using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Venus.Entitys.SecurityMonitorService
{
    /// <summary>
    /// 瀹夊叏鐩戞帶浜哄憳淇℃伅
    /// </summary>
    public class SM_StaffInfo: DefaultBaseEntity
    {
        [Key]
        public Guid ObjectId { get; set; }

        /// <summary>
        /// 椤圭洰ID-SM_ProjectInfo-ObjectId
        /// </summary>
        public Guid ProjectId {  get; set; }

        /// <summary>
        /// 人员信息台账ID-SM_EmployeeProfile-ObjectId
        /// </summary>
        public Guid? EmployeeProfileId { get; set; }

        /// <summary>
        /// 濮撳悕
        /// </summary>
        public string name {  get; set; } = string.Empty;

        /// <summary>
        /// 鎬у埆
        /// </summary>
        public string gender {  get; set; } = string.Empty;

        /// <summary>
        /// 韬唤璇佸彿
        /// </summary>
        public string id {  get; set; } = string.Empty;

        /// <summary>
        /// 鑱旂郴鏂瑰紡
        /// </summary>
        public string phone {  get; set; } = string.Empty;

        /// <summary>
        /// 宸ヤ綔鍗曚綅
        /// </summary>
        public string company {  get; set; } = string.Empty;

        /// <summary>
        /// 宸ヤ綔閮ㄩ棬
        /// </summary>
        public string department { get; set; } = string.Empty;

        /// <summary>
        /// 鍦ㄥ満鐘舵€?0=绂诲満 1=鐧昏涓?2=鍦ㄥ満
        /// </summary>
        public int Status { get; set; }


        /// <summary>
        /// 璁惧1
        /// </summary>
        [MaxLength(50)]
        public string? Device1 { get; set; }

        /// <summary>
        /// Watch IMEI.
        /// </summary>
        [MaxLength(32)]
        public string? WatchImei { get; set; }

        /// <summary>
        /// 褰撳墠姝ラ
        /// </summary>
        public int CurrentStep { get; set; }
    }
}
