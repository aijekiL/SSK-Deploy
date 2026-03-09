using OpenCvSharp;
using System.Runtime.InteropServices;
using System.Text.Json;
using Venus.Face.Common;
using Venus.Face.Dto;
using Venus.Tools;

namespace Venus.Face
{
    /// <summary>
    /// 人脸比较1:1、1:N、抽取人脸特征值、按特征值比较等
    /// </summary>
    public class FaceCompareService : ISingletonDependency
    {
        #region 动态库


        // 特征值比对（传2个人脸的特征值,所有的1:1比对归根结底都是特征值比对，都可通过提取特征值来比对）
        [DllImport("BaiduFaceApi.dll", EntryPoint = "compare_feature", CharSet = CharSet.Ansi
            , CallingConvention = CallingConvention.Cdecl)]
        // type为0表示提取生活照的特征值，1表示证件照的特征值，2表示nir的特征值
        private static extern float compare_feature(ref BDFaceFeature f1, ref BDFaceFeature f2, int type = 0);

        // 人脸1:1比对（传2个人脸的opencv图片帧进行比对）
        [DllImport("BaiduFaceApi.dll", EntryPoint = "match", CharSet = CharSet.Ansi
            , CallingConvention = CallingConvention.Cdecl)]
        // type为0表示提取生活照的特征值，1表示证件照的特征值，2表示nir的特征值
        private static extern int match(IntPtr mat1, IntPtr mat2, int type = 0);

        // 1:N人脸识别（传人脸特征值和库里的比对, 人脸库可参考FaceManager）
        [DllImport("BaiduFaceApi.dll", EntryPoint = "identify", CharSet = CharSet.Ansi
          , CallingConvention = CallingConvention.Cdecl)]
        // type为0表示提取生活照的特征值，1表示证件照的特征值，2表示nir的特征值
        private static extern IntPtr identify(ref BDFaceFeature f1, string group_id_list,
            string user_id, int type = 0);

        // 1:N人脸识别（传人opencv图片帧和库里的比对, 人脸库可参考FaceManager）
        [DllImport("BaiduFaceApi.dll", EntryPoint = "identify_by_mat", CharSet = CharSet.Ansi
          , CallingConvention = CallingConvention.Cdecl)]
        // type为0表示提取生活照的特征值，1表示证件照的特征值，2表示nir的特征值
        private static extern IntPtr identify_by_mat(IntPtr mat, string group_id_list,
            string user_id, int type = 0);

        // 提前加载库里所有数据到内存中
        [DllImport("BaiduFaceApi.dll", EntryPoint = "load_db_face", CharSet = CharSet.Ansi
          , CallingConvention = CallingConvention.Cdecl)]
        public static extern bool load_db_face();

        // 1:N人脸识别（特征值和内存已加载的整个库数据比对）
        [DllImport("BaiduFaceApi.dll", EntryPoint = "identify_with_all", CharSet = CharSet.Ansi
          , CallingConvention = CallingConvention.Cdecl)]
        // type为0表示提取生活照的特征值，1表示证件照的特征值，2表示nir的特征值
        private static extern IntPtr identify_with_all(ref BDFaceFeature f1, int type = 0);

        // 1:N人脸识别（opencv图片帧和内存已加载的整个库数据比对）
        [DllImport("BaiduFaceApi.dll", EntryPoint = "identify_with_all_by_mat", CharSet = CharSet.Ansi
          , CallingConvention = CallingConvention.Cdecl)]
        // type为0表示提取生活照的特征值，1表示证件照的特征值，2表示nir的特征值
        private static extern IntPtr identify_with_all_by_mat(IntPtr mat, int type = 0);


        #endregion

        /// <summary>
        /// 1:N比较，传入二进制流，返回匹配结果
        /// </summary>
        public static FaceResponseDto<List<FaceCompareResultDto>> SearchFacesByImageStream(Stream stream, string userGroup = "default_face_group")
        {
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            byte[] imageData = memoryStream.ToArray();
            Mat mat = Cv2.ImDecode(imageData, ImreadModes.Color);

            string user_id = "";
            // type为0表示提取生活照的特征值，1表示证件照的特征值，2表示nir的特征值 (type需要和前面特征值提取的一致，否则会出错)
            IntPtr ptr = identify_by_mat(mat.CvPtr, userGroup, user_id);
            var buf = Marshal.PtrToStringAnsi(ptr);
            Console.WriteLine("identify res is:" + buf);
            var result = JsonSerializer.Deserialize<FaceResponseDto<List<FaceCompareResultDto>>>(buf ?? "");
            if(result == null)
            {
                result = new FaceResponseDto<List<FaceCompareResultDto>>();
                result.errno = 5003;
                result.msg = "返回值为空，未知原因";
            }
            return result;
        }
    }
}
