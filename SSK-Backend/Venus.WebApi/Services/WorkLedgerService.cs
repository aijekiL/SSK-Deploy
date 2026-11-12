using System.IO.Compression;
using System.Text;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Venus.Entitys;
using Venus.Entitys.SecurityMonitorService;
using Venus.Tools;

namespace Venus.WebApi.Services
{
    /// <summary>
    /// caps 安全帽数据库配置（同实例 MySQL 跨库查询气体上报历史）
    /// </summary>
    public sealed class CapsLedgerOptions
    {
        public string Database { get; set; } = "ry-zhdj";
    }

    /// <summary>
    /// 作业台账（有限空间作业进出登记表 / 气体监测记录表）数据组装与 Word 导出
    /// </summary>
    public class WorkLedgerService : ITransientDependency
    {
        private readonly VenusDbContext _dbContext;
        private readonly CapsLedgerOptions _options;

        public WorkLedgerService(VenusDbContext dbContext, IOptions<CapsLedgerOptions> options)
        {
            _dbContext = dbContext;
            _options = options.Value ?? new CapsLedgerOptions();
        }

        #region 台账数据模型

        public class LedgerHeader
        {
            /// <summary>作业单位（人员所在部门，多个部门逗号分隔）</summary>
            public string Unit { get; set; } = string.Empty;
            /// <summary>工作负责人（工程登记负责人）</summary>
            public string Manager { get; set; } = string.Empty;
            /// <summary>专职监护人</summary>
            public string Guardian { get; set; } = string.Empty;
            /// <summary>项目管理员</summary>
            public string Administrator { get; set; } = string.Empty;
            /// <summary>作业地点/检测位置（区域名称）</summary>
            public string Location { get; set; } = string.Empty;
            /// <summary>工作内容（固定：有限空间作业）</summary>
            public string WorkContent { get; set; } = "有限空间作业";
            /// <summary>项目编号（用于 Word 模板“编号：”字段；当前无正式编号时用 ObjectId 前8位）</summary>
            public string ProjectNo { get; set; } = string.Empty;
        }

        public class EntryExitRow
        {
            public string Date { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Department { get; set; } = string.Empty;
            public string EnterTime { get; set; } = string.Empty;
            public string ExitTime { get; set; } = string.Empty;
            public string Tools { get; set; } = string.Empty;
            public string Guardian { get; set; } = string.Empty;
            public string Remark { get; set; } = string.Empty;
        }

        public class GasRow
        {
            /// <summary>氧气 %VOL</summary>
            public string O2 { get; set; } = string.Empty;
            /// <summary>可燃性气体 %LEL</summary>
            public string Ch4 { get; set; } = string.Empty;
            /// <summary>硫化氢 ppm</summary>
            public string H2s { get; set; } = string.Empty;
            /// <summary>一氧化碳 ppm</summary>
            public string Co { get; set; } = string.Empty;
            /// <summary>检测结果：合格/不合格</summary>
            public string Result { get; set; } = string.Empty;
            /// <summary>检测时间</summary>
            public string DetectTime { get; set; } = string.Empty;
            /// <summary>记录人（工程负责人）</summary>
            public string Recorder { get; set; } = string.Empty;
        }

        public class EntryExitLedger
        {
            public LedgerHeader Header { get; set; } = new();
            public List<EntryExitRow> Rows { get; set; } = new();
        }

        public class GasLedger
        {
            public LedgerHeader Header { get; set; } = new();
            public List<GasRow> Rows { get; set; } = new();
        }

        public class AlarmRow
        {
            /// <summary>报警时间</summary>
            public string Time { get; set; } = string.Empty;
            /// <summary>姓名</summary>
            public string Name { get; set; } = string.Empty;
            /// <summary>单位/部门</summary>
            public string Department { get; set; } = string.Empty;
            /// <summary>联系电话</summary>
            public string Phone { get; set; } = string.Empty;
            /// <summary>设备编号</summary>
            public string DeviceId { get; set; } = string.Empty;
            /// <summary>报警类型</summary>
            public string AlarmType { get; set; } = string.Empty;
            /// <summary>处理情况（导出后人工填写）</summary>
            public string Remark { get; set; } = string.Empty;
        }

        public class AlarmLedger
        {
            public LedgerHeader Header { get; set; } = new();
            /// <summary>统计日期（查询条件）</summary>
            public string StatDate { get; set; } = string.Empty;
            public List<AlarmRow> Rows { get; set; } = new();
        }

        private class GasHistoryItem
        {
            public string DeviceId { get; set; } = string.Empty;
            public DateTime CreateTime { get; set; }
            public decimal? O2 { get; set; }
            public decimal? Ch4 { get; set; }
            public decimal? H2s { get; set; }
            public decimal? Co { get; set; }
        }

        private class AlarmHistoryItem
        {
            public string DeviceId { get; set; } = string.Empty;
            public int Type { get; set; }
            public string TypeInfo { get; set; } = string.Empty;
            public DateTime CreateTime { get; set; }
        }

        #endregion

        #region 数据组装

