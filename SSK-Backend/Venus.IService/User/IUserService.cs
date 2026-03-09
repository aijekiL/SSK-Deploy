using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Venus.Dto.User;
using Venus.Entitys;
using Venus.Tools;
using Venus.Tools.Response;

namespace Venus.IService.User
{
    public interface IUserService: ITransientDependency
    {

        WebResponseContent<UserInfoDto> Get(Guid UserId);

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="loginInfo"></param>
        /// <returns></returns>
        WebResponseContent<Sys_User> Login(LoginInfoDto loginInfo);

        WebResponseContent Add(AddUserDto dto);

        WebResponseContent Update(UpdateUserDto dto);
        WebResponseContent Delete(Guid UserId);
        /// <summary>
        /// 获取用户菜单
        /// </summary>
        /// <returns></returns>
        WebResponseContent menu();
    }
}
