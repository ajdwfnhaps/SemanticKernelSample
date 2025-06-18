using System;

namespace Baodian.AI.SemanticKernel.Milvus.Configuration
{    public class MilvusOptions
    {
        public string Endpoint { get; set; } = "localhost";
        public int Port { get; set; } = 443;
        public string Username { get; set; }
        public string Password { get; set; }
        public bool EnableSsl { get; set; }
        public int Timeout { get; set; } = 30;

        public string ApiKey { get; set; } = string.Empty; // API Key 用于身份验证
        public string Token { get; set; } = string.Empty; // Token 用于身份验证
        public string Database { get; set; } = "default"; // 默认数据库名称
        public string DatabaseName { get; set; } = "default"; // 数据库名称（兼容配置）
        
        // 集合相关配置
        public string CollectionName { get; set; } = "default_collection";
        public int Dimension { get; set; } = 768;
        public int VectorDimension { get; set; } = 1536;
        public string IndexType { get; set; } = "HNSW";
        public string MetricType { get; set; } = "COSINE";

        // 重试配置
        public RetryOptions Retry { get; set; } = new RetryOptions();
        
        // 日志配置
        public LoggingOptions Logging { get; set; } = new LoggingOptions();
    }

    public class RetryOptions
    {
        public bool Enabled { get; set; } = true;
        public int MaxRetries { get; set; } = 3;
        public int RetryInterval { get; set; } = 1000; // 毫秒
        public bool ExponentialBackoff { get; set; } = true;
    }

    public class LoggingOptions
    {
        public bool Enabled { get; set; } = true;
        public LogLevel MinimumLevel { get; set; } = LogLevel.Information;
        public bool LogRequestContent { get; set; } = true;
        public bool LogResponseContent { get; set; } = true;
    }

    public enum LogLevel
    {
        Debug,
        Information,
        Warning,
        Error
    }
} 