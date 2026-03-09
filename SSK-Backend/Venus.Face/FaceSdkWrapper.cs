using Venus.Face.Dto;
using Venus.Tools;

namespace Venus.Face
{
    public class FaceSdkWrapper : IDisposable, ISingletonDependency
    {
        private readonly SingleThreadExecutor _executor = new SingleThreadExecutor();
        //private readonly OfflineSdk _sdk; // 假设这是不支持多线程的SDK
        private FaceInit _faceInit;
        private FaceManageService _faceManageService;
        private bool _initialized;

        public FaceSdkWrapper()
        {
            // 在执行器线程上初始化SDK
            _executor.Execute(() =>
            {
                _faceInit = new FaceInit();
                _faceInit.Main(AppSetting.Configuration["FaceSDKPath"]);
                _faceManageService = new FaceManageService();
                _initialized = true;
            });
        }

        public bool IsInitialized
        {
            get
            {
                return _initialized;
            }
        }

        public FaceResponseDto<AddUserResultDto> FaceManage_AddUser(face_AddUserDto userDto)
        {
            return _executor.Execute(() =>
            {
                return _faceManageService.UserAdd(userDto);
            });
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="group_id"></param>
        /// <returns></returns>
        public FaceResponseDto<List<UserInfoDto>> FaceManage_GetUserInfo(string userId, string userGroup = "default_face_group")
        {
            return _executor.Execute(() =>
             {
                 return _faceManageService.GetUserInfo(userId, userGroup);
             });
        }


        /// <summary>
        /// 1:N比较，传入二进制流，返回匹配结果
        /// </summary>
        public FaceResponseDto<List<FaceCompareResultDto>> FaceCompare_SearchFacesByImageStream(Stream stream, string userGroup = "default_face_group")
        {
            return _executor.Execute(() =>
            {
                return FaceCompareService.SearchFacesByImageStream(stream, userGroup);
            });
        }

        // 同步调用SDK方法
        //public string Recognize(string input)
        //{
        //    if (!_initialized)
        //        throw new InvalidOperationException("SDK尚未初始化");

        //    return _executor.Execute(() => _sdk.Recognize(input));
        //}

        // 异步调用SDK方法（仍在同一线程执行）
        //public Task<string> RecognizeAsync(string input)
        //{
        //    return _executor.ExecuteAsync(() => Recognize(input));
        //}

        public void Dispose()
        {
            // 在执行器线程上释放SDK资源
            _executor.Execute(() =>
            {
                if (_faceInit != null && _initialized)
                {
                    // _faceInit.Dispose();
                    _initialized = false;
                }
            });

            _executor.Dispose();
        }
    }
}
