using Microsoft.AspNetCore.Mvc;
using Venus.Authentication;
using Venus.Tools.Response;
using Venus.WebApi.Services;

namespace Venus.WebApi.Controllers
{
    /// <summary>
    /// 人脸管理（CompreFace）
    /// </summary>
    public class FaceManageController : VenusBaseController
    {
        private readonly CompreFaceClient _compreFaceClient;

        public FaceManageController(CompreFaceClient compreFaceClient)
        {
            _compreFaceClient = compreFaceClient;
        }

        /// <summary>
        /// 添加人脸到用户组
        /// </summary>
        /// <param name="userId">用户唯一标识</param>
        /// <param name="userInfo">用户信息（可选）</param>
        /// <param name="groupId">用户组（用于选择API Key，可选）</param>
        /// <param name="fileInput">人脸图片</param>
        [HttpPost]
        public async Task<WebResponseContent> AddFaceToUserGroup(string userId, string? userInfo, string? groupId, [FromForm] IFormFile? fileInput, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return WebResponseContent.Instance.Error("userId不能为空");
            if (fileInput == null || fileInput.Length == 0)
                return WebResponseContent.Instance.Error("请上传人脸图片");

            try
            {
                await using var stream = fileInput.OpenReadStream();
                var result = await _compreFaceClient.AddFaceAsync(userId, stream, fileInput.FileName, fileInput.ContentType, groupId, cancellationToken);
                return WebResponseContent.Instance.OK("添加成功", result);
            }
            catch (Exception ex)
            {
                return WebResponseContent.Instance.Error($"添加失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 人脸检索
        /// </summary>
        /// <param name="minSimilarity">最小相似度（百分比）</param>
        /// <param name="groupId">用户组（用于选择API Key，可选）</param>
        /// <param name="fileInput">人脸图片</param>
        [HttpPost]
        public async Task<WebResponseContent> SearchFacesInGroup(int minSimilarity, string? groupId, [FromForm] IFormFile? fileInput, CancellationToken cancellationToken)
        {
            if (fileInput == null || fileInput.Length == 0)
                return WebResponseContent.Instance.Error("请上传人脸图片");

            try
            {
                await using var stream = fileInput.OpenReadStream();
                var response = await _compreFaceClient.RecognizeAsync(stream, fileInput.FileName, fileInput.ContentType, groupId, null, null, cancellationToken);
                var results = new Dictionary<string, decimal>();

                if (response?.result != null)
                {
                    foreach (var item in response.result)
                    {
                        if (item?.subjects == null) continue;
                        foreach (var subject in item.subjects)
                        {
                            if (string.IsNullOrWhiteSpace(subject?.subject) || subject.similarity == null)
                                continue;
                            if (!results.TryGetValue(subject.subject, out var current) || subject.similarity.Value > current)
                                results[subject.subject] = subject.similarity.Value;
                        }
                    }
                }

                var filtered = results
                    .Select(x => new
                    {
                        user_id = x.Key,
                        score = Math.Round(x.Value * 100m, 2)
                    })
                    .Where(x => x.score >= minSimilarity)
                    .OrderByDescending(x => x.score)
                    .ToList();

                var payload = new
                {
                    errno = 0,
                    msg = "success",
                    data = new
                    {
                        result = filtered
                    }
                };
                return WebResponseContent.Instance.OK("识别成功", payload);
            }
            catch (Exception ex)
            {
                return WebResponseContent.Instance.Error($"识别失败：{ex.Message}");
            }
        }
    }
}
