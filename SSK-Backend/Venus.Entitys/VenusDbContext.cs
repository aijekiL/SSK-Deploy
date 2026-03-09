using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Loader;
using Venus.Tools;

namespace Venus.Entitys
{
    public class VenusDbContext : DbContext
    {

        /// <summary>
        /// 数据库连接名称 
        /// </summary>
        public string DataBaseName = null;
        public VenusDbContext()
                : base()
        {
        }
        public VenusDbContext(string connction)
            : base()
        {
            DataBaseName = connction;
        }

        public VenusDbContext(DbContextOptions<VenusDbContext> options)
            : base(options)
        {

        }
        public override void Dispose()
        {
            base.Dispose();
        }
        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (Exception ex)//DbUpdateException 
            {
                throw (ex.InnerException as Exception ?? ex);
            }
        }
        public override DbSet<TEntity> Set<TEntity>()
        {
            return base.Set<TEntity>();
        }

        /// <summary>
        /// 设置跟踪状态
        /// </summary>
        public bool QueryTracking
        {
            set
            {
                this.ChangeTracker.QueryTrackingBehavior =
                       value ? QueryTrackingBehavior.TrackAll
                       : QueryTrackingBehavior.NoTracking;
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = AppSetting.DbConnectionString;

            string DBType = AppSetting.GetSection("Connection")["DBType"] ?? "SqlServer";

            switch (DBType)
            {
                case "SqlServer":
                    optionsBuilder.UseSqlServer(connectionString);
                    break;
                case "MySql":
                    var serverVersion = ServerVersion.AutoDetect(connectionString);
                    optionsBuilder.UseMySql(connectionString, serverVersion);
                    break;
                default:
                    break;
            }

            //默认禁用实体跟踪
            optionsBuilder = optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            try
            {
                //获取所有类库
                var compilationLibrary = DependencyContext.Default
                    .RuntimeLibraries
                    .Where(x => !x.Serviceable && x.Type != "package" && x.Type == "project");
                foreach (var _compilation in compilationLibrary)
                {
                    //加载指定类
                    AssemblyLoadContext.Default
                    .LoadFromAssemblyName(new AssemblyName(_compilation.Name))
                    .GetTypes()
                    .Where(x =>
                        x.GetTypeInfo().BaseType != null && (x.BaseType == (typeof(BaseEntity)) || x.BaseType == (typeof(DefaultBaseEntity))))
                        .ToList().ForEach(t =>
                        {
                            modelBuilder.Entity(t);
                            //  modelBuilder.Model.AddEntityType(t);
                        });
                }
                base.OnModelCreating(modelBuilder);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            //2023.08.22 CSEPDI.DW.Huqixing 直接在访问数据库的最底层过滤已删除的数据
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var isDeletedProperty = entityType.FindProperty("Deleted");
                if (isDeletedProperty != null)
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "p");
                    var filterIsDeleted = Expression.Equal(Expression.Property(parameter, isDeletedProperty.PropertyInfo)
                                                          , Expression.Constant(false, typeof(bool)));
                    var Delete = Expression.Lambda(filterIsDeleted, parameter);
                    entityType.SetQueryFilter(Delete);
                }
            }

        }
    }
}
