using System.Collections.Concurrent;

namespace Venus.WebApi.Services
{
    public class WatchConnectionRegistry
    {
        private readonly ConcurrentDictionary<string, WatchConnectionState> _connections = new();

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

        public bool Online => DisconnectedAt == null;
    }
}
