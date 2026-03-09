using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Venus.Authentication;
using Venus.Entitys;
using Venus.IService.User;
using Venus.Tools;
using Venus.Tools.Extension;
using Venus.Tools.Response;

namespace Venus.WebApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private VenusDbContext _dbContext;
        private IUserService _userService;


        public AuthController(VenusDbContext dbContext, IUserService userService)
        {
            _dbContext = dbContext;
            _userService = userService;
        }

        public class LoginInfo
        {

            [Display(Name = "用户名")]
            [MaxLength(50)]
            [Required(ErrorMessage = "用户名不能为空")]
            public string LoginName { get; set; } = string.Empty;
            [MaxLength(50)]
            [Display(Name = "密码")]
            [Required(ErrorMessage = "密码不能为空")]
            public string Password { get; set; } = string.Empty;
        }

        /// <summary>
        /// WebApi登陆
        /// </summary>
        /// <param name="loginInfo"></param>
        /// <returns></returns>
        [HttpPost, AllowAnonymous]
        public WebResponseContent Login(LoginInfo loginInfo)
        {
            string msg = string.Empty;
            try
            {
                var user = _dbContext.Set<Sys_User>().FirstOrDefault(o => o.loginName == loginInfo.LoginName);
                if (user == null && loginInfo.LoginName == "admin" && loginInfo.Password.Trim() == "111111")
                {
                    user = new Sys_User();
                    user.UserId = Guid.NewGuid();
                    user.loginName = "admin";
                    user.Name = "管理员";
                    user.Password = loginInfo.Password.Trim().EncryptDES(AppSetting.Secret.User);
                    user.Sex = 1;
                    user.Number = 0;
                    user.Email = "huqixing@huwork.cn";
                    user.SystemUser = false;
                    user.LastLoginDate = DateTime.Now;
                    user.RecordStatus = 100;
                    user.CreateID = Guid.Parse("a2213f45-e0c2-11ec-ae74-001132c4ec80");
                    user.CreateDate = DateTime.Now;
                    user.ModifyID = Guid.Parse("a2214c10-e0c2-11ec-ae74-001132c4ec80");
                    user.ModifyDate = DateTime.Now;
                    user.Deleted = false;
                    _dbContext.Set<Sys_User>().Add(user);
                    _dbContext.SaveChanges();
                }
                else
                {
                    if (user == null || loginInfo.Password.Trim().EncryptDES(AppSetting.Secret.User) != (user.Password ?? ""))
                        return WebResponseContent.Instance.Error("用户名或密码错误！");
                }


                string token = JwtHelper.IssueJwt(new JwtUserInfo()
                {
                    UserId = user.UserId,
                    UserName = user.Name,
                    LoginName = user.loginName,
                });
                user.LastLoginDate = DateTime.Now;
                _dbContext.Update(user);
                _dbContext.SaveChanges();
                //user.LastIp = VOL.Core.Utilities.HttpContext.Current.GetUserIp()?.Replace("::ffff:", ""); ;
                //repository.Update(user, x => new { x.LastLoginDate, x.LastIp, x.Token }, true);
                //UserContext.Current.LogOut(user.User_Id);

                loginInfo.Password = string.Empty;

                return WebResponseContent.Instance.OK("登录成功！", new { token, userId = user.UserId, userName = user.Name, loginName = user.loginName });
            }
            catch (Exception ex)
            {
                msg = ex.Message + ex.StackTrace;
                //if (_context.GetService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>().IsDevelopment())
                //{
                //    throw new Exception(ex.Message + ex.StackTrace);
                //}
                return WebResponseContent.Instance.Error(msg);
            }
            finally
            {
                //Logger.Info(LoggerType.Login, loginInfo.Serialize(), webResponse.Message, msg);
            }
        }


        /// <summary>
        /// 保持登录
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public WebResponseContent KeepLogin()
        {
            return WebResponseContent.Instance.OK("成功");
        }
    }
}

