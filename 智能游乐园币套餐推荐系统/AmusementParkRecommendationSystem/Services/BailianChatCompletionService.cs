using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace AmusementParkRecommendationSystem.Services;

/// <summary>
/// 阿里云百炼聊天集成服务
/// </summary>
public class BailianChatService
{
    private readonly ILogger<BailianChatService> _logger;
    private readonly IConfiguration _configuration;

    public BailianChatService(
        IConfiguration configuration,
        ILogger<BailianChatService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }    /// <summary>
    /// 将百炼服务添加到Semantic Kernel
    /// </summary>
    public void AddBailianToKernel(IKernelBuilder kernelBuilder)
    {
        var apiKey = _configuration["Bailian:ApiKey"];
        var modelId = _configuration["Bailian:ModelId"] ?? "qwen-max";
        var endpoint = _configuration["Bailian:Endpoint"] ?? "https://dashscope.aliyuncs.com/compatible-mode/v1";

        if (string.IsNullOrEmpty(apiKey) || apiKey == "your-bailian-api-key-here")
        {
            _logger.LogWarning("百炼API密钥未配置，无法使用百炼服务");
            return;
        }        try
        {
            // 使用统一的HttpClient工厂
            var httpClient = AIHttpClientFactory.CreateConfiguredClient();

            // 使用标准的OpenAI兼容方式
            // 百炼的OpenAI兼容模式会自动处理Bearer认证
            kernelBuilder.AddOpenAIChatCompletion(
                modelId: modelId,
                apiKey: apiKey,
                endpoint: new Uri(endpoint),
                httpClient: httpClient);

            _logger.LogInformation("[就绪] 已配置阿里云百炼服务 (模型: {ModelId}, 端点: {Endpoint})", modelId, endpoint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "配置阿里云百炼服务时出错");
            throw;
        }
    }
}