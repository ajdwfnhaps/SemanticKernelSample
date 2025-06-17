using System;
using System.Threading.Tasks;
using Baodian.AI.SemanticKernel.Milvus.Configuration;

namespace Baodian.AI.SemanticKernel.Milvus.Services
{
    public class RetryService
    {
        private readonly RetryOptions _options;
        private readonly LoggingService _loggingService;

        public RetryService(MilvusOptions options, LoggingService loggingService)
        {
            _options = options.Retry;
            _loggingService = loggingService;
        }

        public async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> action, string operation)
        {
            if (!_options.Enabled)
            {
                return await action();
            }

            var retryCount = 0;
            var delay = _options.RetryInterval;

            while (true)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex)
                {
                    retryCount++;
                    if (retryCount > _options.MaxRetries)
                    {
                        _loggingService.LogError("Retry", operation, ex);
                        throw;
                    }

                    _loggingService.LogError("Retry", $"{operation} (Attempt {retryCount}/{_options.MaxRetries})", ex);
                    
                    if (_options.ExponentialBackoff)
                    {
                        delay *= 2;
                    }

                    await Task.Delay(delay);
                }
            }
        }
    }
} 