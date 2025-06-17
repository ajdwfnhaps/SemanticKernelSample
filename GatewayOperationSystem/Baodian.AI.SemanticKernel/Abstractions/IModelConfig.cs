using System.Collections.Generic;

namespace Baodian.AI.SemanticKernel.Abstractions;

/// <summary>
/// 模型配置接口
/// </summary>
public interface IModelConfig
{
    /// <summary>
    /// 模型名称
    /// </summary>
    string ModelName { get; set; }

    /// <summary>
    /// API密钥
    /// </summary>
    string ApiKey { get; set; }

    /// <summary>
    /// 端点地址
    /// </summary>
    string Endpoint { get; set; }

    public int? MaxTokens { get; set; } // 可选，因为 DeepSeek 配置中没有
    public double? Temperature { get; set; } // 可选
    public bool? EnableStreaming { get; set; } // 可选
    string? DeploymentName { get; set; }
}