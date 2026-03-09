namespace Venus.WebApi.Services
{
    public sealed class CompreFaceOptions
    {
        /// <summary>
        /// CompreFace base URL, e.g. http://localhost:8000
        /// </summary>
        public string BaseUrl { get; set; } = "http://localhost:8000";

        /// <summary>
        /// Recognition service base path.
        /// </summary>
        public string RecognitionPath { get; set; } = "/api/v1/recognition";

        /// <summary>
        /// Default API key for the recognition service.
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Optional per-group API keys.
        /// </summary>
        public Dictionary<string, string> GroupApiKeys { get; set; } = new();

        /// <summary>
        /// Default prediction count (subjects per face).
        /// </summary>
        public int DefaultPredictionCount { get; set; } = 1;

        /// <summary>
        /// Default limit (max faces to detect). 0 means server default.
        /// </summary>
        public int DefaultLimit { get; set; } = 0;

        /// <summary>
        /// HTTP timeout for CompreFace requests.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;
    }
}
