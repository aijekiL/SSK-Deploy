using System.Net;
using System.Net.Sockets;
using Venus.Entitys;
using Venus.Entitys.SecurityMonitorService;

namespace Venus.WebApi.Services
{
    public class WatchTcpHostedService : BackgroundService
    {
        private static readonly byte[] Header = { 0xBD, 0xBD, 0xBD, 0xBD };
        private static readonly byte[] HeartbeatReply = { 0xBD, 0xBD, 0xBD, 0xBD, 0xF3, 0x01 };
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WatchTcpHostedService> _logger;
        private readonly WatchConnectionRegistry _registry;
        private TcpListener? _listener;

        public WatchTcpHostedService(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<WatchTcpHostedService> logger,
            WatchConnectionRegistry registry)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _logger = logger;
            _registry = registry;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var port = GetListenPort();
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            _logger.LogInformation("[WatchTcp] listening on 0.0.0.0:{Port}", port);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync(stoppingToken);
                    _ = Task.Run(() => HandleClientAsync(client, stoppingToken), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[WatchTcp] accept failed");
                }
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _listener?.Stop();
            return base.StopAsync(cancellationToken);
        }

        private int GetListenPort()
        {
            var env = Environment.GetEnvironmentVariable("WATCH_TCP_PORT");
            if (int.TryParse(env, out var envPort) && envPort > 0)
            {
                return envPort;
            }

            var configured = _configuration.GetValue<int?>("WatchTcp:Port");
            return configured.GetValueOrDefault(5011);
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken stoppingToken)
        {
            var connectionId = Guid.NewGuid().ToString("N");
            var remoteEndPoint = client.Client.RemoteEndPoint?.ToString() ?? string.Empty;
            _registry.MarkConnected(connectionId, remoteEndPoint);
            _logger.LogInformation("[WatchTcp] connected connectionId={ConnectionId} remote={Remote}", connectionId, remoteEndPoint);

            var state = new WatchClientState { ConnectionId = connectionId };
            var buffer = new byte[4096];
            var pending = new List<byte>();

            try
            {
                using (client)
                    await using (var stream = client.GetStream())
                    {
                        _registry.AttachStream(connectionId, stream);
                        while (!stoppingToken.IsCancellationRequested && client.Connected)
                    {
                        var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), stoppingToken);
                        if (read <= 0)
                        {
                            break;
                        }

                        pending.AddRange(buffer.Take(read));
                        while (TryExtractFrame(pending, out var frame))
                        {
                            await HandleFrameAsync(frame, stream, state, stoppingToken);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[WatchTcp] client failed connectionId={ConnectionId} remote={Remote}", connectionId, remoteEndPoint);
            }
            finally
            {
                _registry.MarkDisconnected(connectionId);
                _logger.LogInformation("[WatchTcp] disconnected connectionId={ConnectionId} remote={Remote} imei={Imei}", connectionId, remoteEndPoint, state.WatchImei);
            }
        }

        private async Task HandleFrameAsync(byte[] frame, NetworkStream stream, WatchClientState state, CancellationToken stoppingToken)
        {
            if (frame.Length < 6)
            {
                return;
            }

            var messageId = frame[4];
            switch (messageId)
            {
                case 0xF0:
                    var imei = ParseImei(frame);
                    state.WatchImei = imei;
                    _registry.MarkLogin(state.ConnectionId, imei);
                    _logger.LogInformation("[WatchTcp] received F0 imei={Imei}", imei);
                    var loginReply = BuildLoginReply();
                    await stream.WriteAsync(loginReply, stoppingToken);
                    _logger.LogInformation("[WatchTcp] login ok imei={Imei}", imei);
                    await SendDefaultHealthPeriodAsync(imei, stoppingToken);
                    await FlushPendingCommandsAsync(imei, stoppingToken);
                    break;

                case 0xC0:
                    _registry.MarkDownlinkAck(state.WatchImei ?? string.Empty, ToHex(frame));
                    _logger.LogInformation("[WatchTcp] downlink ack imei={Imei} hex={Hex}", state.WatchImei, ToHex(frame));
                    break;

                case 0xF9:
                    _registry.MarkHeartbeat(state.ConnectionId);
                    await stream.WriteAsync(HeartbeatReply, stoppingToken);
                    _logger.LogInformation("[WatchTcp] F9 heartbeat imei={Imei} remote={Remote}", state.WatchImei, state.ConnectionId);
                    break;

                case 0x32:
                    var health = ParseHealth(frame, state.WatchImei);
                    if (health == null)
                    {
                        _logger.LogWarning("[WatchTcp] invalid 0x32 frame imei={Imei} hex={Hex}", state.WatchImei, ToHex(frame));
                        return;
                    }
                    await SaveHealthAsync(health, stoppingToken);
                    _registry.MarkHealth(state.ConnectionId);
                    _logger.LogInformation(
                        "[WatchTcp] health imei={Imei} heartRate={HeartRate} spo2={Spo2} bp={Systolic}/{Diastolic}",
                        health.WatchImei,
                        health.HeartRate,
                        health.Spo2,
                        health.Systolic,
                        health.Diastolic);
                    break;
            }
        }

        private static bool TryExtractFrame(List<byte> pending, out byte[] frame)
        {
            frame = Array.Empty<byte>();
            var headerIndex = IndexOfHeader(pending, 0);
            if (headerIndex < 0)
            {
                pending.Clear();
                return false;
            }

            if (headerIndex > 0)
            {
                pending.RemoveRange(0, headerIndex);
            }

            if (pending.Count < 6)
            {
                return false;
            }

            var messageId = pending[4];
            int frameLength;
            switch (messageId)
            {
                case 0xF0:
                    frameLength = 16;
                    break;
                case 0xF9:
                    frameLength = 21;
                    break;
                case 0xC0:
                    // 下行反馈：BDBDBDBD C0 <len:u8> <msgid×len> <checksum>（文档 4.6.1）
                    if (pending.Count < 6)
                    {
                        return false;
                    }
                    frameLength = 7 + pending[5];
                    break;
                case 0x32:
                    if (pending.Count < 12)
                    {
                        return false;
                    }
                    var contentLength = ReadUInt16LE(pending, 10);
                    frameLength = 13 + contentLength;
                    break;
                default:
                    var nextHeaderIndex = IndexOfHeader(pending, 4);
                    if (nextHeaderIndex < 0)
                    {
                        return false;
                    }
                    pending.RemoveRange(0, nextHeaderIndex);
                    return TryExtractFrame(pending, out frame);
            }

            if (pending.Count < frameLength)
            {
                return false;
            }

            frame = pending.Take(frameLength).ToArray();
            pending.RemoveRange(0, frameLength);
            return true;
        }

        private static int IndexOfHeader(List<byte> bytes, int start)
        {
            for (var i = Math.Max(0, start); i <= bytes.Count - Header.Length; i++)
            {
                if (bytes[i] == Header[0] &&
                    bytes[i + 1] == Header[1] &&
                    bytes[i + 2] == Header[2] &&
                    bytes[i + 3] == Header[3])
                {
                    return i;
                }
            }
            return -1;
        }

        private static string ParseImei(byte[] frame)
        {
            if (frame.Length < 13)
            {
                return string.Empty;
            }
            return BitConverter.ToUInt64(frame, 5).ToString();
        }

        private static SM_WatchHealthReport? ParseHealth(byte[] frame, string? watchImei)
        {
            if (string.IsNullOrWhiteSpace(watchImei) || frame.Length < 13)
            {
                return null;
            }

            var timestamp = BitConverter.ToUInt32(frame, 6);
            var contentLength = BitConverter.ToUInt16(frame, 10);
            if (frame.Length < 13 + contentLength)
            {
                return null;
            }

            int? heartRate = null;
            int? spo2 = null;
            int? systolic = null;
            int? diastolic = null;
            var offset = 12;
            var end = offset + contentLength;
            while (offset < end)
            {
                var id = frame[offset++];
                var type = id >> 3;
                var length = id & 0x07;
                if (length <= 0 || offset + length > end)
                {
                    break;
                }

                var value = ReadUnsignedLittleEndian(frame, offset, length);
                offset += length;

                switch (type)
                {
                    case 0x02:
                        heartRate = value;
                        break;
                    case 0x06:
                        diastolic = value;
                        break;
                    case 0x07:
                        systolic = value;
                        break;
                    case 0x08:
                        spo2 = value;
                        break;
                }
            }

            return new SM_WatchHealthReport
            {
                ObjectId = Guid.NewGuid(),
                WatchImei = watchImei.Trim(),
                HeartRate = heartRate,
                Spo2 = spo2,
                Systolic = systolic,
                Diastolic = diastolic,
                ReportTime = DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime,
                RawHex = ToHex(frame),
                CreateDate = DateTime.Now,
                CreateID = Guid.Empty
            };
        }

        private async Task SaveHealthAsync(SM_WatchHealthReport report, CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<VenusDbContext>();
            var staff = dbContext.Set<SM_StaffInfo>()
                .Where(x => x.WatchImei == report.WatchImei)
                .OrderByDescending(x => x.Status == 2)
                .ThenByDescending(x => x.CreateDate)
                .FirstOrDefault();

            if (staff != null)
            {
                report.StaffId = staff.ObjectId;
                report.ProjectId = staff.ProjectId;
            }

            dbContext.Set<SM_WatchHealthReport>().Add(report);
            await dbContext.SaveChangesAsync(stoppingToken);
        }

        /// <summary>
        /// 登录成功后自动下发默认健康采样上报频率（0x1C）。
        /// 频率取配置 WatchTcp:HealthReportMinutes，默认 2 分钟；配置为 0 或负数时不下发。
        /// 设备最高支持 2 分钟（协议 5.1.4），因此配置小于 2 时按 2 下发。
        /// </summary>
        private async Task SendDefaultHealthPeriodAsync(string imei, CancellationToken stoppingToken)
        {
            var minutes = _configuration.GetValue<int?>("WatchTcp:HealthReportMinutes") ?? 2;
            if (minutes <= 0)
            {
                return;
            }

            if (minutes < 2)
            {
                minutes = 2;
            }

            var frame = WatchDownlinkCommands.SetHealthSamplePeriod((ushort)minutes);
            try
            {
                var sent = await _registry.TrySendAsync(imei, frame, stoppingToken);
                _logger.LogInformation(
                    "[WatchTcp] default health period imei={Imei} minutes={Minutes} sent={Sent} hex={Hex}",
                    imei, minutes, sent, ToHex(frame));

                // 追加下发 0xCE-0B 立即上报健康数据（文档 5.1.6 Type=0B）：设备立刻采样并上报，免去等待采样周期
                var reportNowFrame = WatchDownlinkCommands.ReportHealthNow();
                var reportNowSent = await _registry.TrySendAsync(imei, reportNowFrame, stoppingToken);
                _logger.LogInformation(
                    "[WatchTcp] report health now imei={Imei} sent={Sent} hex={Hex}",
                    imei, reportNowSent, ToHex(reportNowFrame));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[WatchTcp] default health period send failed imei={Imei}", imei);
            }
        }

        private async Task FlushPendingCommandsAsync(string? imei, CancellationToken stoppingToken)
        {
            if (string.IsNullOrWhiteSpace(imei))
            {
                return;
            }

            var pending = _registry.TakePendingCommands(imei);
            foreach (var frame in pending)
            {
                try
                {
                    var sent = await _registry.TrySendAsync(imei, frame, stoppingToken);
                    _logger.LogInformation("[WatchTcp] flush pending command imei={Imei} sent={Sent} hex={Hex}", imei, sent, ToHex(frame));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[WatchTcp] flush pending command failed imei={Imei}", imei);
                }
            }
        }

        private static byte[] BuildLoginReply()
        {
            var timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(timestamp));
            bytes.Add(0xF1);
            bytes.AddRange(Header);
            bytes.Add(CalculateChecksum(bytes));
            return bytes.ToArray();
        }

        private static byte CalculateChecksum(IEnumerable<byte> bytes)
        {
            var sum = 0;
            foreach (var b in bytes)
            {
                sum = (sum + b) % 0x100;
            }
            return (byte)(0xff - sum);
        }

        private static ushort ReadUInt16LE(List<byte> bytes, int offset)
        {
            return (ushort)(bytes[offset] | (bytes[offset + 1] << 8));
        }

        private static int ReadUnsignedLittleEndian(byte[] bytes, int offset, int length)
        {
            var value = 0;
            for (var i = 0; i < length; i++)
            {
                value |= bytes[offset + i] << (8 * i);
            }
            return value;
        }

        private static string ToHex(byte[] bytes)
        {
            return Convert.ToHexString(bytes);
        }

        private class WatchClientState
        {
            public string ConnectionId { get; set; } = string.Empty;

            public string? WatchImei { get; set; }
        }
    }
}
