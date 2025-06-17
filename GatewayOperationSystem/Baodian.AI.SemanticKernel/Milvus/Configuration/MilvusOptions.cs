using System;

namespace Baodian.AI.SemanticKernel.Milvus.Configuration
{    public class MilvusOptions
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 9091;
        public string Username { get; set; }
        public string Password { get; set; }
        public string Database { get; set; } = "default";
        public bool UseSSL { get; set; }
        public bool EnableSsl { get; set; } // 兼容性别名
        public int Timeout { get; set; } = 30;
        
        // 用于 Zilliz Cloud 的认证
        public string ApiKey { get; set; }
        public string Token { get; set; }

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