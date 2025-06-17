using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Baodian.AI.SemanticKernel.Milvus.Configuration;
using LogLevel = Baodian.AI.SemanticKernel.Milvus.Configuration.LogLevel;

namespace Baodian.AI.SemanticKernel.Milvus.Services
{
    public class LoggingService
    {
        private readonly ILogger<LoggingService> _logger;
        private readonly LoggingOptions _options;

        public LoggingService(ILogger<LoggingService> logger, MilvusOptions options)
        {
            _logger = logger;
            _options = options.Logging;
        }

        public void LogRequest(string method, string endpoint, object content = null)
        {
            if (!_options.Enabled) return;

            var message = new StringBuilder();
            message.AppendLine($"Milvus Request: {method} {endpoint}");
            
            if (_options.LogRequestContent && content != null)
            {
                message.AppendLine($"Request Content: {content}");
            }

            Log(LogLevel.Information, message.ToString());
        }

        public void LogResponse(string method, string endpoint, object content = null)
        {
            if (!_options.Enabled) return;

            var message = new StringBuilder();
            message.AppendLine($"Milvus Response: {method} {endpoint}");
            
            if (_options.LogResponseContent && content != null)
            {
                message.AppendLine($"Response Content: {content}");
            }

            Log(LogLevel.Information, message.ToString());
        }

        public void LogError(string method, string endpoint, Exception ex)
        {
            if (!_options.Enabled) return;

            var message = new StringBuilder();
            message.AppendLine($"Milvus Error: {method} {endpoint}");
            message.AppendLine($"Error Message: {ex.Message}");
            message.AppendLine($"Stack Trace: {ex.StackTrace}");

            Log(LogLevel.Error, message.ToString());
        }

        private void Log(LogLevel level, string message)
        {
            if (level < _options.MinimumLevel) return;

            switch (level)
            {
                case LogLevel.Debug:
                    _logger.LogDebug(message);
                    break;
                case LogLevel.Information:
                    _logger.LogInformation(message);
                    break;
                case LogLevel.Warning:
                    _logger.LogWarning(message);
                    break;
                case LogLevel.Error:
                    _logger.LogError(message);
                    break;
            }
        }
    }
} 