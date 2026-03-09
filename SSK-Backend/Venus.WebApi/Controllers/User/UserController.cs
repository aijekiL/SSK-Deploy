using Microsoft.AspNetCore.Mvc;
using Venus.Authentication;
using Venus.Authentication.User;
using Venus.Dto.User;
using Venus.Entitys;
using Venus.IService.User;
using Venus.Tools;
using Venus.Tools.Extension;
using Venus.Tools.Request;
using Venus.Tools.Response;
using Venus.Service.EfCore;
using Venus.Core;

namespace Venus.WebApi.Controllers.User
{
    public class UserController : VenusBaseController
    {
        private IUserService _userService;
        private VenusDbContext _DbContext;
        public UserController(IUserService userService, VenusDbContext dbContext)
        {
            _userService = userService;
            _DbContext = dbContext;
        }


        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        [HttpPost]
        public WebResponseContent GetUserListData([FromBody] PageDataOptions options)
        {
            var query = _DbContext.Set<Sys_User>()
                  .Where(o => o.SystemUser == false)
                  .Select((u) => new UserInfoDto
                  {
                      userId = u.UserId,
                      loginName = u.loginName,
                      Name = u.Name,
                      Sex = u.Sex,
                      Number = u.Number,
                      Email = u.Email,
                      LastLoginDate = u.LastLoginDate,
                  });
            var data = query.GetPageDataOptions(options);
            return WebResponseContent.Instance.OK("成功", data);
        }

        public class modifyPasswordDto
        {
            public Guid UserId { get; set; }

            public string newPwd { get; set; } = string.Empty;
        }

        [HttpPost]
        public WebResponseContent ModifyPassword(modifyPasswordDto dto)
        {
            Sys_User user = _DbContext.Set<Sys_User>().First(o => o.UserId == dto.UserId);
            if (user == null)
            {
                return WebResponseContent.Instance.Error("用户不存在！");
            }
            user.Password = dto.newPwd.Trim().EncryptDES(AppSetting.Secret.User);
            _DbContext.Set<Sys_User>().Update(user);
            var i = _DbContext.SaveChanges();
            if (i > 0)
            {
                return WebResponseContent.Instance.OK("密码修改成功！");
            }
            else
            {
                return WebResponseContent.Instance.Error("密码修改失败，数据库无变化！");
            }
        }


        [HttpGet]
        public WebResponseContent GetUser(Guid Id)
        {
            return _userService.Get(Id);
        }

        /// <summary>
        /// 获取当前登录用户信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public WebResponseContent info()
        {
            return _userService.Get(UserContext.Current.UserId);
        }

        /// <summary>
        /// 获取用户菜单
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public WebResponseContent menu()
        {
            return _userService.menu();
        }

        [HttpPost]
        public WebResponseContent ReplaceToken()
        {
            try
            {
                //如果5秒内替换过token,直接使用最新的token(防止一个页面多个并发请求同时替换token导致token错位)
                string requestToken = VenusHttpContext.Current.Request.Headers[AppSetting.TokenHeaderName];
                requestToken = requestToken?.Replace("Bearer ", "");

                if (JwtHelper.IsExp(requestToken)) return WebResponseContent.Instance.Error("Token已过期!");

                var userResp = _userService.Get(UserContext.Current.UserId);
                if (userResp == null || !userResp.Status) return WebResponseContent.Instance.Error("未查到用户信息!");

                string token = JwtHelper.IssueJwt(new JwtUserInfo()
                {
                    UserId = userResp.Data.userId,
                    UserName = userResp.Data.Name,
                    LoginName = userResp.Data.loginName,
                });
                return WebResponseContent.Instance.OK("获取成功", new { token });
            }
            catch (Exception ex)
            {
                return WebResponseContent.Instance.Error("token替换异常");
            }
            finally
            {
            }
        }

        [HttpPost]
        public WebResponseContent Add([FromBody] AddUserDto dto)
        {
            return _userService.Add(dto);
        }

        [HttpPost]
        public WebResponseContent Update([FromBody] UpdateUserDto dto)
        {
            return _userService.Update(dto);
        }
        [HttpPost]
        public WebResponseContent Delete([FromBody] DeleteUserDto dto)
        {
            if (dto.UserId == Guid.Empty)
                return WebResponseContent.Instance.Error("请传入要删除的用户ID");
            return _userService.Delete(dto.UserId);
        }
    }
}

