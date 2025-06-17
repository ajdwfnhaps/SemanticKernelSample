namespace Baodian.AI.SemanticKernel.Configuration;

/// <summary>
/// Azure OpenAI配置
/// </summary>
public class AzureOpenAIConfig : ModelConfig
{
    /// <summary>
    /// 部署名称
    /// </summary>
    public string DeploymentName { get; set; } = string.Empty;

    /// <summary>
    /// API版本
    /// </summary>
    public string ApiVersion { get; set; } = "2024-02-15-preview";

    /// <summary>
    /// 最大Token数
    /// </summary>
    public int MaxTokens { get; set; } = 2000;

    /// <summary>
    /// 温度参数
    /// </summary>
    public double Temperature { get; set; } = 0.7;

    /// <summary>
    /// 是否启用流式响应
    /// </summary>
    public bool EnableStreaming { get; set; } = false;
} 