using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using Venus.Tools;
using Venus.Tools.Extension;

namespace Venus.Authentication
{
    public class ApiAuthorizeFilter : IAuthorizationFilter
    {

        public ApiAuthorizeFilter()
        {

        }
        /// <summary>
        /// 只判断token是否正确，不判断权限
        /// 如果需要判断权限的在Action上加上ApiActionPermission属性标识权限类别，ActionPermissionFilter作权限处理
        ///(string,string,string)1、请求参数,2、返回消息，3,异常消息,4状态
        /// </summary>
        /// <param name="context"></param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // is Microsoft.AspNetCore.Authentication.AllowAnonymousAttribute
            //if (context.Filters.Any(item => item is IAllowAnonymousFilter))
            //限定一个帐号不能在多处登陆   UserContext.Current.Token != ((ClaimsIdentity)context.HttpContext.User.Identity)?.BootstrapContext?.ToString()

            // &&UserContext.Current.UserName!="admin666"为演示环境，实际使用时去掉此条件
            //if (!context.HttpContext.User.Identity.IsAuthenticated
            //    || (
            //    UserContext.Current.Token != ((ClaimsIdentity)context.HttpContext.User.Identity)
            //    ?.BootstrapContext?.ToString()
            //    && UserContext.Current.UserName != "admin666" 
            //    ))
            //{
            //    Console.Write($"IsAuthenticated:{context.HttpContext.User.Identity.IsAuthenticated}," +
            //        $"userToken{UserContext.Current.Token}" +
            //        $"BootstrapContext:{((ClaimsIdentity)context.HttpContext.User.Identity)?.BootstrapContext?.ToString()}");
            //    context.Unauthorized("登陆已过期");
            //    return;
            //}
            if (context.ActionDescriptor.EndpointMetadata.Any(item => item is IAllowAnonymous))
                return;

            var fixedoken = context.HttpContext.Request.Headers[AppSetting.TokenHeaderName].ToString();
            if (string.IsNullOrWhiteSpace(fixedoken)) return;
            var expClaim = context.HttpContext.User.Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Exp)
                .Select(x => x.Value)
                .FirstOrDefault();
            if (string.IsNullOrWhiteSpace(expClaim)) return;
            DateTime expDate = expClaim.GetTimeSpmpToDate();
            if (expDate < DateTime.Now)
            {
                context.Result = new ContentResult()
                {
                    Content = new { message = "登陆已过期", status = false, code = (int)401 }.Serialize(),
                    ContentType = "application/json",
                    StatusCode = (int)HttpStatusCode.Unauthorized
                };
                return;
            }
            //动态标识刷新token(2021.05.01)
            if ((expDate - DateTime.Now).TotalMinutes < AppSetting.ExpMinutes / 3 && context.HttpContext.Request.Path != replaceTokenPath)
            {
                //动态标识刷新token(2021.05.01)
                context.HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "venus_exp";
                context.HttpContext.Response.Headers["venus_exp"] = "1";
            }
        }
        private static readonly string replaceTokenPath = "/api/User/replaceToken";
    }
}
