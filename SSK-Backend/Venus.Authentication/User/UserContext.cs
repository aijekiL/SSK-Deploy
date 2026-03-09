using Venus.Tools;

namespace Venus.Authentication.User
{
    public static class UserContext
    {
        public static JwtUserInfo Current => GetCurUserInfo();


        /// <summary>
        /// 获取当前用户身份缓存对象
        /// </summary>
        /// <returns></returns>
        public static JwtUserInfo GetCurUserInfo()
        {
            JwtUserInfo userInfo = new JwtUserInfo();
            var headrs = VenusHttpContext.Current.Request.Headers;
            //判断
            if (headrs.ContainsKey("QuartzAccessKey"))
            {
                userInfo = new JwtUserInfo()
                {
                    UserId = Guid.Empty,
                    UserName = "自动服务",
                    LoginName = "QuartzAccessKey"
                };
            }
            else
            {
                var authorization = headrs["Authorization"];
                string token = authorization!;
                if (token == null) return new JwtUserInfo();
                if (token.Contains("Bearer"))
                {
                    string[] tokenStr = authorization.ToString().Split(' ');
                    token = tokenStr[1];
                }
                //解密
                userInfo = JwtHelper.SerializeJwt(token);
            }
            return userInfo;
        }
    }
}
