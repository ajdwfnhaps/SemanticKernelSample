using Baodian.AI.SemanticKernel.Abstractions;
using Baodian.AI.SemanticKernel.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;

namespace Baodian.AI.SemanticKernel.Providers;

/// <summary>
/// DeepSeek Kernel提供者
/// </summary>
public class DeepSeekProvider : IKernelProvider
{
    private readonly IServiceProvider _serviceProvider;

    public DeepSeekProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Kernel CreateKernel(IModelConfig config)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        var builder = Kernel.CreateBuilder();
        var httpClient = _serviceProvider.GetRequiredService<HttpClient>();

        // 设置 httpClient 的 BaseAddress 为 DeepSeek endpoint
        if (httpClient.BaseAddress == null || httpClient.BaseAddress.ToString() != config.Endpoint)
        {
            httpClient.BaseAddress = new Uri(config.Endpoint);
        }

        // DeepSeek 兼容 OpenAI API
        builder.AddOpenAIChatCompletion(
            modelId: config.ModelName,
            apiKey: config.ApiKey,
            endpoint: new Uri(config.Endpoint),
            httpClient: httpClient
        );

        // 关键：注册 DeepSeek 的 Embedding 服务（使用兼容的重载）
        builder.AddOpenAITextEmbeddingGeneration(
            modelId: config.ModelName,
            apiKey: config.ApiKey,
            httpClient: httpClient
        );

        var kernel = builder.Build();

        RegisterPlugins(kernel);
        return kernel;
    }

    private void RegisterPlugins(Kernel kernel)
    {
        var pluginTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => {
                try
                {
                    return a.GetTypes();
                }
                catch (ReflectionTypeLoadException)
                {
                    // 忽略无法加载类型的程序集
                    return Array.Empty<Type>();
                }
                catch (Exception)
                {
                    // 处理其他可能的异常
                    return Array.Empty<Type>();
                }
            })
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IPlugin).IsAssignableFrom(t));

        foreach (var pluginType in pluginTypes)
        {
            var plugin = ActivatorUtilities.CreateInstance(_serviceProvider, pluginType);
            kernel.Plugins.AddFromObject(plugin, pluginType.Name);
        }
    }
}