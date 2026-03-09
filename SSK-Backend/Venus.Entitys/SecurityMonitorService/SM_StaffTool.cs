using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Venus.Entitys.SecurityMonitorService
{
    /// <summary>
    /// 员工工具类
    /// </summary>
    public class SM_StaffTool: DefaultBaseEntity
    {
        [Key]
        public Guid ObjectId { get; set; }

        /// <summary>
        /// 员工ID-SM_StaffInfo.ObjectId
        /// </summary>
        public Guid ParentId {  get; set; }

        /// <summary>
        /// 工具名称
        /// </summary>
        public string ToolName { get; set; } = string.Empty;

        /// <summary>
        /// 工具数量
        /// </summary>
        public int ToolNumber { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }


    }
}

