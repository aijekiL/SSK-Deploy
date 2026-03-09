using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Venus.Tools;

namespace Venus.Service
{
    public class FileStorageService
    {
        private readonly GridFSBucket _gridFS;
        private readonly IMongoCollection<GridFSFileInfo> _filesCollection;

        public FileStorageService(MongoClient mongoClient, string databaseName)
        {
            var database = mongoClient.GetDatabase(databaseName);
            _gridFS = new GridFSBucket(database);
            _filesCollection = database.GetCollection<GridFSFileInfo>($"{_gridFS.Options.BucketName}.files");
        }


        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="fileStream"></param>
        /// <param name="fileName"></param>
        /// <param name="contentType"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            return await this.UploadFileAsync(fileStream, fileName, contentType, new BsonDocument());
        }

        /// <summary>
        /// 上传文件并添加元数据
        /// </summary>
        /// <param name="fileStream"></param>
        /// <param name="fileName"></param>
        /// <param name="contentType"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, BsonDocument metadata)
        {
            // 计算文件的 SHA - 256 哈希值
            string fileHash = CalculateFileHash(fileStream);
            fileStream.Position = 0; // 重置流的位置以便后续上传

            // 检查是否已经存在具有相同哈希值的文件
            var filter = Builders<GridFSFileInfo>.Filter.Eq("metadata.fileHash", fileHash);
            var existingFile = await _filesCollection.Find(filter).FirstOrDefaultAsync();

            if (existingFile != null)
            {
                return existingFile.Id.ToString();
            }

            // 初始化访问次数为 0
            metadata.Add("accessCount", 0);
            metadata.Add("contentType", contentType);
            metadata.Add("fileHash", fileHash);
            var options = new GridFSUploadOptions
            {
                Metadata = metadata
            };
            var fileId = await _gridFS.UploadFromStreamAsync(fileName, fileStream, options);
            return fileId.ToString();
        }

        private string CalculateFileHash(Stream fileStream)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(fileStream);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }


        /// <summary>
        /// 下载文件并返回文件名、文件流和元数据
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public async Task<(Stream? fileStream, BsonDocument metadata, string? fileName)> DownloadFileAsync(string fileId)
        {
            if (!ObjectId.TryParse(fileId, out ObjectId objectId))
            {
                throw new FormatException($"{fileId} fileId输入不正确.");
            }

            var memoryStream = new MemoryStream();
            var filter = Builders<GridFSFileInfo>.Filter.Eq("_id", objectId);
            var fileInfo = await _gridFS.Find(filter).FirstOrDefaultAsync();

            if (fileInfo == null)
            {
                return (null, new BsonDocument(), null);
            }

            await _gridFS.DownloadToStreamAsync(objectId, memoryStream);
            memoryStream.Position = 0;

            // 更新访问次数
            await UpdateAccessCount(objectId);

            return (memoryStream, fileInfo.Metadata, fileInfo.Filename);
        }


        // 更新访问次数
        private async Task UpdateAccessCount(ObjectId fileId)
        {
            var filter = Builders<GridFSFileInfo>.Filter.Eq("_id", fileId);
            var update = Builders<GridFSFileInfo>.Update.Inc("metadata.accessCount", 1);
            await _filesCollection.UpdateOneAsync(filter, update);
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="fileId"></param>
        public void DeleteFile(string fileId)
        {
            if (!ObjectId.TryParse(fileId, out ObjectId objectId))
            {
                return;
            }

            try
            {
                _gridFS.Delete(objectId);
            }
            catch (GridFSFileNotFoundException)
            {
                // Ignore missing files.
            }
        }
    }
}
