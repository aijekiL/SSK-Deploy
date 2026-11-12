using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Venus.WebApi.Services
{
    public class WatchConnectionRegistry
    {
        private const int MaxPendingPerImei = 20;
        private readonly ConcurrentDictionary<string, WatchConnectionState> _connections = new();
        private readonly ConcurrentDictionary<string, ConcurrentQueue<byte[]>> _pendingCommands =
            new(StringComparer.OrdinalIgnoreCase);

        public void MarkConnected(string connectionId, string remoteEndPoint)
        {
            _connections[connectionId] = new WatchConnectionState
            {
                ConnectionId = connectionId,
                RemoteEndPoint = remoteEndPoint,
                ConnectedAt = DateTime.Now
            };
        }

        public void MarkLogin(string connectionId, string watchImei)
        {
            if (_connections.TryGetValue(connectionId, out var state))
            {
                state.WatchImei = watchImei;
                state.LastLoginAt = DateTime.Now;
            }
        }

        public void MarkHeartbeat(string connectionId)
        {
            if (_connections.TryGetValue(connectionId, out var state))
            {
                state.LastHeartbeatAt = DateTime.Now;
            }
        }

        public void MarkHealth(string connectionId)
        {
            if (_connections.TryGetValue(connectionId, out var state))
            {
                state.LastHealthAt = DateTime.Now;
            }
        }

        public void MarkDisconnected(string connectionId)
        {
            if (_connections.TryGetValue(connectionId, out var state))
            {
                state.DisconnectedAt = DateTime.Now;
                state.Stream = null;
            }
        }

        public void AttachStream(string connectionId, NetworkStream stream)
        {
            if (_connections.TryGetValue(connectionId, out var state))
            {
                state.Stream = stream;
            }
        }

        public WatchConnectionState? FindOnline(string watchImei)
        {
            if (string.IsNullOrWhiteSpace(watchImei))
            {
                return null;
            }

            var normalized = watchImei.Trim();
            return _connections.Values
                .Where(x => x.Online && x.Stream != null && string.Equals(x.WatchImei, normalized, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.LastLoginAt ?? x.ConnectedAt)
                .FirstOrDefault();
        }

        public async Task<bool> TrySendAsync(string watchImei, byte[] frame, CancellationToken cancellationToken = default)
        {
            var state = FindOnline(watchImei);
            var stream = state?.Stream;
            if (state == null || stream == null)
            {
                return false;
            }

            await state.WriteLock.WaitAsync(cancellationToken);
            try
            {
                // 协议要求：下行指令需 20 毫秒内连续下行两次，保障成功（W300PGW V2.0 文档 2.3）
                await stream.WriteAsync(frame, cancellationToken);
                await stream.WriteAsync(frame, cancellationToken);
                state.LastCommandAt = DateTime.Now;
                state.LastCommandHex = Convert.ToHexString(frame);
                return true;
            }
            finally
            {
                state.WriteLock.Release();
            }
        }

        /// <summary>
        /// 下发下行指令：在线立即发送，离线写入缓存队列等待登录后补发
        /// </summary>
        public async Task<(bool Sent, bool Queued)> SendOrQueueAsync(string watchImei, byte[] frame, CancellationToken cancellationToken = default)
        {
            var normalized = (watchImei ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(normalized))
            {
                return (false, false);
            }

            if (await TrySendAsync(normalized, frame, cancellationToken))
            {
                return (true, false);
            }

            var queue = _pendingCommands.GetOrAdd(normalized, _ => new ConcurrentQueue<byte[]>());
            while (queue.Count >= MaxPendingPerImei && queue.TryPeek(out _))
            {
                queue.TryDequeue(out _);
            }
            queue.Enqueue(frame);
            return (false, true);
        }

        public List<byte[]> TakePendingCommands(string watchImei)
        {
            var normalized = (watchImei ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(normalized) || !_pendingCommands.TryRemove(normalized, out var queue))
            {
                return new List<byte[]>();
            }

            var frames = new List<byte[]>();
            while (queue.TryDequeue(out var frame))
            {
                frames.Add(frame);
            }
            return frames;
        }

        public void MarkDownlinkAck(string watchImei, string ackHex)
        {
            if (string.IsNullOrWhiteSpace(watchImei))
            {
                return;
            }

            var normalized = watchImei.Trim();
            var state = FindOnline(normalized)
                ?? _connections.Values.FirstOrDefault(x => string.Equals(x.WatchImei, normalized, StringComparison.OrdinalIgnoreCase));
            if (state != null)
            {
                state.LastAckAt = DateTime.Now;
                state.LastAckHex = ackHex;
            }
        }

        public IReadOnlyCollection<WatchConnectionState> GetAll()
        {
            return _connections.Values
                .OrderByDescending(x => x.ConnectedAt)
                .ToList();
        }
    }

    public class WatchConnectionState
    {
        public string ConnectionId { get; set; } = string.Empty;

        public string RemoteEndPoint { get; set; } = string.Empty;

        public string? WatchImei { get; set; }

        public DateTime ConnectedAt { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public DateTime? LastHeartbeatAt { get; set; }

        public DateTime? LastHealthAt { get; set; }

        public DateTime? DisconnectedAt { get; set; }

        public DateTime? LastCommandAt { get; set; }

        public string? LastCommandHex { get; set; }

        public DateTime? LastAckAt { get; set; }

        public string? LastAckHex { get; set; }

        public bool Online => DisconnectedAt == null;

        internal NetworkStream? Stream { get; set; }

        internal SemaphoreSlim WriteLock { get; } = new(1, 1);
    }

    /// <summary>
    /// 手表下行指令帧构造器：帧头(BDBDBDBD) + MessageId + Payload + 校验和
    /// 校验和 = 0xFF - ((帧头+MessageId+Payload 逐字节累加) % 256)
    /// </summary>
    public static class WatchDownlinkCommands
    {
        private static readonly byte[] Header = { 0xBD, 0xBD, 0xBD, 0xBD };

        public const byte MsgSetUploadPeriod = 0x17;
        public const byte MsgHeartRateThreshold = 0x19;
        public const byte MsgHealthSamplePeriod = 0x1C;
        public const byte MsgCommand = 0xCE;

        public static byte[] Build(byte messageId, byte[] payload)
        {
            var bytes = new List<byte>();
            bytes.AddRange(Header);
            bytes.Add(messageId);
            if (payload.Length > 0)
            {
                bytes.AddRange(payload);
            }
            bytes.Add(CalculateChecksum(bytes));
            return bytes.ToArray();
        }

        /// <summary>
        /// 0x1C 健康采样上报频率设置（W300PGW-CAT1 TCP协议V2.0 文档 5.1.4，注意与 W200P 的 0x1A 不同）：
        /// payload = 4 个时间段，每段 7 字节：
        ///   enable(u8，1=启用) + Interval(U16 小端，分钟) + time_start_h + time_start_m + time_end_h + time_end_m
        /// 未启用时间段 7 字节全部填 0；设备仅在启用时间段内上报，时间段外不上报。
        /// 此处使用时间段1 覆盖全天 00:00-23:59。
        /// </summary>
        public static byte[] SetHealthSamplePeriod(ushort minutes)
        {
            var payload = new byte[28];
            payload[0] = 0x01;                          // 时间段1 enable=启用
            payload[1] = (byte)(minutes & 0xFF);        // Interval 低字节（小端）
            payload[2] = (byte)((minutes >> 8) & 0xFF); // Interval 高字节（小端）
            payload[3] = 0x00;                          // start 00:00
            payload[4] = 0x00;
            payload[5] = 23;                            // end 23:59
            payload[6] = 59;
            // 时间段2~4 保持全 0（未启用）
            return Build(MsgHealthSamplePeriod, payload);
        }

        /// <summary>
        /// 0xCE-0B 立即上报健康数据（W300PGW V2.0 文档 5.1.6 Type=0B）：
        /// payload = Type(0x0B) + switch(0x01 单次下发单次上报) + 0x0000 固定值。
        /// 设备收到后立刻开始健康采样并上报，用于免等待采样周期、立即获取最新数据。
        /// </summary>
        public static byte[] ReportHealthNow()
        {
            return Build(MsgCommand, new byte[] { 0x0B, 0x01, 0x00, 0x00 });
        }

        /// <summary>
        /// 0x19 心率报警门限：payload = u16 上限 + u16 下限（小端）
        /// </summary>
        public static byte[] SetHeartRateThreshold(ushort upper, ushort lower)
        {
            var payload = new byte[]
            {
                (byte)(upper & 0xFF), (byte)((upper >> 8) & 0xFF),
                (byte)(lower & 0xFF), (byte)((lower >> 8) & 0xFF)
            };
            return Build(MsgHeartRateThreshold, payload);
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
    }
}
