using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Venus.Authentication;
using Venus.Entitys;
using Venus.Entitys.Sys;
using Venus.IService.Attachment;
using Venus.Tools.Response;

namespace Venus.WebApi.Controllers.Attachment
{
    /// <summary>
    /// 附件管理API
    /// </summary>
    public class AttachmentController : VenusBaseController
    {
        private readonly IAttachmentService _attachmentService;
        private readonly VenusDbContext _dbContext;
        public AttachmentController(IAttachmentService attachmentService, VenusDbContext dbContext)
        {
            _attachmentService = attachmentService;
            _dbContext = dbContext;
        }

        /// <summary>
        /// 上传附件
        /// </summary>
        /// <param name="fileInput">附件集合</param>
        /// <param name="BizObjectTable">业务表名</param>
        /// <param name="DataField">业务表字段</param>
        /// <returns></returns>
        [HttpPost, Route("{BizObjectTable}/{DataField}")]
        public virtual async Task<WebResponseContent> Upload(List<IFormFile> fileInput, string BizObjectTable, string DataField)
        {
            //var BizObjectTable = Request.Query["BizObjectTable"].ToString();
            if (string.IsNullOrEmpty(BizObjectTable)) return WebResponseContent.Instance.Error("上传接口缺失[BizObjectTable]字段,请联系管理员！");

            //var DataField = Request.Query["DataField"].ToString();
            if (string.IsNullOrEmpty(DataField)) return WebResponseContent.Instance.Error("上传接口缺失[DataField]字段,请联系管理员！");

            string BizObjectId = Request.Query["BizObjectId"].ToString();

            var result = await _attachmentService.Upload(fileInput, BizObjectId, BizObjectTable, DataField);
            return new WebResponseContent
            {
                Status = result.Status,
                Message = result.Message,
                Data = result.Data
            };
        }


        /// <summary>
        /// 下载附件
        /// </summary>
        /// <param name="AttachmentId"></param>
        /// <returns></returns>
        [HttpGet("Download/{AttachmentId:guid}")]
        public async Task<IActionResult> Download(Guid AttachmentId)
        {
            var doc = _dbContext.Set<Sys_Attachment>().FirstOrDefault(o => o.ObjectId == AttachmentId);
            if (doc == null) { return Json(WebResponseContent.Instance.Error("数据库中未找到此记录！")); }

            var att = await _attachmentService.Get(doc.MongoId);
            if (!att.Status)
            {
                return Json(WebResponseContent.Instance.Error(att.Message));
            }

            if (att.Data.fileStream == null)
            {
                return Json(WebResponseContent.Instance.Error("文件流为空"));
            }
            // 从元数据中获取 contentType
            var contentType = att.Data.metadata.ContainsKey("contentType")
                ? (att.Data.metadata["contentType"]?.ToString() ?? "application/octet-stream")
                : "application/octet-stream";
            Response.Headers["Content-Type"] = contentType;

            // 对文件名进行 URL 编码
            var encodedFileName = System.Web.HttpUtility.UrlEncode(att.Data.fileName ?? "download", System.Text.Encoding.UTF8) ?? "download";
            var contentDisposition = $"attachment; filename*=UTF-8''{encodedFileName}";
            Response.Headers["Content-Disposition"] = contentDisposition;

            return File(att.Data.fileStream, contentType);
        }

        /// <summary>
        /// 预览附件
        /// </summary>
        /// <param name="AttachmentId"></param>
        /// <returns></returns>
        [HttpGet("Preview/{AttachmentId:guid}")]
        public async Task<IActionResult> Preview(Guid AttachmentId)
        {
            var doc = _dbContext.Set<Sys_Attachment>().FirstOrDefault(o => o.ObjectId == AttachmentId);
            if (doc == null) { return Json(WebResponseContent.Instance.Error("数据库中未找到此记录！")); }

            var att = await _attachmentService.Get(doc.MongoId);
            if (!att.Status)
            {
                return Json(WebResponseContent.Instance.Error(att.Message));
            }

            if (att.Data.fileStream == null)
            {
                return Json(WebResponseContent.Instance.Error("文件流为空"));
            }
            // 从元数据中获取 contentType
            var contentType = att.Data.metadata.ContainsKey("contentType")
                ? (att.Data.metadata["contentType"]?.ToString() ?? "application/octet-stream")
                : "application/octet-stream";
            Response.Headers["Content-Type"] = contentType;

            // 对文件名进行 URL 编码
            var encodedFileName = System.Web.HttpUtility.UrlEncode(att.Data.fileName ?? "download", System.Text.Encoding.UTF8) ?? "download";
            var contentDisposition = $"inline; filename*=UTF-8''{encodedFileName}";
            Response.Headers["Content-Disposition"] = contentDisposition;
            // Preview URL is often reused by BizObjectId route; disable client/proxy cache to avoid stale images.
            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            return File(att.Data.fileStream, contentType);
        }

        /// <summary>
        /// 根据业务ID获取附件
        /// </summary>
        /// <param name="type"></param>
        /// <param name="BizObjectTable"></param>
        /// <param name="DataField"></param>
        /// <param name="BizObjectId"></param>
        /// <returns></returns>
        [HttpGet,Route("GetFileToBusinessId/{type}")]
        public async Task<IActionResult> GetFileToBusinessId(string type, string BizObjectTable, string DataField, string BizObjectId)
        {
            // Always pick the latest attachment bound to this business object/field.
            var doc = _dbContext.Set<Sys_Attachment>()
                .Where(o => o.BizObjectTable == BizObjectTable && o.DataField == DataField && o.BizObjectId == BizObjectId)
                .OrderByDescending(o => o.ModifyDate ?? o.CreateDate)
                .ThenByDescending(o => o.CreateDate)
                .FirstOrDefault();
            if (doc == null) { return Json(WebResponseContent.Instance.Error("数据库中未找到此记录！")); }

            if (type == "Preview")
                return await this.Preview(doc.ObjectId);
            else
                return await this.Download(doc.ObjectId);

        }


    }
}
