using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using Venus.Core;
using Venus.Entitys;
using Venus.Entitys.Sys;
using Venus.IService.Attachment;
using Venus.Tools.Response;

namespace Venus.Service.Attachment
{
    public class AttachmentService : ServiceBase, IAttachmentService
    {
        private readonly VenusDbContext _dbContext;
        private readonly FileStorageService _fileStorageService;
        public AttachmentService(VenusDbContext dbContext, FileStorageService fileStorageService, ICurrentUserContextService currentUserContextService)
            : base(currentUserContextService)
        {
            _dbContext = dbContext;
            _fileStorageService = fileStorageService;
        }


        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public async Task<WebResponseContent<List<Sys_Attachment>>> Upload(List<IFormFile> fileInput, string? BizObjectId, string BizObjectTable, string DataField)
        {
            int i = 0;
            List<Sys_Attachment> dtos = new List<Sys_Attachment>();
            try
            {
                for (i = 0; i < fileInput.Count; i++)
                {
                    Sys_Attachment doc = new Sys_Attachment();
                    doc.ObjectId = Guid.NewGuid();
                    doc.BizObjectTable = BizObjectTable;
                    doc.BizObjectId = BizObjectId;
                    doc.DataField = DataField;
                    doc.FileName = fileInput[i].FileName;
                    doc.FileExt = System.IO.Path.GetExtension(doc.FileName);
                    doc.ContentLength = fileInput[i].Length;
                    doc.ContentType = fileInput[i].ContentType;

                    //写入到mangodb
                    using var mongoStream = fileInput[i].OpenReadStream();
                    var fileId = await _fileStorageService.UploadFileAsync(mongoStream, doc.FileName, doc.ContentType);
                    doc.MongoId = fileId;

                    doc.CreateID = base._currentUserContextService.UserId;
                    //doc.Creator = UserContext.Current.UserNameView;
                    doc.CreateDate = DateTime.Now;
                    _dbContext.Add(doc);
                    _dbContext.SaveChanges();
                    dtos.Add(doc);
                }
            }
            catch (Exception ex)
            {
                //Logger.Error($"上传文件失败：路径：{filePath},失败文件:{fileInput[i]},{ex.Message + ex.StackTrace}");
                return WebResponseContent<List<Sys_Attachment>>.Instance.Error("文件上传失败:" + ex.Message);
            }

            System.Diagnostics.Debug.WriteLine($"Get UserId 文件服务: {_currentUserContextService.UserId}");
            return WebResponseContent<List<Sys_Attachment>>.Instance.OK("文件上传成功", dtos);
        }

        /// <summary>
        /// 获取文件
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public async Task<WebResponseContent<(Stream? fileStream, Dictionary<string,object> metadata, string? fileName)>> Get(string fileId)
        {
            WebResponseContent<(Stream? fileStream, Dictionary<string, object> metadata, string? fileName)> result = new();
            var (fileStream, metadata, fileName) = await _fileStorageService.DownloadFileAsync(fileId);
            if (fileStream == null)
            {
                return result.Error("文件不存在");
            }
            return result.OK("获取成功", (fileStream, BsonDocumentToDictionary(metadata), fileName));
        }

        #region Bson转Dictionary
        private Dictionary<string, object> BsonDocumentToDictionary(BsonDocument bsonDocument)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            foreach (var element in bsonDocument)
            {
                string key = element.Name;
                object value;

                switch (element.Value.BsonType)
                {
                    case BsonType.Document:
                        value = BsonDocumentToDictionary(element.Value.AsBsonDocument);
                        break;
                    case BsonType.Array:
                        value = BsonArrayToList(element.Value.AsBsonArray);
                        break;
                    default:
                        value = element.Value;
                        break;
                }

                dictionary.Add(key, value);
            }
            return dictionary;
        }

        private List<object> BsonArrayToList(BsonArray bsonArray)
        {
            List<object> list = new List<object>();
            foreach (var item in bsonArray)
            {
                object value;
                switch (item.BsonType)
                {
                    case BsonType.Document:
                        value = BsonDocumentToDictionary(item.AsBsonDocument);
                        break;
                    case BsonType.Array:
                        value = BsonArrayToList(item.AsBsonArray);
                        break;
                    default:
                        value = item;
                        break;
                }
                list.Add(value);
            }
            return list;
        }

        #endregion
    }
}