        /// <summary>
        /// 组装有限空间作业进出登记表数据
        /// </summary>
        public async Task<EntryExitLedger> GetEntryExitLedgerAsync(Guid projectId, DateTime? date, CancellationToken cancellationToken)
        {
            var ledger = new EntryExitLedger
            {
                Header = await BuildHeaderAsync(projectId, date, cancellationToken)
            };
            var logs = await GetInOutLogsAsync(projectId, date, cancellationToken);
            ledger.Rows = logs
                .OrderBy(x => x.EnterTime)
                .Select(x => new EntryExitRow
                {
                    Date = x.EnterTime.ToString("yyyy年M月d日"),
                    Name = x.Name,
                    Department = string.IsNullOrWhiteSpace(x.Department) ? x.Company : x.Department,
                    EnterTime = x.EnterTime.ToString("HH:mm"),
                    ExitTime = x.ExitTime?.ToString("HH:mm") ?? string.Empty,
                    Tools = x.Tools,
                    Guardian = ledger.Header.Guardian,
                    Remark = string.Empty
                })
                .ToList();
            return ledger;
        }

        /// <summary>
        /// 组装气体监测记录表数据：设备首条有效气体数据立即记录，此后每15分钟取一次瞬时值（同时间点多设备取最不利值）
        /// </summary>
        public async Task<GasLedger> GetGasLedgerAsync(Guid projectId, DateTime? date, CancellationToken cancellationToken)
        {
            var ledger = new GasLedger
            {
                Header = await BuildHeaderAsync(projectId, date, cancellationToken)
            };
            var logs = await GetInOutLogsAsync(projectId, date, cancellationToken);
            if (logs.Count == 0)
            {
                return ledger;
            }

            var start = logs.Min(x => x.EnterTime);
            var lastExit = logs.Where(x => x.ExitTime.HasValue).Select(x => x.ExitTime!.Value).DefaultIfEmpty(start).Max();
            var end = lastExit > DateTime.Now ? lastExit : DateTime.Now;
            // 按天查询时，结束时间不超过该天末尾
            if (date.HasValue)
            {
                var dayEnd = date.Value.Date.AddDays(1);
                if (end > dayEnd) end = dayEnd;
            }

            var deviceIds = logs
                .Where(x => !string.IsNullOrWhiteSpace(x.Device1))
                .Select(x => x.Device1!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var history = deviceIds.Count > 0
                ? await QueryGasHistoryAsync(deviceIds, start.AddMinutes(-8), end.AddMinutes(8), cancellationToken)
                : new List<GasHistoryItem>();

            var byDevice = history
                .GroupBy(x => x.DeviceId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.CreateTime).ToList(), StringComparer.OrdinalIgnoreCase);

            // 以人员在场期间设备的首条有效气体上报作为第一个记录点，避免首条记录延后一个采样周期。
            var firstSample = history
                .Where(HasGasValue)
                .Where(sample => logs.Any(log =>
                    !string.IsNullOrWhiteSpace(log.Device1) &&
                    string.Equals(log.Device1.Trim(), sample.DeviceId, StringComparison.OrdinalIgnoreCase) &&
                    log.EnterTime <= sample.CreateTime &&
                    (log.ExitTime == null || log.ExitTime >= sample.CreateTime)))
                .OrderBy(x => x.CreateTime)
                .FirstOrDefault();
            if (firstSample == null)
            {
                return ledger;
            }

            for (var t = firstSample.CreateTime; t <= end; t = t.AddMinutes(15))
            {
                // 该时间点在场的人员及其设备
                var devices = logs
                    .Where(x => x.EnterTime <= t && (x.ExitTime == null || x.ExitTime >= t))
                    .Where(x => !string.IsNullOrWhiteSpace(x.Device1))
                    .Select(x => x.Device1!.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                if (devices.Count == 0)
                {
                    continue;
                }

                var samples = new List<GasHistoryItem>();
                foreach (var device in devices)
                {
                    if (!byDevice.TryGetValue(device, out var items) || items.Count == 0)
                    {
                        continue;
                    }
                    // 取距该时间点最近（±7.5分钟窗口内）的一条上报记录
                    var nearest = items
                        .Where(x => HasGasValue(x) && Math.Abs((x.CreateTime - t).TotalMinutes) <= 7.5)
                        .OrderBy(x => Math.Abs((x.CreateTime - t).Ticks))
                        .FirstOrDefault();
                    if (nearest != null)
                    {
                        samples.Add(nearest);
                    }
                }

                if (samples.Count == 0)
                {
                    continue;
                }

                var row = new GasRow
                {
                    O2 = FormatGas(WorstO2(samples)),
                    Ch4 = FormatGas(samples.Max(x => x.Ch4)),
                    H2s = FormatGas(samples.Max(x => x.H2s)),
                    Co = FormatGas(samples.Max(x => x.Co)),
                    DetectTime = t.ToString("M月d日 HH:mm"),
                    Recorder = ledger.Header.Guardian
                };
                row.Result = EvaluateGasResult(row);
                ledger.Rows.Add(row);
            }

            return ledger;
        }

        /// <summary>
        /// 组装告警记录表数据：查询当日智能安全帽上报的告警事件（caps_device_alarm），关联在场人员补全姓名/单位/电话
        /// </summary>
        public async Task<AlarmLedger> GetAlarmLedgerAsync(Guid projectId, DateTime? date, CancellationToken cancellationToken)
        {
            var ledger = new AlarmLedger
            {
                Header = await BuildHeaderAsync(projectId, date, cancellationToken),
                StatDate = (date ?? DateTime.Today).ToString("yyyy年M月d日")
            };

            var logs = await GetInOutLogsAsync(projectId, date, cancellationToken);
            if (logs.Count == 0)
            {
                return ledger;
            }

            // 当日有进出记录的全部设备（含已退场），告警只统计这些设备上报的事件
            var deviceIds = logs
                .Where(x => !string.IsNullOrWhiteSpace(x.Device1))
                .Select(x => x.Device1!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (deviceIds.Count == 0)
            {
                return ledger;
            }

            DateTime start;
            DateTime end;
            if (date.HasValue)
            {
                start = date.Value.Date;
                end = start.AddDays(1);
            }
            else
            {
                start = DateTime.Today;
                end = start.AddDays(1);
            }

            var history = await QueryAlarmHistoryAsync(deviceIds, start, end, cancellationToken);
            var byDevice = logs
                .Where(x => !string.IsNullOrWhiteSpace(x.Device1))
                .GroupBy(x => x.Device1!.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(x => x.EnterTime).First(),
                    StringComparer.OrdinalIgnoreCase);

            ledger.Rows = history
                .OrderBy(x => x.CreateTime)
                .Select(x =>
                {
                    byDevice.TryGetValue(x.DeviceId, out var log);
                    return new AlarmRow
                    {
                        Time = x.CreateTime.ToString("HH:mm:ss"),
                        Name = log?.Name ?? string.Empty,
                        Department = log == null
                            ? string.Empty
                            : (string.IsNullOrWhiteSpace(log.Department) ? log.Company : log.Department),
                        Phone = log?.Phone ?? string.Empty,
                        DeviceId = x.DeviceId,
                        AlarmType = string.IsNullOrWhiteSpace(x.TypeInfo) ? $"未知报警（{x.Type}）" : x.TypeInfo,
                        Remark = string.Empty
                    };
                })
                .ToList();
            return ledger;
        }

        /// <summary>跨库查询 caps_device_alarm 告警上报历史（同实例 ry-zhdj 库）</summary>
        private async Task<List<AlarmHistoryItem>> QueryAlarmHistoryAsync(List<string> deviceIds, DateTime start, DateTime end, CancellationToken cancellationToken)
        {
            var result = new List<AlarmHistoryItem>();
            var connection = _dbContext.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken);
            }

            var dbName = _options.Database?.Replace("`", string.Empty) ?? "ry-zhdj";
            var paramNames = deviceIds.Select((_, i) => $"@d{i}").ToList();
            var sql = new StringBuilder()
                .Append($"SELECT device_id, type, type_info, create_time FROM `{dbName}`.caps_device_alarm ")
                .Append("WHERE create_time >= @start AND create_time < @end AND device_id IN (")
                .Append(string.Join(",", paramNames))
                .Append(")");

            await using var command = connection.CreateCommand();
            command.CommandText = sql.ToString();
            var startParam = command.CreateParameter();
            startParam.ParameterName = "@start";
            startParam.Value = start;
            command.Parameters.Add(startParam);
            var endParam = command.CreateParameter();
            endParam.ParameterName = "@end";
            endParam.Value = end;
            command.Parameters.Add(endParam);
            for (var i = 0; i < deviceIds.Count; i++)
            {
                var p = command.CreateParameter();
                p.ParameterName = paramNames[i];
                p.Value = deviceIds[i];
                command.Parameters.Add(p);
            }

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(new AlarmHistoryItem
                {
                    DeviceId = reader["device_id"]?.ToString() ?? string.Empty,
                    Type = reader["type"] == DBNull.Value ? 0 : Convert.ToInt32(reader["type"]),
                    TypeInfo = reader["type_info"]?.ToString() ?? string.Empty,
                    CreateTime = reader["create_time"] as DateTime? ?? DateTime.MinValue
                });
            }
            return result;
        }

