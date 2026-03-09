using System.Runtime.InteropServices;
using Venus.Face.Common;

namespace Venus.Face
{
    public class FaceInit
    {
        #region 动态库
        // sdk初始化
        [DllImport("BaiduFaceApi.dll", EntryPoint = "sdk_init", CharSet = CharSet.Ansi
            , CallingConvention = CallingConvention.Cdecl)]
        private static extern int sdk_init(string model_path);
        // 是否授权
        [DllImport("BaiduFaceApi.dll", EntryPoint = "is_auth", CharSet = CharSet.Ansi
               , CallingConvention = CallingConvention.Cdecl)]
        private static extern bool is_auth();
        // sdk销毁
        [DllImport("BaiduFaceApi.dll", EntryPoint = "sdk_destroy", CharSet = CharSet.Ansi
            , CallingConvention = CallingConvention.Cdecl)]
        private static extern void sdk_destroy();

        // 获取设备指纹
        [DllImport("BaiduFaceApi.dll", EntryPoint = "get_device_id", CharSet = CharSet.Ansi
            , CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get_device_id();

        // 获取sdk版本号
        [DllImport("BaiduFaceApi.dll", EntryPoint = "sdk_version", CharSet = CharSet.Ansi
            , CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr sdk_version();
        #endregion
        // 获取sdk信息如设备指纹，版本号
        void get_sdk_info()
        {
            // 获取设备指纹
            IntPtr ptr = get_device_id();
            var buf = Marshal.PtrToStringAnsi(ptr);
            Console.WriteLine("设备ID是:" + buf);
            // 获取sdk版本号
            IntPtr ptr_v = sdk_version();
            var vbuf = Marshal.PtrToStringAnsi(ptr_v);
            Console.WriteLine("SDK版本是:" + vbuf);
        }

        // 人脸c#入口方法
        public void Main(string? model_path = null)
        {
            Console.WriteLine("in main");
            // model_path为模型文件夹路径，即models文件夹（里面存的是人脸识别的模型文件）
            // 传空为采用默认路径，若想定置化路径，请填写全局路径如：d:\\face （models模型文件夹目录放置后为d:\\face\\models）
            // 若模型文件夹采用定置化路径，则激活文件(license.ini, license.key)也可采用定制化路径放置到该目录如d:\\face\\license
            // 亦可在激活文件默认生成的路径
            // string model_path = null;
            //model_path = "F:\\csepdiCode\\FaceOfflineSdk\\FaceOfflineSdk\\bin\\";
            long time_begin = TimeUtil.get_time_stamp();
            int n = sdk_init(model_path);
            //若没通过初始化，则n不为0, 返回的错误码及原因可参考说明文档
            if (n != 0)
            {
                Console.WriteLine("sdk初始化失败，错误代码为 {0:D}", n);
                throw new Exception($"sdk初始化失败，错误代码为 {n:D}");
            }
            // 获取设备指纹
            get_sdk_info();
            // 验证是否授权
            bool authed = is_auth();
            if (!authed)
            {
                Console.WriteLine("设备未授权");
                throw new Exception("设备未授权");
            }
            Console.WriteLine("设备是否已授权:" + authed);
            long time_end = TimeUtil.get_time_stamp();
            Console.WriteLine("sdk初始成本 {0:D}ms", time_end - time_begin);

        }
    }
}
