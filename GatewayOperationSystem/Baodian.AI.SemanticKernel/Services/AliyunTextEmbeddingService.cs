using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Baodian.AI.SemanticKernel.Abstractions;

namespace Baodian.AI.SemanticKernel.Services
{
    public class AliyunTextEmbeddingService : IEmbeddingService
    {
        private readonly string _apiKey;
        private readonly string _endpoint;
        private readonly HttpClient _httpClient;
        private readonly string _model = "text-embedding-v4";
        private readonly int _dimension = 768;

        public AliyunTextEmbeddingService(string apiKey, string endpoint, string model = "text-embedding-v4", int dimension = 768, HttpClient? httpClient = null)
        {
            _apiKey = apiKey;
            _endpoint = endpoint;
            _model = model;
            _dimension = dimension;
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
        {
            var embeddings = await GenerateEmbeddingsAsync(new[] { text }, cancellationToken);
            return embeddings.FirstOrDefault() ?? Array.Empty<float>();
        }

        public async Task<IList<float[]>> GenerateEmbeddingsAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
        {
            var requestBody = new AliyunEmbeddingRequest
            {
                Model = _model,
                Input = texts.ToArray(),
                Dimension = _dimension,
                EncodingFormat = "float"
            };
            var request = new HttpRequestMessage(HttpMethod.Post, _endpoint)
            {
                Content = JsonContent.Create(requestBody)
            };
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<AliyunEmbeddingResponse>(cancellationToken: cancellationToken);
            if (result?.Data == null)
                return Array.Empty<float[]>();
            return result.Data.Select(d => d.Embedding ?? Array.Empty<float>()).ToList();
        }

        private class AliyunEmbeddingRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;
            [JsonPropertyName("input")]
            public object Input { get; set; } = Array.Empty<string>(); // string or string[]
            [JsonPropertyName("dimension")]
            public int Dimension { get; set; }
            [JsonPropertyName("encoding_format")]
            public string EncodingFormat { get; set; } = string.Empty;
        }

        private class AliyunEmbeddingResponse
        {
            [JsonPropertyName("data")]
            public List<AliyunEmbeddingData>? Data { get; set; }
        }

        private class AliyunEmbeddingData
        {
            [JsonPropertyName("embedding")]
            public float[]? Embedding { get; set; }
            [JsonPropertyName("index")]
            public int Index { get; set; }
            [JsonPropertyName("object")]
            public string? Object { get; set; }
        }
    }
}