        private static bool HasGasValue(GasHistoryItem sample)
        {
            return sample.O2.HasValue || sample.Ch4.HasValue || sample.H2s.HasValue || sample.Co.HasValue;
        }

        private async Task<LedgerHeader> BuildHeaderAsync(Guid projectId, DateTime? date, CancellationToken cancellationToken)
        {
            var project = await _dbContext.Set<SM_ProjectInfo>()
                .FirstOrDefaultAsync(x => x.ObjectId == projectId, cancellationToken);
            if (project == null)
            {
                throw new InvalidOperationException("工程不存在。");
            }

            var logs = await GetInOutLogsAsync(projectId, date, cancellationToken);
            // 作业单位=人员工作单位（去重拼接），工作单位未填写时回退工作部门；台账可编辑覆盖
            var unit = string.Join("、", logs
                .Select(x => x.Company?.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct());
            if (string.IsNullOrWhiteSpace(unit))
            {
                unit = string.Join("、", logs
                    .Select(x => x.Department?.Trim())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct());
            }
            if (!string.IsNullOrWhiteSpace(project.UnitName))
            {
                unit = project.UnitName;
            }

            return new LedgerHeader
            {
                Unit = unit,
                Manager = project.ManagerName,
                // 专职监护人 / 项目管理员可在台账中编辑，未填写时回退为工作负责人
                Guardian = string.IsNullOrWhiteSpace(project.GuardianName) ? project.ManagerName : project.GuardianName,
                Administrator = string.IsNullOrWhiteSpace(project.AdministratorName) ? project.ManagerName : project.AdministratorName,
                Location = project.ProjectName,
                ProjectNo = project.ObjectId.ToString("N").Substring(0, 8).ToUpperInvariant()
            };
        }

        /// <summary>
        /// 保存作业台账表头（作业单位/工作负责人/作业地点/专职监护人/项目管理员），供台账编辑
        /// </summary>
        public async Task SaveLedgerHeaderAsync(Guid projectId, string unit, string manager, string location, string guardian, string administrator, CancellationToken cancellationToken)
        {
            var project = await _dbContext.Set<SM_ProjectInfo>()
                .FirstOrDefaultAsync(x => x.ObjectId == projectId, cancellationToken);
            if (project == null)
            {
                throw new InvalidOperationException("工程不存在。");
            }

            project.UnitName = string.IsNullOrWhiteSpace(unit) ? null : unit.Trim();
            if (!string.IsNullOrWhiteSpace(manager))
            {
                project.ManagerName = manager.Trim();
            }
            if (!string.IsNullOrWhiteSpace(location))
            {
                project.ProjectName = location.Trim();
            }
            project.GuardianName = string.IsNullOrWhiteSpace(guardian) ? null : guardian.Trim();
            project.AdministratorName = string.IsNullOrWhiteSpace(administrator) ? null : administrator.Trim();
            project.ModifyDate = DateTime.Now;
            _dbContext.Set<SM_ProjectInfo>().Update(project);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private Task<List<SM_StaffInOutLog>> GetInOutLogsAsync(Guid projectId, DateTime? date, CancellationToken cancellationToken)
        {
            var query = _dbContext.Set<SM_StaffInOutLog>()
                .Where(x => !x.Deleted && x.ProjectId == projectId);
            if (date.HasValue)
            {
                var dayStart = date.Value.Date;
                var dayEnd = dayStart.AddDays(1);
                query = query.Where(x => x.EnterTime >= dayStart && x.EnterTime < dayEnd);
            }
            return query
                .OrderBy(x => x.EnterTime)
                .ToListAsync(cancellationToken);
        }

        private async Task<List<GasHistoryItem>> QueryGasHistoryAsync(List<string> deviceIds, DateTime start, DateTime end, CancellationToken cancellationToken)
        {
            var result = new List<GasHistoryItem>();
            var connection = _dbContext.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken);
            }

            var dbName = _options.Database?.Replace("`", string.Empty) ?? "ry-zhdj";
            var paramNames = deviceIds.Select((_, i) => $"@d{i}").ToList();
            var sql = new StringBuilder()
                .Append($"SELECT device_id, o2, co, h2s, ch4, create_time FROM `{dbName}`.caps_device_report_history ")
                .Append("WHERE create_time >= @start AND create_time <= @end AND device_id IN (")
                .Append(string.Join(",", paramNames))
                .Append(")");

            await using var command = connection.CreateCommand();
            command.CommandText = sql.ToString();
            var startParam = command.CreateParameter();
            startParam.ParameterName = "@start";
            startParam.Value = start;
            command.Parameters.Add(startParam);
            var endParam = command.CreateParameter();
            endParam.ParameterName = "@end";
            endParam.Value = end;
            command.Parameters.Add(endParam);
            for (var i = 0; i < deviceIds.Count; i++)
            {
                var p = command.CreateParameter();
                p.ParameterName = paramNames[i];
                p.Value = deviceIds[i];
                command.Parameters.Add(p);
            }

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(new GasHistoryItem
                {
                    DeviceId = reader["device_id"]?.ToString() ?? string.Empty,
                    CreateTime = reader["create_time"] as DateTime? ?? DateTime.MinValue,
                    O2 = ParseGas(reader["o2"]),
                    Ch4 = ParseGas(reader["ch4"]),
                    H2s = ParseGas(reader["h2s"]),
                    Co = ParseGas(reader["co"])
                });
            }
            return result;
        }

