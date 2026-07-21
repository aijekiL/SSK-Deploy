using System.ComponentModel.DataAnnotations;

namespace Venus.Entitys.SecurityMonitorService
{
    /// <summary>
    /// Watch health report.
    /// </summary>
    public class SM_WatchHealthReport : DefaultBaseEntity
    {
        [Key]
        public Guid ObjectId { get; set; }

        [MaxLength(32)]
        public string WatchImei { get; set; } = string.Empty;

        public Guid? StaffId { get; set; }

        public Guid? ProjectId { get; set; }

        public int? HeartRate { get; set; }

        public int? Spo2 { get; set; }

        public int? Systolic { get; set; }

        public int? Diastolic { get; set; }

        public DateTime ReportTime { get; set; }

        public string RawHex { get; set; } = string.Empty;
    }
}
