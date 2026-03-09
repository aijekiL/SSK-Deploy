using Microsoft.AspNetCore.Http;
using Venus.Entitys.Sys;
using Venus.Tools;
using Venus.Tools.Response;

namespace Venus.IService.Attachment
{
    public interface IAttachmentService : ITransientDependency
    {

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        Task<WebResponseContent<List<Sys_Attachment>>> Upload(List<IFormFile> fileInput, string? BizObjectId, string BizObjectTable, string DataField);

        Task<WebResponseContent<(Stream? fileStream, Dictionary<string, object> metadata, string? fileName)>> Get(string fileId);
    }
}
