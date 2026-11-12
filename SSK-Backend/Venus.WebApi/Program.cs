using IGeekFan.AspNetCore.Knife4jUI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using NETCore.MailKit.Extensions;
using NETCore.MailKit.Infrastructure.Internal;
using System.Reflection;
using System.Text;
using Venus.Authentication;
using Venus.Entitys;
using Venus.Service;
using Venus.Tools;
using Venus.Tools.Extension;
using Venus.Tools.Response;
using Venus.WebApi.Common;
using Venus.WebApi.Middleware;
using Venus.WebApi.Services;

namespace Venus.WebApi
{
    public class Program
    {
        // Add-Migration InitialCreate
        // Update-Database
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //初始化配置文件
            AppSetting.Init(builder.Configuration);

            var services = builder.Services;
            services.AddHttpContextAccessor();
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(ApiAuthorizeFilter));
                //忽略实体模型中的默认必填校验
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                //options.Filters.Add(typeof(ActionExecuteFilter));
                //  options.SuppressAsyncSuffixInActionNames = false;
            });

            services.AddDbContext<VenusDbContext>();
            services.AddScoped<UserContextFilter>();
            //builder.Services.AddTransient<ICurrentUserContextService, CurrentUserContextService>();

            //自动注入服务
            services.AddServices();


            #region 注册Kit邮件服务
            //发邮件
            services.AddMailKit(optionBuilder =>
            {
                optionBuilder.UseMailKit(new MailKitOptions()
                {
                    //get options from sercets.json
                    Server = builder.Configuration.GetValue<string>("EmailOptions:Host"),
                    Port = builder.Configuration.GetValue<int>("EmailOptions:Port"),
                    SenderName = builder.Configuration.GetValue<string>("EmailOptions:SenderName"),
                    SenderEmail = builder.Configuration.GetValue<string>("EmailOptions:FromAddress"),

                    // can be optional with no authentication 
                    Account = builder.Configuration.GetValue<string>("EmailOptions:UserName"),
                    Password = builder.Configuration.GetValue<string>("EmailOptions:Password"),
                    // enable ssl or tls
                    Security = true
                });
            });

            #endregion


            #region 配置Hangfire服务
            //services.AddHangfire(configuration => configuration
            //    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170) // 设置数据兼容性级别为 1.7 版本                
            //    .UseSimpleAssemblyNameTypeSerializer() // 使用简单的程序集名称类型序列化器
            //    .UseRecommendedSerializerSettings() // 使用推荐的序列化器设置
            //    .UseSqlServerStorage(AppSetting.DbConnectionString, new SqlServerStorageOptions
            //    {
            //        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5), // 批处理作业的最大超时时间为 5 分钟
            //        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5), // 作业的可见性超时时间为 5 分钟
            //        QueuePollInterval = TimeSpan.FromSeconds(5), // 检查作业队列的间隔时间为 5 秒
            //        UseRecommendedIsolationLevel = true, // 使用推荐的事务隔离级别
            //        DisableGlobalLocks = true // 禁用全局锁定机制                   
            //    }));

            //services.AddHangfireServer(); // 添加 Hangfire 服务器
            #endregion


            builder.Services.AddSingleton(new MongoClient(builder.Configuration["Connection:MongoDbConnectionString"]));
            // 注册文件存储服务
            builder.Services.AddScoped(sp =>
            {
                var mongoClient = sp.GetRequiredService<MongoClient>();
                return new FileStorageService(mongoClient, "Attachment");
            });



            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.CustomSchemaIds(type =>
                {
                    return type.FullName;
                });
                // 获取当前程序集的 XML 文档路径
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                // 启用 XML 注释
                c.IncludeXmlComments(xmlPath);
            });
            //格式化输出参数
            services.AddControllers().AddNewtonsoftJson(op =>
            {
                op.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
                op.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            });
            services.AddControllers().ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = (content) =>
                {
                    var msg = string.Join(Environment.NewLine, content.ModelState.Values.SelectMany(o => o.Errors.Select(e => e.ErrorMessage)));
                    return new JsonResult(WebResponseContent.Instance.Error(msg));
                };
            });
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
           {
               options.TokenValidationParameters = new TokenValidationParameters
               {
                   SaveSigninToken = true,//保存token,后台验证token是否生效(重要)
                   ValidateIssuer = true,//是否验证Issuer
                   ValidateAudience = true,//是否验证Audience
                   ValidateLifetime = true,//是否验证失效时间
                   ValidateIssuerSigningKey = true,//是否验证SecurityKey
                   ValidAudience = AppSetting.Secret.Audience,//Audience
                   ValidIssuer = AppSetting.Secret.Issuer,//Issuer，这两项和前面签发jwt的设置一致
                   IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppSetting.Secret.JWT))
               };
               options.Events = new JwtBearerEvents()
               {
                   OnChallenge = context =>
                   {
                       context.HandleResponse();
                       context.Response.Clear();
                       context.Response.ContentType = "application/json";
                       context.Response.StatusCode = 401;
                       context.Response.WriteAsync(new { message = "授权未通过", status = false, code = 401 }.Serialize());
                       return Task.CompletedTask;
                   },
                   OnMessageReceived = context =>
                   {
                       string? fixedoken = context.HttpContext.Request.Headers[AppSetting.TokenHeaderName];
                       //token 为空，尝试通过url参数取值
                       if (fixedoken == null)
                       {
                           var authToken = context.HttpContext.Request.Query["authtoken"].ToString();
                           if (authToken != null)
                           {
                               authToken = "Bearer " + authToken;
                               context.HttpContext.Request.Headers[AppSetting.TokenHeaderName] = authToken;
                               fixedoken = authToken;
                           }
                       }
                       return Task.CompletedTask;
                   }
               };
           });

            //配置跨域
            builder.Services.AddCors(option =>
            {
                option.AddPolicy("Cors", builder =>
                {
                    builder.AllowAnyMethod()
                    .AllowAnyOrigin()
                    .AllowAnyHeader();
                });
            });

            services.AddHttpClient();
            services.Configure<CompreFaceOptions>(builder.Configuration.GetSection("CompreFace"));
            services.AddTransient<CompreFaceClient>();
            services.Configure<CapsLedgerOptions>(builder.Configuration.GetSection("Caps"));
            services.AddTransient<WorkLedgerService>();
            services.Configure<ZlmOptions>(builder.Configuration.GetSection("Zlm"));
            services.AddTransient<ZlmClient>();
            services.Configure<VideoPlaybackOptions>(builder.Configuration.GetSection("VideoPlayback"));
            services.Configure<OpenApiOptions>(builder.Configuration.GetSection("OpenApi")); //第三方对接开放接口
            services.AddSingleton<WatchConnectionRegistry>();
            services.AddHostedService<WatchTcpHostedService>();
            services.AddHostedService<VideoPlaybackCleanupService>();

            //静态化Services
            AppService.Services = builder.Services;

            //builder.Services.AddHostedService<FaceSdkInitializationService>();
            var app = builder.Build();
            ChangeToken.OnChange(() => app.Configuration.GetReloadToken(), () =>
            {
                Console.WriteLine($"{DateTime.Now:HH:mm:ss} 更新了配置文件");
                AppSetting.Init(app.Configuration);
                //HangfireJobManager.InitHangfireJob(app.Services, app.Configuration);
            });
            app.UseGlobalExceptionHandler();
            // Configure the HTTP request pipeline.
            
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseKnife4UI(c =>
                {
                    c.RoutePrefix = "swagger";
                    c.SwaggerEndpoint("/v1/swagger.json", "V1 Docs");
                });
            }

            #region 启用Hangfire仪表盘
            //app.UseHangfireDashboard("/hangfire", new DashboardOptions
            //{
            //    //Authorization = new[] { new CustomAuthorizeFilter() },
            //    IsReadOnlyFunc = (context) => false  // 设置仪表盘为可写模式
            //});
            ////if (!builder.Environment.IsDevelopment())
            //{
            //    //初始化任务
            //    //HangfireJobManager.InitHangfireJob(app.Services, app.Configuration);
            //}
            #endregion


            //允许访问静态资源
            app.UseHttpsRedirection();
            //识别wwwroot目录下的任何静态资源文件，默认打开index.html页面
            app.UseFileServer();

            //跨域设置
            app.UseCors("Cors");

            app.UseWebSockets();

            app.UseAuthentication();
            app.UseAuthorization();

            //配置HttpContext
            app.UseStaticHttpContext();

            app.MapControllers();

            //app.UseSpa(spa =>
            //{
            //    spa.Options.SourcePath = "ClientApp"; // 这里假设Vue项目在名为ClientApp的文件夹中
            //    if (app.Environment.IsDevelopment())
            //    {
            //        //spa.UseVueDevelopmentServer();
            //    }
            //    else
            //    {
            //        // 在生产环境下，将请求重定向到index.html来处理前端路由
            //        spa.Options.DefaultPage = "/index.html";
            //    }
            //});
            app.Run();
        }


    }
}
