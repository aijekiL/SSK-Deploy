using System.Reflection;
using System.Text.RegularExpressions;
using Venus.Tools;

namespace Venus.WebApi.Common
{
    public static class ServicesSetup
    {
        /// <summary>
        /// 自动注入服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            #region 依赖注入
            //services.AddScoped<IUserService, UserService>();           
            var baseType = typeof(IDependency);
            var ScopeType = typeof(IScopeDependency);
            var SingletonType = typeof(ISingletonDependency);
            var TransientType = typeof(ITransientDependency);

            var path = AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory;
            var getFiles = Directory.GetFiles(path, "*.dll").Where(Match);  //.Where(o=>o.Match())
            var referencedAssemblies = getFiles.Select(Assembly.LoadFrom).ToList();  //.Select(o=> Assembly.LoadFrom(o))         

            var ss = referencedAssemblies.SelectMany(o => o.GetTypes());

            var types = referencedAssemblies
                .SelectMany(a => a.DefinedTypes)
                .Select(type => type.AsType())
                .Where(x => !new Type[] { baseType, ScopeType, SingletonType, TransientType }.Contains(x) && baseType.IsAssignableFrom(x)).ToList();
            var implementTypes = types.Where(x => x.IsClass).ToList();
            var interfaceTypes = types.Where(x => x.IsInterface).ToList();
            foreach (var implementType in implementTypes)
            {
                if (typeof(IScopeDependency).IsAssignableFrom(implementType))
                {
                    var interfaceType = interfaceTypes.FirstOrDefault(x => x.IsAssignableFrom(implementType));
                    if (interfaceType != null)
                        services.AddScoped(interfaceType, implementType);
                }
                else if (typeof(ISingletonDependency).IsAssignableFrom(implementType))
                {
                    var interfaceType = interfaceTypes.FirstOrDefault(x => x.IsAssignableFrom(implementType));
                    if (interfaceType != null)
                        services.AddSingleton(interfaceType, implementType);
                    else
                        services.AddSingleton(implementType);
                }
                else
                {
                    var interfaceType = interfaceTypes.FirstOrDefault(x => x.IsAssignableFrom(implementType));
                    if (interfaceType != null)
                        services.AddTransient(interfaceType, implementType);
                }
            }
            #endregion

            return services;
        }

        /// <summary>
        /// 程序集是否匹配
        /// </summary>
        public static bool Match(string assemblyName)
        {
            // 要扫描的程序集名称
            // 默认为[^Shop.Utils|^Shop.]多个使用|分隔
            string MatchAssemblies = "^Venus.";

            assemblyName = Path.GetFileName(assemblyName);
            if (assemblyName.StartsWith($"{AppDomain.CurrentDomain.FriendlyName}.Views"))
                return false;
            if (assemblyName.StartsWith($"{AppDomain.CurrentDomain.FriendlyName}.PrecompiledViews"))
                return false;
            return Regex.IsMatch(assemblyName, MatchAssemblies, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }
    }
}
