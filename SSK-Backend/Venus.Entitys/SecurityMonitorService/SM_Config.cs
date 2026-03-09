using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Venus.Entitys.SecurityMonitorService
{
    /// <summary>
    /// 设备配置
    /// </summary>
    public class SM_Config:BaseEntity
    {
        [Key]
        public Guid ObjectId { get; set; }


        public string Code {  get; set; } = string.Empty;


        public string Value {  get; set; } = string.Empty;
    }
}

