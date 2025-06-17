using Baodian.AI.SemanticKernel.Abstractions;
using System.Collections.Generic;

namespace Baodian.AI.SemanticKernel.Configuration;

/// <summary>
/// Semantic Kernel配置选项
/// </summary>
public class SemanticKernelOptions
{
    /// <summary>
    /// 默认模型名称
    /// </summary>
    public string DefaultModel { get; set; } = string.Empty;

    /// <summary>
    /// 模型配置列表
    /// </summary>
    public List<ModelConfig> Models { get; set; } = new();
}


public class PineconeOptions
{
    public string ApiKey { get; set; }
    public string IndexName { get; set; }
    public string Environment { get; set; }

    public string ModelName { get; set; } = "text-embedding-3-small"; // 默认模型名称
}
