using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Venus.Tools;

namespace Venus.WebApi.Services
{
    public class CompreFaceClient : ITransientDependency
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CompreFaceOptions _options;

        public CompreFaceClient(IHttpClientFactory httpClientFactory, IOptions<CompreFaceOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value ?? new CompreFaceOptions();
        }

        public async Task<CompreFaceAddFaceResponse?> AddFaceAsync(string subject, Stream imageStream, string fileName, string? contentType, string? groupId, CancellationToken cancellationToken)
        {
            var apiKey = ResolveApiKey(groupId);
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("CompreFace API key is not configured.");
            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException("subject is required", nameof(subject));

            var url = BuildUrl($"faces?subject={Uri.EscapeDataString(subject)}");
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("x-api-key", apiKey);

            using var form = new MultipartFormDataContent();
            using var fileContent = new StreamContent(imageStream);
            if (!string.IsNullOrWhiteSpace(contentType))
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            form.Add(fileContent, "file", fileName);
            request.Content = form;

            using var response = await SendAsync(request, cancellationToken);
            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            EnsureSuccess(response, payload);
            return JsonSerializer.Deserialize<CompreFaceAddFaceResponse>(payload, JsonOptions);
        }

        public async Task<CompreFaceRecognizeResponse?> RecognizeAsync(Stream imageStream, string fileName, string? contentType, string? groupId, int? predictionCount, int? limit, CancellationToken cancellationToken)
        {
            var apiKey = ResolveApiKey(groupId);
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("CompreFace API key is not configured.");

            var query = new List<string>();
            var limitValue = limit ?? _options.DefaultLimit;
            var predictionValue = predictionCount ?? _options.DefaultPredictionCount;
            if (limitValue > 0) query.Add($"limit={limitValue}");
            if (predictionValue > 0) query.Add($"prediction_count={predictionValue}");
            var queryString = query.Count > 0 ? "?" + string.Join("&", query) : string.Empty;

            var url = BuildUrl($"recognize{queryString}");
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("x-api-key", apiKey);

            using var form = new MultipartFormDataContent();
            using var fileContent = new StreamContent(imageStream);
            if (!string.IsNullOrWhiteSpace(contentType))
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            form.Add(fileContent, "file", fileName);
            request.Content = form;

            using var response = await SendAsync(request, cancellationToken);
            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            EnsureSuccess(response, payload);
            return JsonSerializer.Deserialize<CompreFaceRecognizeResponse>(payload, JsonOptions);
        }

        private string BuildUrl(string path)
        {
            var baseUrl = (_options.BaseUrl ?? string.Empty).TrimEnd('/');
            var recognitionPath = (_options.RecognitionPath ?? string.Empty).Trim('/');
            if (string.IsNullOrWhiteSpace(recognitionPath))
                return $"{baseUrl}/{path.TrimStart('/')}";
            return $"{baseUrl}/{recognitionPath}/{path.TrimStart('/')}";
        }

        private string ResolveApiKey(string? groupId)
        {
            if (!string.IsNullOrWhiteSpace(groupId) && _options.GroupApiKeys.TryGetValue(groupId, out var apiKey) && !string.IsNullOrWhiteSpace(apiKey))
                return apiKey;
            return _options.ApiKey;
        }

        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(Math.Max(5, _options.TimeoutSeconds));
            return await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }

        private static void EnsureSuccess(HttpResponseMessage response, string payload)
        {
            if (response.IsSuccessStatusCode)
                return;
            throw new InvalidOperationException($"CompreFace error {(int)response.StatusCode}: {payload}");
        }
    }

    public sealed class CompreFaceAddFaceResponse
    {
        public string? image_id { get; set; }
        public string? subject { get; set; }
    }

    public sealed class CompreFaceRecognizeResponse
    {
        public List<CompreFaceRecognizeResult>? result { get; set; }
    }

    public sealed class CompreFaceRecognizeResult
    {
        public List<CompreFaceSubject>? subjects { get; set; }
    }

    public sealed class CompreFaceSubject
    {
        public string? subject { get; set; }
        public decimal? similarity { get; set; }
    }
}
