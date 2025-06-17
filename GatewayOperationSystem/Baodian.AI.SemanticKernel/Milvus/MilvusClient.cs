using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;
using Baodian.AI.SemanticKernel.Milvus.Configuration;
using Baodian.AI.SemanticKernel.Milvus.Services;

namespace Baodian.AI.SemanticKernel.Milvus
{
    public class MilvusClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly LoggingService _loggingService;
        private readonly RetryService _retryService;        public MilvusClient(MilvusOptions options, LoggingService loggingService, RetryService retryService)
        {
            // 支持 EnableSsl 和 UseSSL 两种配置
            var useSSL = options.EnableSsl || options.UseSSL;
            _baseUrl = $"http{(useSSL ? "s" : "")}://{options.Host}:{options.Port}/api/v1";
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(options.Timeout)
            };

            // 支持多种认证方式
            if (!string.IsNullOrEmpty(options.Token))
            {
                // Zilliz Cloud Token 认证
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {options.Token}");
            }
            else if (!string.IsNullOrEmpty(options.ApiKey))
            {
                // API Key 认证
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {options.ApiKey}");
            }
            else if (!string.IsNullOrEmpty(options.Username) && !string.IsNullOrEmpty(options.Password))
            {
                // 基本认证
                var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{options.Username}:{options.Password}"));
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {authToken}");
            }

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            _loggingService = loggingService;
            _retryService = retryService;
        }

        public MilvusClient(string host = "localhost", int port = 9091)
            : this(new MilvusOptions { Host = host, Port = port },
                  new LoggingService(new Microsoft.Extensions.Logging.Abstractions.NullLogger<LoggingService>(), new MilvusOptions()),
                  new RetryService(new MilvusOptions(), new LoggingService(new Microsoft.Extensions.Logging.Abstractions.NullLogger<LoggingService>(), new MilvusOptions())))
        {
        }

        protected async Task<T> GetAsync<T>(string endpoint)
        {
            return await _retryService.ExecuteWithRetryAsync<T>(async () =>
            {
                _loggingService.LogRequest("GET", endpoint);
                var response = await _httpClient.GetAsync($"{_baseUrl}/{endpoint}");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                _loggingService.LogResponse("GET", endpoint, content);
                return JsonSerializer.Deserialize<T>(content, _jsonOptions) ?? throw new InvalidOperationException("Deserialization returned null");
            }, $"GET {endpoint}");
        }

        protected async Task<T> PostAsync<T>(string endpoint, object data)
        {
            return await _retryService.ExecuteWithRetryAsync<T>(async () =>
            {
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                _loggingService.LogRequest("POST", endpoint, json);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/{endpoint}", content);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                _loggingService.LogResponse("POST", endpoint, responseContent);
                return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions) ?? throw new InvalidOperationException("Deserialization returned null");
            }, $"POST {endpoint}");
        }

        protected async Task<T> PutAsync<T>(string endpoint, object data)
        {
            return await _retryService.ExecuteWithRetryAsync<T>(async () =>
            {
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                _loggingService.LogRequest("PUT", endpoint, json);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUrl}/{endpoint}", content);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                _loggingService.LogResponse("PUT", endpoint, responseContent);
                return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions) ?? throw new InvalidOperationException("Deserialization returned null");
            }, $"PUT {endpoint}");
        }

        protected async Task DeleteAsync(string endpoint)
        {
            await _retryService.ExecuteWithRetryAsync<object>(async () =>
            {
                _loggingService.LogRequest("DELETE", endpoint);
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/{endpoint}");
                response.EnsureSuccessStatusCode();
                _loggingService.LogResponse("DELETE", endpoint);
                return new object();
            }, $"DELETE {endpoint}");
        }
    }
}