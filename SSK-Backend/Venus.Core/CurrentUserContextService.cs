using Venus.Tools;

namespace Venus.Core
{
    // 自定义上下文服务接口
    public interface ICurrentUserContextService : IScopeDependency
    {
        Guid UserId { get; set; }
    }

    // 自定义上下文服务实现
    public class CurrentUserContextService : ICurrentUserContextService, IScopeDependency
    {
        private static readonly AsyncLocal<Guid> _asyncLocalUserId = new AsyncLocal<Guid>();

        public Guid UserId
        {
            get => _asyncLocalUserId.Value;
            set => _asyncLocalUserId.Value = value;
        }
    }
}
