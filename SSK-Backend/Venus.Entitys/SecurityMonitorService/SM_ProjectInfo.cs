using System.ComponentModel.DataAnnotations;

namespace Venus.Entitys.SecurityMonitorService
{
    /// <summary>
    /// 项目信息表
    /// </summary>
    public class SM_ProjectInfo : DefaultBaseEntity
    {
        /// <summary>
        /// 项目ID
        /// </summary>
        [Key]
        public Guid ObjectId { get; set; }

        /// <summary>
        /// 项目名称
        /// </summary>
        public string ProjectName { get; set; } = string.Empty;


        /// <summary>
        /// 负责人姓名
        /// </summary>
        public string ManagerName { get; set; } = string.Empty;

        /// <summary>
        /// 负责人手机号
        /// </summary>
        public string ManagerPhone { get; set; } = string.Empty;

        /// <summary>
        /// 专职监护人
        /// </summary>
        public string? GuardianName { get; set; }

        /// <summary>
        /// 项目管理员
        /// </summary>
        public string? AdministratorName { get; set; }

        /// <summary>
        /// 作业单位（台账可编辑覆盖；为空时按人员工作单位/工作部门推导）
        /// </summary>
        public string? UnitName { get; set; }

        /// <summary>
        /// 开工时间
        /// </summary>
        public DateTime StartWorkDate { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 是否当前项目
        /// </summary>
        public bool IsCurrent { get; set;}
    }
}

