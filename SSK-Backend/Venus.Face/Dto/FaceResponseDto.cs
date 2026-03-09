namespace Venus.Face.Dto
{
    /// <summary>
    /// 人脸响应实体
    /// </summary>
    public class FaceResponseDto<T>
    {
        /// <summary>
        /// 
        /// </summary>
        public int errno { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string msg { get; set; } = string.Empty;
        /// <summary>
        /// 数据体
        /// </summary>
        public FaceResponseDataDto<T>? data { get; set; }
    }

    public class FaceResponseDataDto<T>
    {
        public string? face_token {  get; set; }
        public string log_id { get; set; } = string.Empty;
        public T result { get; set; }
    }
}

