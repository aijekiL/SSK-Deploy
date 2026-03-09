using OpenCvSharp;
using System.Runtime.InteropServices;
using System.Text.Json;
using Venus.Face.Common;
using Venus.Face.Dto;

namespace Venus.Face
{
    /// <summary>
    /// 人脸库管理服务
    /// </summary>
    public class FaceManageService
    {
        #region 动态库调用

        // 人脸注册(传特征值,特征值可参考FaceFeature.cs提取，亦可参考FaceCompare.cs查看特征值的比对)
        [DllImport("BaiduFaceApi.dll", EntryPoint = "user_add", CharSet = CharSet.Ansi
            , CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr user_add(ref BDFaceFeature f1, string user_id, string group_id,
            string user_info = "");

        // 人脸注册(传opencv图片帧,特征值可参考FaceFeature.cs提取，亦可参考FaceCompare.cs查看特征值的比对)
        [DllImport("BaiduFaceApi.dll", EntryPoint = "user_add_by_mat", CharSet = CharSet.Ansi
            , CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr user_add_by_mat(IntPtr mat, string user_id, string group_id,
            string user_info = "");

        // 人脸更新(传图片帧)
        [DllImport("BaiduFaceApi.dll", EntryPoint = "user_update", CharSet = CharSet.Ansi
            , CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr user_update(IntPtr mat, string user_id, string group_id,
            string user_info = "");

        // 用户删除
        [DllImport("BaiduFaceApi.dll", EntryPoint = "user_delete", CharSet = CharSet.Ansi
           , CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr user_delete(string user_id, string group_id);
        // 组添加
        [DllImport("BaiduFaceApi.dll", EntryPoint = "group_add", CharSet = CharSet.Ansi
           , CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr group_add(string group_id);
        // 组删除
        [DllImport("BaiduFaceApi.dll", EntryPoint = "group_delete", CharSet = CharSet.Ansi
           , CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr group_delete(string group_id);
        // 查询用户信息
        [DllImport("BaiduFaceApi.dll", EntryPoint = "get_user_info", CharSet = CharSet.Ansi
           , CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get_user_info(string user_id, string group_id);

        // 查询用户图片
        [DllImport("BaiduFaceApi.dll", EntryPoint = "get_user_image", CharSet = CharSet.Ansi
           , CallingConvention = CallingConvention.Cdecl)]
        private static extern int get_user_image(IntPtr out_mat, string user_id, string group_id);

        // 用户组列表查询
        [DllImport("BaiduFaceApi.dll", EntryPoint = "get_user_list", CharSet = CharSet.Ansi
           , CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get_user_list(string group_id, int start = 0, int length = 100);
        // 组列表查询
        [DllImport("BaiduFaceApi.dll", EntryPoint = "get_group_list", CharSet = CharSet.Ansi
           , CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get_group_list(int start = 0, int length = 100);

        // 数据库人脸数量查询
        [DllImport("BaiduFaceApi.dll", EntryPoint = "db_face_count", CharSet = CharSet.Ansi
           , CallingConvention = CallingConvention.Cdecl)]
        private static extern int db_face_count(string group_id);
        #endregion

        public FaceManageService()
        {
        }

        /// <summary>
        /// 人脸注册
        /// </summary>
        /// <param name="addDto"></param>
        /// <returns></returns>
        public FaceResponseDto<AddUserResultDto> UserAdd(face_AddUserDto addDto)
        {
            var result = new FaceResponseDto<AddUserResultDto>();

            // 人脸注册
            // 通过图片帧人脸注册（只有该方法进行的人脸注册，人脸库才会保存人脸图片)
            Mat? mat = null;
            IntPtr resultPtr = IntPtr.Zero;
            try
            {
                if (addDto.ImageBytes != null && addDto.ImageBytes.Length > 0)
                {
                    if (!ValidateImage(addDto.ImageBytes))
                    {
                        result.errno = 5000;
                        result.msg = "传入的图片校验不合法";
                        return result;
                    }

                    mat = Cv2.ImDecode(addDto.ImageBytes, ImreadModes.Color);
                }
                else
                {
                    mat = Cv2.ImRead(addDto.file_name);
                }
                if (mat == null || mat.Empty())
                {
                    result.errno = 5000;
                    result.msg = "传入的图片校验不合法";
                    return result;
                }

                //var f1 = new FaceFeature().get_face_feature(mat);
                //if (f1 == null || f1.Length == 0)
                //{
                //    result.errno = 5001;
                //    result.msg = "获取人脸特征值失败";
                //    return result;
                //}

                //resultPtr = user_add(ref f1[0], addDto.user_id, addDto.group_id, addDto.user_info);

                resultPtr = user_add_by_mat(mat.CvPtr, addDto.user_id, addDto.group_id, addDto.user_info);
                // 检查指针有效性
                if (resultPtr == IntPtr.Zero)
                {
                    Console.WriteLine("本机方法返回空指针");
                    result.errno = 5002;
                    result.msg = "本机方法返回空指针";
                    return result;
                }

                var mbuf = Marshal.PtrToStringAnsi(resultPtr);
                result = JsonSerializer.Deserialize<FaceResponseDto<AddUserResultDto>>(mbuf ?? "");
                if (result == null)
                {
                    result = new FaceResponseDto<AddUserResultDto>();
                    result.errno = 5003;
                    result.msg = "返回值为空，未知原因";
                    return result;
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                mat?.Dispose();
                if (resultPtr != IntPtr.Zero)
                {
                    //FreeResultPtr(resultPtr);
                }
            }
            return null;

        }

        public bool ValidateImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length < 10)
                return false;

            // 检查文件头
            if (!IsImageHeaderValid(imageData))
                return false;

            // 尝试解码（开销较大，可选）
            try
            {
                using (var mat = Cv2.ImDecode(imageData, ImreadModes.Color))
                {
                    return !mat.Empty();
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 检查字节数组是否为有效的 JPG 或 PNG 图片
        /// </summary>
        private bool IsImageHeaderValid(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 8)
                return false;

            // JPEG: FF D8 FF
            if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
                return true;

            // PNG: 89 50 4E 47 0D 0A 1A 0A
            if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E &&
                bytes[3] == 0x47 && bytes[4] == 0x0D && bytes[5] == 0x0A &&
                bytes[6] == 0x1A && bytes[7] == 0x0A)
                return true;

            return false;
        }

        /// <summary>
        /// 查询用户信息
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="group_id"></param>
        /// <returns></returns>
        public FaceResponseDto<List<UserInfoDto>> GetUserInfo(string user_id, string group_id)
        {
            FaceResponseDto<List<UserInfoDto>>? result=  new FaceResponseDto<List<UserInfoDto>>();
            IntPtr resultPtr = get_user_info(user_id, group_id);
            var buf = Marshal.PtrToStringAnsi(resultPtr);
            // 检查指针有效性
            if (resultPtr == IntPtr.Zero)
            {
                Console.WriteLine("本机方法返回空指针");
                result.errno = 5002;
                result.msg = "本机方法返回空指针";
                return result;
            }

            var mbuf = Marshal.PtrToStringAnsi(resultPtr);
            result = JsonSerializer.Deserialize<FaceResponseDto<List<UserInfoDto>>>(mbuf ?? "");
            if (result == null)
            {
                result = new FaceResponseDto<List<UserInfoDto>>();
                result.errno = 5003;
                result.msg = "返回值为空，未知原因";
                return result;
            }
            return result;
        }

        /// <summary>
        /// 用户组列表查询
        /// </summary>
        /// <param name="group_id"></param>
        /// <returns></returns>
        public string GetUserList(string group_id)
        {
            IntPtr ptr = get_user_list(group_id);
            var buf = Marshal.PtrToStringAnsi(ptr);
            Console.WriteLine("get_user_list res is:" + buf);
            return buf;
        }
    }
}
