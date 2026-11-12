using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Venus.Tools.Extension;
using Venus.WebApi.Services;

namespace Venus.WebApi.Common
{
    /// <summary>
    /// 开放接口密钥校验：请求需携带 Header X-Api-Key（或 URL 参数 apiKey），
    /// 与配置 OpenApi:ApiKey 一致方可访问。用于第三方（场站内网系统）对接，
    /// 与登录 JWT 体系无关；使用处需同时标记 [AllowAnonymous]。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class OpenApiKeyAttribute : Attribute, IAuthorizationFilter
    {
        /// <summary>
        /// 校验 X-Api-Key（或 URL 参数 apiKey）与配置 OpenApi:ApiKey 是否一致
        /// </summary>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var options = context.HttpContext.RequestServices.GetRequiredService<IOptions<OpenApiOptions>>().Value;
            var expected = options.ApiKey?.Trim();
            if (string.IsNullOrWhiteSpace(expected))
            {
                // fail closed：未配置密钥时全部拒绝
                context.Result = new ContentResult
                {
                    Content = new { message = "服务端未配置 OpenApi:ApiKey，开放接口不可用", status = false, code = 500 }.Serialize(),
                    ContentType = "application/json",
                    StatusCode = 500
                };
                return;
            }

            var provided = context.HttpContext.Request.Headers["X-Api-Key"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(provided))
            {
                provided = context.HttpContext.Request.Query["apiKey"].FirstOrDefault();
            }

            if (string.IsNullOrWhiteSpace(provided) || provided.Trim() != expected)
            {
                context.Result = new ContentResult
                {
                    Content = new { message = "ApiKey 无效", status = false, code = 401 }.Serialize(),
                    ContentType = "application/json",
                    StatusCode = 401
                };
            }
        }
    }
}
