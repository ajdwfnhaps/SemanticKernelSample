namespace Baodian.AI.SemanticKernel.Configuration;

/// <summary>
/// OpenAI配置
/// </summary>
public class OpenAIConfig : ModelConfig
{
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