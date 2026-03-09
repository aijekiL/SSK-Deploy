using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;
using Venus.Tools.Response;

namespace Venus.WebApi.Middleware
{
    public static class ExceptionMiddlewareExtensions
    {
        public static void UseGlobalExceptionHandler(this WebApplication app)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.ContentType = "application/json";
                    var errorFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (errorFeature != null)
                    {
                        var msg = JsonConvert.SerializeObject(WebResponseContent.Instance.Error(errorFeature.Error.Message), new JsonSerializerSettings
                        {
                            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                        });
                        await context.Response.WriteAsync(msg);
                    }
                });
            });
        }
    }
}
