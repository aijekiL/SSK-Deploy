using Venus.Dto.User;
using Venus.Entitys;
using Venus.IService.User;
using Venus.Tools;
using Venus.Tools.Extension;
using Venus.Tools.Response;

namespace Venus.Service.User
{
    public class UserService : IUserService
    {
        //private BaseRepository<Sys_User> _userRepository;
        private VenusDbContext _dbContext;
        public UserService(VenusDbContext dbContext)
        {
            _dbContext = dbContext;
            //_userRepository = baseRepository;
        }

        public WebResponseContent<UserInfoDto> Get(Guid UserId)
        {
            var data = _dbContext.Set<Sys_User>().Where(o => o.UserId == UserId).Select(o => new UserInfoDto()
            {
                userId = o.UserId,
                loginName = o.loginName,
                Name = o.Name,
                Sex = o.Sex,
                Number = o.Number,
                Email = o.Email,
                LastLoginDate = o.LastLoginDate,
            }).First();
            return WebResponseContent<UserInfoDto>.Instance.OK("获取用户信息成功", data);
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="loginInfo"></param>
        /// <returns></returns>
        public WebResponseContent<Sys_User> Login(LoginInfoDto loginInfo)
        {
            Sys_User user = _dbContext.Set<Sys_User>().First(o => o.loginName == loginInfo.loginName);

            if (user == null || loginInfo.passWord.Trim() != (user.Password ?? ""))
                return WebResponseContent<Sys_User>.Instance.Error("用户名或密码错误！");

            return WebResponseContent<Sys_User>.Instance.OK("登录成功", user);
        }

        public WebResponseContent Add(AddUserDto dto)
        {
            //先查询是否存在此用户名
            var allow = _dbContext.Set<Sys_User>().Any(o => o.loginName == dto.LoginName);
            if (allow)
                return WebResponseContent.Instance.Error("已存在此用户登录名，请更换！");
            var user = new Sys_User();
            user.loginName = dto.LoginName;
            user.Name = dto.Name;
            user.Email = dto.Email;
            user.Password = dto.Password.Trim().EncryptDES(AppSetting.Secret.User);
            user.Sex = dto.Sex.ToInt();
            _dbContext.Add(user);
            return WebResponseContent.Instance.OK("新增成功");
        }
        public WebResponseContent Update(UpdateUserDto dto)
        {
            var user = _dbContext.Set<Sys_User>().FirstOrDefault(o => o.UserId == dto.UserId);
            if (user == null) return WebResponseContent.Instance.Error("要更新的数据不存在");
            user.loginName = dto.LoginName;
            user.Name = dto.Name;
            user.Email = dto.Email;
            user.Sex = dto.Sex.ToInt();
            _dbContext.Update(user);
            return WebResponseContent.Instance.OK("更新成功");
        }

        public WebResponseContent Delete(Guid UserId)
        {
            var entity = _dbContext.Set<Sys_User>().FirstOrDefault(o => o.UserId == UserId);
            if (entity == null)
                return WebResponseContent.Instance.Error("要删除的用户不存在");
            if (entity.loginName == "admin")
                return WebResponseContent.Instance.Error("管理员账号不可删除");
            //int i = _dbContext.Delete(entity);
            return WebResponseContent.Instance.OK("删除成功");
        }

        public class menuOpt
        {
            public Guid MenuId { get; set; }

            public string path { get; set; } = string.Empty;

            public string name { get; set; } = string.Empty;

            public menuMetaOpt meta { get; set; } = new menuMetaOpt();

            public List<menuOpt> children { get; set; } = new();
        }

        public class menuMetaOpt
        {
            public string locale { get; set; } = string.Empty;

            public bool requiresAuth { get; set; }
            public string icon { get; set; } = string.Empty;

            public int order { get; set; }
        }

        private List<menuOpt>? GetChildrenMenus(Guid? menuId)
        {
            //var m = _dbContext.Queryable<Sys_Menu>().Where(o => o.ParentId == menuId).ToList();
            //List<menuOpt> ms = new List<menuOpt>();
            //foreach (var item in m)
            //{
            //    ms.Add(new menuOpt
            //    {
            //        MenuId = item.MenuId,
            //        path = item.routerPath,
            //        name = item.routerName,
            //        meta = new menuMetaOpt()
            //        {
            //            locale = item.MenuLocale,
            //            requiresAuth = item.requiresAuth,
            //            icon = item.icon,
            //            order = item.order,
            //        },
            //        children = GetChildrenMenus(item.MenuId)
            //    });
            //}
            //if (ms.Any())
            //{
            //    return ms;
            //}
            //else
            //{
            //    return null;
            //}
            return null;
        }

        /// <summary>
        /// 获取用户菜单
        /// </summary>
        /// <returns></returns>
        public WebResponseContent menu()
        {
            var menus = GetChildrenMenus(null);
            return WebResponseContent.Instance.OK("", menus);
        }
    }
}

