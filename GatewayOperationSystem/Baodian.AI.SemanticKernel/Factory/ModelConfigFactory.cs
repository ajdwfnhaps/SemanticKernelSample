using Baodian.AI.SemanticKernel.Abstractions;
using Baodian.AI.SemanticKernel.Configuration;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Baodian.AI.SemanticKernel.Factory;

/// <summary>
/// 模型配置工厂实现
/// </summary>
public class ModelConfigFactory : IModelConfigFactory
{
    private readonly ConcurrentDictionary<string, ModelConfig> _configs;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options">配置选项</param>
    public ModelConfigFactory(SemanticKernelOptions options)
    {
        _configs = new ConcurrentDictionary<string, ModelConfig>();
        
        // 注册默认配置
        foreach (var config in options.Models)
        {
            _configs.TryAdd(config.ModelName, config);
        }
    }

    /// <inheritdoc/>
    public IModelConfig CreateConfig(string modelName)
    {
        if (_configs.TryGetValue(modelName, out var config))
        {
            return config;
        }

        throw new KeyNotFoundException($"未找到模型配置: {modelName}");
    }
} 