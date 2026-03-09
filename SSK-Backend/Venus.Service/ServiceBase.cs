using Venus.Core;
using Venus.Tools;

namespace Venus.Service
{
    public class ServiceBase : ITransientDependency
    {
        public readonly ICurrentUserContextService _currentUserContextService;
        public ServiceBase(ICurrentUserContextService currentUserContextService)
        {
            _currentUserContextService = currentUserContextService;
        }
    }
}