        private static decimal? ParseGas(object? value)
        {
            var text = value?.ToString();
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }
            return decimal.TryParse(text.Trim(), out var parsed) ? parsed : null;
        }

        /// <summary>氧气最不利值：偏离20.9%最远的值</summary>
        private static decimal? WorstO2(List<GasHistoryItem> samples)
        {
            decimal? worst = null;
            foreach (var sample in samples)
            {
                if (sample.O2 == null)
                {
                    continue;
                }
                if (worst == null || Math.Abs(sample.O2.Value - 20.9m) > Math.Abs(worst.Value - 20.9m))
                {
                    worst = sample.O2;
                }
            }
            return worst;
        }

        private static string FormatGas(decimal? value)
        {
            return value?.ToString("0.0") ?? string.Empty;
        }

        /// <summary>气体安全区间：氧气19.5~21%VOL、可燃性气体&lt;10%LEL、硫化氢&lt;7ppm、一氧化碳&lt;25ppm</summary>
        private static string EvaluateGasResult(GasRow row)
        {
            var hasValue = !string.IsNullOrEmpty(row.O2) || !string.IsNullOrEmpty(row.Ch4)
                || !string.IsNullOrEmpty(row.H2s) || !string.IsNullOrEmpty(row.Co);
            if (!hasValue)
            {
                return string.Empty;
            }
            var abnormal =
                (decimal.TryParse(row.O2, out var o2) && (o2 < 19.5m || o2 > 21m)) ||
                (decimal.TryParse(row.Ch4, out var ch4) && ch4 >= 10m) ||
                (decimal.TryParse(row.H2s, out var h2s) && h2s >= 7m) ||
                (decimal.TryParse(row.Co, out var co) && co >= 25m);
            return abnormal ? "不合格" : "合格";
        }

        #endregion

        #region Word 导出（docx 模板填充）

        private const string WNamespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

        /// <summary>
        /// 生成进出登记表 Word
        /// </summary>
        public Task<byte[]> BuildEntryExitWordAsync(EntryExitLedger ledger, CancellationToken cancellationToken)
        {
            return Task.Run(() => FillTemplate("Templates/entry-exit-ledger.docx", (doc, ns) =>
            {
                RemoveExtraTables(doc, ns);
                FillHeaderLabels(doc, ns, ledger.Header);
                FillEntryExitDataRows(GetFirstTable(doc, ns), ns, ledger.Rows);
            }), cancellationToken);
        }

        /// <summary>
        /// 生成气体监测记录表 Word
        /// </summary>
        public Task<byte[]> BuildGasWordAsync(GasLedger ledger, CancellationToken cancellationToken)
        {
            return Task.Run(() => FillTemplate("Templates/gas-monitor-ledger.docx", (doc, ns) =>
            {
                RemoveExtraTables(doc, ns);
                FillHeaderLabels(doc, ns, ledger.Header);
                FillGasDataRows(GetFirstTable(doc, ns), ns, ledger.Rows);
            }), cancellationToken);
        }

        /// <summary>
        /// 生成告警记录表 Word（无模板依赖，直接构建最小 docx 包）
        /// </summary>
        public Task<byte[]> BuildAlarmWordAsync(AlarmLedger ledger, CancellationToken cancellationToken)
        {
            return Task.Run(() => BuildMinimalDocx(writer => WriteAlarmWordBody(writer, ledger)), cancellationToken);
        }

        /// <summary>构建仅含 document.xml 的最小 docx 包</summary>
        private static byte[] BuildMinimalDocx(Action<XmlWriter> writeBody)
        {
            var contentTypes = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">" +
                "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>" +
                "<Default Extension=\"xml\" ContentType=\"application/xml\"/>" +
                "<Override PartName=\"/word/document.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml\"/>" +
                "</Types>";
            var rels = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"word/document.xml\"/>" +
                "</Relationships>";

            using var output = new MemoryStream();
            using (var archive = new ZipArchive(output, ZipArchiveMode.Create, true))
            {
                var typesEntry = archive.CreateEntry("[Content_Types].xml", CompressionLevel.Optimal);
                using (var writer = new StreamWriter(typesEntry.Open()))
                {
                    writer.Write(contentTypes);
                }
                var relsEntry = archive.CreateEntry("_rels/.rels", CompressionLevel.Optimal);
                using (var writer = new StreamWriter(relsEntry.Open()))
                {
                    writer.Write(rels);
                }
                var docEntry = archive.CreateEntry("word/document.xml", CompressionLevel.Optimal);
                using (var writer = XmlWriter.Create(docEntry.Open(), new XmlWriterSettings
                {
                    Indent = true,
                    Encoding = Encoding.UTF8
                }))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("w", "document", WNamespace);
                    writer.WriteStartElement("w", "body", WNamespace);
                    writeBody(writer);
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            }
            return output.ToArray();
        }

        /// <summary>写入告警记录表正文：标题、表头信息、数据表格、落款</summary>
        private static void WriteAlarmWordBody(XmlWriter writer, AlarmLedger ledger)
        {
            var header = ledger.Header;
            WriteWordParagraph(writer, "告警记录表", "36", bold: true);
            WriteWordParagraph(writer, $"编号：{header.ProjectNo}", "21", align: "left");
            WriteWordParagraph(writer, $"统计日期：{ledger.StatDate}", "21", align: "left");
            WriteWordParagraph(writer, $"作业单位：{header.Unit}", "21", align: "left");
            WriteWordParagraph(writer, $"作业地点：{header.Location}", "21", align: "left");
            WriteWordParagraph(writer, $"工作负责人：{header.Manager}    专职监护人：{header.Guardian}", "21", align: "left");

            var widths = new[] { 500, 1000, 900, 1300, 1300, 1500, 1500, 1600 };
            writer.WriteStartElement("w", "tbl", WNamespace);
            WriteWordTableProps(writer, widths);

            writer.WriteStartElement("w", "tr", WNamespace);
            foreach (var title in new[] { "序号", "报警时间", "姓名", "单位/部门", "联系电话", "设备编号", "报警类型", "处理情况" })
            {
                WriteWordTableCell(writer, title, "21", bold: true);
            }
            writer.WriteEndElement();

            var index = 1;
            foreach (var row in ledger.Rows)
            {
                writer.WriteStartElement("w", "tr", WNamespace);
                WriteWordTableCell(writer, index.ToString(), "21");
                WriteWordTableCell(writer, row.Time, "21");
                WriteWordTableCell(writer, row.Name, "21");
                WriteWordTableCell(writer, row.Department, "21");
                WriteWordTableCell(writer, row.Phone, "21");
                WriteWordTableCell(writer, row.DeviceId, "21");
                WriteWordTableCell(writer, row.AlarmType, "21");
                WriteWordTableCell(writer, row.Remark, "21");
                writer.WriteEndElement();
                index++;
            }

            if (ledger.Rows.Count == 0)
            {
                writer.WriteStartElement("w", "tr", WNamespace);
                for (var i = 0; i < 8; i++)
                {
                    WriteWordTableCell(writer, i == 0 ? "1" : string.Empty, "21");
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            WriteWordParagraph(writer, $"记录人：{header.Guardian}    记录日期：{DateTime.Now:yyyy年M月d日}", "21", align: "left");
        }

        /// <summary>写入表格属性与列宽（dxa 单位，总和适配 A4 可打印宽度）</summary>
        private static void WriteWordTableProps(XmlWriter writer, int[] widths)
        {
            writer.WriteStartElement("w", "tblPr", WNamespace);
            writer.WriteStartElement("w", "tblBorders", WNamespace);
            foreach (var edge in new[] { "top", "left", "bottom", "right", "insideH", "insideV" })
            {
                writer.WriteStartElement("w", edge, WNamespace);
                writer.WriteAttributeString("w", "val", WNamespace, "single");
                writer.WriteAttributeString("w", "sz", WNamespace, "4");
                writer.WriteAttributeString("w", "color", WNamespace, "000000");
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteStartElement("w", "tblW", WNamespace);
            writer.WriteAttributeString("w", "w", WNamespace, "5000");
            writer.WriteAttributeString("w", "type", WNamespace, "pct");
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteStartElement("w", "tblGrid", WNamespace);
            foreach (var width in widths)
            {
                writer.WriteStartElement("w", "gridCol", WNamespace);
                writer.WriteAttributeString("w", "w", WNamespace, width.ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        private static void WriteWordTableCell(XmlWriter writer, string text, string sizeHalfPoint, bool bold = false)
        {
            writer.WriteStartElement("w", "tc", WNamespace);
            writer.WriteStartElement("w", "tcPr", WNamespace);
            writer.WriteStartElement("w", "vAlign", WNamespace);
            writer.WriteAttributeString("w", "val", WNamespace, "center");
            writer.WriteEndElement();
            writer.WriteEndElement();
            WriteWordParagraph(writer, text, sizeHalfPoint, bold, "center");
            writer.WriteEndElement();
        }

        private static void WriteWordParagraph(XmlWriter writer, string text, string sizeHalfPoint, bool bold = false, string align = "center")
        {
            writer.WriteStartElement("w", "p", WNamespace);
            writer.WriteStartElement("w", "pPr", WNamespace);
            writer.WriteStartElement("w", "jc", WNamespace);
            writer.WriteAttributeString("w", "val", WNamespace, align);
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteStartElement("w", "r", WNamespace);
            writer.WriteStartElement("w", "rPr", WNamespace);
            writer.WriteStartElement("w", "rFonts", WNamespace);
            writer.WriteAttributeString("w", "eastAsia", WNamespace, "宋体");
            writer.WriteAttributeString("w", "ascii", WNamespace, "Times New Roman");
            writer.WriteAttributeString("w", "hAnsi", WNamespace, "Times New Roman");
            writer.WriteEndElement();
            writer.WriteStartElement("w", "sz", WNamespace);
            writer.WriteAttributeString("w", "val", WNamespace, sizeHalfPoint);
            writer.WriteEndElement();
            writer.WriteStartElement("w", "szCs", WNamespace);
            writer.WriteAttributeString("w", "val", WNamespace, sizeHalfPoint);
            writer.WriteEndElement();
            if (bold)
            {
                writer.WriteStartElement("w", "b", WNamespace);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteStartElement("w", "t", WNamespace);
            if (text != text.TrimStart())
            {
                writer.WriteAttributeString("xml", "space", "http://www.w3.org/XML/1998/namespace", "preserve");
            }
            writer.WriteString(text ?? string.Empty);
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        /// <summary>
        /// 将表头标签（作业单位/工作负责人/专职监护人/工作内容/作业地点/项目管理员/检测位置/进出登记点位置）后追加对应值。
        /// 新模板无 {{}} 占位符，值直接写在标签所在单元格/段落文本之后。
        /// </summary>
        private static void FillHeaderLabels(XmlDocument doc, XmlNamespaceManager ns, LedgerHeader header)
        {
            var manager = header.Manager ?? string.Empty;
            var guardian = string.IsNullOrWhiteSpace(header.Guardian) ? manager : header.Guardian;
            var administrator = string.IsNullOrWhiteSpace(header.Administrator) ? manager : header.Administrator;

            var labelValues = new (string[] Labels, string Value)[]
            {
                (new[] { "作业单位：" }, header.Unit ?? string.Empty),
                (new[] { "工作负责人" }, manager),
                (new[] { "专职监护人：" }, guardian),
                (new[] { "工作内容：" }, header.WorkContent ?? string.Empty),
                (new[] { "作业地点：" }, header.Location ?? string.Empty),
                (new[] { "项目管理员：" }, administrator),
                (new[] { "检测位置：" }, header.Location ?? string.Empty),
                (new[] { "进出登记点位置：" }, header.Location ?? string.Empty),
            };

            foreach (XmlNode node in doc.SelectNodes("//w:tc|//w:p", ns)!)
            {
                var text = GetNodeText(node, ns);
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }
                foreach (var pair in labelValues)
                {
                    if (string.IsNullOrEmpty(pair.Value))
                    {
                        continue;
                    }
                    foreach (var label in pair.Labels)
                    {
                        if (text.TrimStart().StartsWith(label) && !text.Contains(pair.Value))
                        {
                            text = text.TrimEnd() + pair.Value;
                            SetNodeText(node, ns, text);
                            text = GetNodeText(node, ns);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 模板含两份相同表格（第二页副本），仅保留第一份，避免重复输出。
        /// </summary>
        private static void RemoveExtraTables(XmlDocument doc, XmlNamespaceManager ns)
        {
            var tables = doc.SelectNodes("//w:tbl", ns)!;
            for (var i = tables.Count - 1; i >= 1; i--)
            {
                tables[i]!.ParentNode?.RemoveChild(tables[i]!);
            }
        }

        private static XmlNode GetFirstTable(XmlDocument doc, XmlNamespaceManager ns)
        {
            var tables = doc.SelectNodes("//w:tbl", ns);
            if (tables == null || tables.Count == 0)
            {
                throw new InvalidOperationException("Word 模板中未找到表格。");
            }
            return tables[0]!;
        }

        /// <summary>
        /// 读取 docx 模板，填充 document.xml 后重新打包
        /// </summary>
        private static byte[] FillTemplate(string templateRelativePath, Action<XmlDocument, XmlNamespaceManager> fill)
        {
            var templatePath = Path.Combine(AppContext.BaseDirectory, templateRelativePath);
            if (!File.Exists(templatePath))
            {
                throw new InvalidOperationException($"未找到 Word 模板：{templatePath}");
            }

            using var archive = ZipFile.OpenRead(templatePath);
            var entry = archive.GetEntry("word/document.xml") ?? throw new InvalidOperationException("Word 模板缺少 document.xml。");

            var doc = new XmlDocument();
            using (var stream = entry.Open())
            {
                doc.Load(stream);
            }

            var ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("w", WNamespace);
            fill(doc, ns);

            using var output = new MemoryStream();
            using (var newArchive = new ZipArchive(output, ZipArchiveMode.Create, true))
            {
                foreach (var item in archive.Entries)
                {
                    var newEntry = newArchive.CreateEntry(item.FullName, CompressionLevel.Optimal);
                    using var target = newEntry.Open();
                    if (item.FullName == "word/document.xml")
                    {
                        doc.Save(target);
                    }
                    else
                    {
                        using var source = item.Open();
                        source.CopyTo(target);
                    }
                }
            }
            return output.ToArray();
        }

        /// <summary>按索引给数据行某单元格赋值（索引越界忽略）</summary>
        private static void SetCell(XmlNode row, XmlNamespaceManager ns, int index, string value)
        {
            var cells = row.SelectNodes("./w:tc", ns)!;
            if (index < cells.Count)
            {
                SetNodeText(cells[index]!, ns, value ?? string.Empty);
            }
        }

        private static XmlNode FindDataTemplateRow(XmlNode table, XmlNamespaceManager ns, Func<string, bool> isTemplateRow)
        {
            foreach (XmlNode row in table.SelectNodes("./w:tr", ns)!)
            {
                if (isTemplateRow(GetNodeText(row, ns)))
                {
                    return row;
                }
            }
            throw new InvalidOperationException("Word 模板中未找到数据行模板。");
        }

        private static void RemoveDataRowsFrom(XmlNode table, XmlNamespaceManager ns, XmlNode templateRow)
        {
            var rows = table.SelectNodes("./w:tr", ns)!;
            var toRemove = new List<XmlNode>();
            var found = false;
            foreach (XmlNode row in rows)
            {
                if (ReferenceEquals(row, templateRow))
                {
                    found = true;
                }
                if (found)
                {
                    toRemove.Add(row);
                }
            }
            foreach (var row in toRemove)
            {
                table.RemoveChild(row);
            }
        }

        private static void FillEntryExitDataRows(XmlNode table, XmlNamespaceManager ns, List<EntryExitRow> rows)
        {
            var templateRow = FindDataTemplateRow(table, ns, rowText => rowText.Contains("年"));
            RemoveDataRowsFrom(table, ns, templateRow);

            foreach (var item in rows)
            {
                var newRow = templateRow.CloneNode(true);
                SetCell(newRow, ns, 0, item.Date);
                SetCell(newRow, ns, 1, item.Name);
                SetCell(newRow, ns, 2, item.Department);
                SetCell(newRow, ns, 3, item.EnterTime);
                SetCell(newRow, ns, 4, item.ExitTime);
                SetCell(newRow, ns, 5, item.Tools);
                SetCell(newRow, ns, 6, item.Guardian);
                SetCell(newRow, ns, 7, item.Remark);
                table.AppendChild(newRow);
            }

            if (rows.Count == 0)
            {
                table.AppendChild(templateRow.CloneNode(true));
            }
        }

        private static void FillGasDataRows(XmlNode table, XmlNamespaceManager ns, List<GasRow> rows)
        {
            var templateRow = FindDataTemplateRow(table, ns, rowText => rowText.Contains("合格□"));
            RemoveDataRowsFrom(table, ns, templateRow);

            foreach (var item in rows)
            {
                var newRow = templateRow.CloneNode(true);
                SetCell(newRow, ns, 0, item.O2);
                SetCell(newRow, ns, 1, item.Ch4);
                SetCell(newRow, ns, 2, item.H2s);
                SetCell(newRow, ns, 3, item.Co);
                // 4~8 列为其他气体（二氧化碳/氮氧化物/二氧化硫/氨气/粉尘），系统不采集，保持空白
                SetCell(newRow, ns, 9, item.Result == "合格" ? "合格■不合格□" : item.Result == "不合格" ? "合格□不合格■" : "合格□不合格□");
                SetCell(newRow, ns, 10, item.DetectTime);
                SetCell(newRow, ns, 11, item.Recorder);
                table.AppendChild(newRow);
            }

            if (rows.Count == 0)
            {
                table.AppendChild(templateRow.CloneNode(true));
            }
        }

        /// <summary>获取节点内全部文本（拼接所有 w:t）</summary>
        private static string GetNodeText(XmlNode node, XmlNamespaceManager ns)
        {
            var builder = new StringBuilder();
            foreach (XmlNode text in node.SelectNodes(".//w:t", ns)!)
            {
                builder.Append(text.InnerText);
            }
            return builder.ToString();
        }

        /// <summary>
        /// 设置节点文本：保留第一个段落承载内容并删除多余段落/run（避免模板单元格多段落产生多余换行），无 w:t 时创建 run
        /// </summary>
        private static void SetNodeText(XmlNode node, XmlNamespaceManager ns, string text)
        {
            var doc = node.OwnerDocument!;

            // 定位承载段落：节点本身是段落时用自身，否则取第一个子段落并删除其余段落
            XmlNode paragraph;
            if (node.Name == "w:p")
            {
                paragraph = node;
            }
            else
            {
                var paragraphs = node.SelectNodes(".//w:p", ns)!;
                paragraph = paragraphs.Count > 0 ? paragraphs[0]! : node.AppendChild(doc.CreateElement("w", "p", WNamespace))!;
                for (var i = paragraphs.Count - 1; i >= 1; i--)
                {
                    paragraphs[i]!.ParentNode?.RemoveChild(paragraphs[i]);
                }
            }

            // 段落内保留首个含 w:t 的 run（无则用第一个 run），删除其余 run
            var runs = paragraph.SelectNodes("./w:r", ns)!;
            XmlNode? keepRun = null;
            foreach (XmlNode run in runs)
            {
                if (keepRun == null && run.SelectSingleNode("./w:t", ns) != null)
                {
                    keepRun = run;
                }
            }
            keepRun ??= runs.Count > 0 ? runs[0] : null;
            for (var i = runs.Count - 1; i >= 0; i--)
            {
                if (!ReferenceEquals(runs[i], keepRun))
                {
                    paragraph.RemoveChild(runs[i]!);
                }
            }
            if (keepRun == null)
            {
                keepRun = paragraph.AppendChild(doc.CreateElement("w", "r", WNamespace))!;
            }

            // run 内保留首个 w:t 并赋值，删除其余 w:t
            var textNodes = keepRun.SelectNodes("./w:t", ns)!;
            XmlNode textNode;
            if (textNodes.Count > 0)
            {
                textNode = textNodes[0]!;
                for (var i = textNodes.Count - 1; i >= 1; i--)
                {
                    keepRun.RemoveChild(textNodes[i]!);
                }
            }
            else
            {
                textNode = keepRun.AppendChild(doc.CreateElement("w", "t", WNamespace))!;
            }
            textNode.InnerText = text ?? string.Empty;
        }

        #endregion
    }
}
