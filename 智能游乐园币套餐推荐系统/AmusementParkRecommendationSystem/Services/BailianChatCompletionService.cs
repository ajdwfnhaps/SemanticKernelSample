using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace AmusementParkRecommendationSystem.Services;

/// <summary>
/// �����ư������켯�ɷ���
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
    /// ������������ӵ�Semantic Kernel
    /// </summary>
    public void AddBailianToKernel(IKernelBuilder kernelBuilder)
    {
        var apiKey = _configuration["Bailian:ApiKey"];
        var modelId = _configuration["Bailian:ModelId"] ?? "qwen-max";
        var endpoint = _configuration["Bailian:Endpoint"] ?? "https://dashscope.aliyuncs.com/compatible-mode/v1";

        if (string.IsNullOrEmpty(apiKey) || apiKey == "your-bailian-api-key-here")
        {
            _logger.LogWarning("����API��Կδ���ã��޷�ʹ�ð�������");
            return;
        }        try
        {
            // ʹ��ͳһ��HttpClient����
            var httpClient = AIHttpClientFactory.CreateConfiguredClient();

            // ʹ�ñ�׼��OpenAI���ݷ�ʽ
            // ������OpenAI����ģʽ���Զ�����Bearer��֤
            kernelBuilder.AddOpenAIChatCompletion(
                modelId: modelId,
                apiKey: apiKey,
                endpoint: new Uri(endpoint),
                httpClient: httpClient);

            _logger.LogInformation("[����] �����ð����ư������� (ģ��: {ModelId}, �˵�: {Endpoint})", modelId, endpoint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "���ð����ư�������ʱ����");
            throw;
        }
    }
}