using Microsoft.AspNetCore.Mvc.Filters;
using Venus.Authentication.User;
using Venus.Core;
using Venus.Tools.Extension;

namespace Venus.Authentication
{
    public class UserContextFilter : IActionFilter
    {
        private readonly ICurrentUserContextService _currentUserContextService;

        public UserContextFilter(ICurrentUserContextService currentUserContextService)
        {
            _currentUserContextService = currentUserContextService;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            //var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userid = UserContext.Current.UserId;
            userid = userid == Guid.Empty ? "d63c7514-855c-4369-a2a9-416d0e1fc64a".ToGuid() : userid;
            _currentUserContextService.UserId = userid;

            // 可以添加日志来确认赋值是否成功
            System.Diagnostics.Debug.WriteLine($"Set UserId: {_currentUserContextService.UserId}");
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // 可以在这里做一些清理工作
            // 可以添加日志来确认后续是否还能获取到正确的值
            System.Diagnostics.Debug.WriteLine($"Get UserId after action: {_currentUserContextService.UserId}");
        }
    }
}