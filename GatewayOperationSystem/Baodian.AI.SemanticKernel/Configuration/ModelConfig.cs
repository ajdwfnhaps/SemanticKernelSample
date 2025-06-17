using Baodian.AI.SemanticKernel.Abstractions;
using System.Collections.Generic;

namespace Baodian.AI.SemanticKernel.Configuration;

/// <summary>
/// 模型配置基类
/// </summary>
public class ModelConfig : IModelConfig
{
    /// <inheritdoc/>
    public string ModelName { get; set; } = string.Empty;

    /// <inheritdoc/>
    public string ApiKey { get; set; } = string.Empty;

    /// <inheritdoc/>
    public string Endpoint { get; set; } = string.Empty;
    public int? MaxTokens { get; set; } // 可选，因为 DeepSeek 配置中没有
    public double? Temperature { get; set; } // 可选
    public bool? EnableStreaming { get; set; } // 可选
    public string? DeploymentName { get; set; }


} 