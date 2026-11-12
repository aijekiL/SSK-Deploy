using Microsoft.AspNetCore.Mvc;
using Venus.Authentication;
using Venus.Tools.Response;
using Venus.WebApi.Services;

namespace Venus.WebApi.Controllers.SecurityMonitorService
{
    /// <summary>
    /// 作业台账（有限空间作业进出登记表 / 气体监测记录表）
    /// </summary>
    public class WorkLedgerController : VenusBaseController
    {
        private readonly WorkLedgerService _workLedgerService;
        private readonly ILogger<WorkLedgerController> _logger;

        public WorkLedgerController(WorkLedgerService workLedgerService, ILogger<WorkLedgerController> logger)
        {
            _workLedgerService = workLedgerService;
            _logger = logger;
        }

        /// <summary>
        /// 获取进出登记表数据（预览）
        /// </summary>
        [HttpGet]
        public async Task<WebResponseContent> GetEntryExitLedger(Guid ProjectId, DateTime? Date, CancellationToken cancellationToken)
        {
            try
            {
                var ledger = await _workLedgerService.GetEntryExitLedgerAsync(ProjectId, Date, cancellationToken);
                return WebResponseContent.Instance.OK("获取成功", ledger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取进出登记表失败，ProjectId={ProjectId}", ProjectId);
                return WebResponseContent.Instance.Error("获取进出登记表失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 获取气体监测记录表数据（预览）
        /// </summary>
        [HttpGet]
        public async Task<WebResponseContent> GetGasLedger(Guid ProjectId, DateTime? Date, CancellationToken cancellationToken)
        {
            try
            {
                var ledger = await _workLedgerService.GetGasLedgerAsync(ProjectId, Date, cancellationToken);
                return WebResponseContent.Instance.OK("获取成功", ledger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取气体监测记录表失败，ProjectId={ProjectId}", ProjectId);
                return WebResponseContent.Instance.Error("获取气体监测记录表失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 获取告警记录表数据（预览）
        /// </summary>
        [HttpGet]
        public async Task<WebResponseContent> GetAlarmLedger(Guid ProjectId, DateTime? Date, CancellationToken cancellationToken)
        {
            try
            {
                var ledger = await _workLedgerService.GetAlarmLedgerAsync(ProjectId, Date, cancellationToken);
                return WebResponseContent.Instance.OK("获取成功", ledger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取告警记录表失败，ProjectId={ProjectId}", ProjectId);
                return WebResponseContent.Instance.Error("获取告警记录表失败：" + ex.Message);
            }
        }

        public class SaveLedgerHeaderDto
        {
            public Guid ProjectId { get; set; }
            public string Unit { get; set; } = string.Empty;
            public string Manager { get; set; } = string.Empty;
            public string Location { get; set; } = string.Empty;
            public string Guardian { get; set; } = string.Empty;
            public string Administrator { get; set; } = string.Empty;
        }

        /// <summary>
        /// 保存台账表头（作业单位/工作负责人/作业地点/专职监护人/项目管理员）
        /// </summary>
        [HttpPost]
        public async Task<WebResponseContent> SaveLedgerHeader([FromBody] SaveLedgerHeaderDto dto)
        {
            try
            {
                await _workLedgerService.SaveLedgerHeaderAsync(dto.ProjectId, dto.Unit, dto.Manager, dto.Location, dto.Guardian, dto.Administrator, CancellationToken.None);
                return WebResponseContent.Instance.OK("保存成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存台账表头失败，ProjectId={ProjectId}", dto.ProjectId);
                return WebResponseContent.Instance.Error("保存失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 导出进出登记表 Word
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportEntryExitWord(Guid ProjectId, DateTime? Date, CancellationToken cancellationToken)
        {
            var ledger = await _workLedgerService.GetEntryExitLedgerAsync(ProjectId, Date, cancellationToken);
            var bytes = await _workLedgerService.BuildEntryExitWordAsync(ledger, cancellationToken);
            return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                $"有限空间作业进出登记表-{ledger.Header.Location}-{DateTime.Now:yyyyMMddHHmmss}.docx");
        }

        /// <summary>
        /// 导出气体监测记录表 Word
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportGasWord(Guid ProjectId, DateTime? Date, CancellationToken cancellationToken)
        {
            var ledger = await _workLedgerService.GetGasLedgerAsync(ProjectId, Date, cancellationToken);
            var bytes = await _workLedgerService.BuildGasWordAsync(ledger, cancellationToken);
            return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                $"有限空间作业过程中气体监测记录表-{ledger.Header.Location}-{DateTime.Now:yyyyMMddHHmmss}.docx");
        }

        /// <summary>
        /// 导出告警记录表 Word
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportAlarmWord(Guid ProjectId, DateTime? Date, CancellationToken cancellationToken)
        {
            var ledger = await _workLedgerService.GetAlarmLedgerAsync(ProjectId, Date, cancellationToken);
            var bytes = await _workLedgerService.BuildAlarmWordAsync(ledger, cancellationToken);
            return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                $"有限空间作业告警记录表-{ledger.Header.Location}-{DateTime.Now:yyyyMMddHHmmss}.docx");
        }
    }
}
