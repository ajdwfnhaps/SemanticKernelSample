using Baodian.AI.SemanticKernel.Abstractions;
using Baodian.AI.SemanticKernel.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Baodian.AI.SemanticKernel.Factory;

/// <summary>
/// Kernel工厂实现
/// </summary>
public class KernelFactory : IKernelFactory
{
    private readonly ConcurrentDictionary<string, IKernelProvider> _providers;
    private readonly IModelConfigFactory _configFactory;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="configFactory">配置工厂</param>
    /// <param name="serviceProvider">服务提供者</param>
    public KernelFactory(
        IModelConfigFactory configFactory,
        IServiceProvider serviceProvider)
    {
        _providers = new ConcurrentDictionary<string, IKernelProvider>();
        _configFactory = configFactory;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public Kernel CreateKernel(string modelName)
    {
        if (!_providers.TryGetValue(modelName, out var provider))
        {
            throw new KeyNotFoundException($"未找到模型 {modelName} 的提供者");
        }

        var config = _configFactory.CreateConfig(modelName);
        var kernel = provider.CreateKernel(config);
        // 自动注册全局Filter
        if (kernel is not null)
        {
            var filter = _serviceProvider.GetService<SemanticKernelHookFilter>();
            if (filter is not null)
            {
                kernel.FunctionInvocationFilters.Add(filter);
            }
        }
        return kernel;
    }

    /// <inheritdoc/>
    public void RegisterProvider(string modelName, IKernelProvider provider)
    {
        _providers[modelName] = provider;
    }
} 