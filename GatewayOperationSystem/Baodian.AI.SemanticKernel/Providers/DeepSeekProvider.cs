

using Baodian.AI.SemanticKernel.Abstractions;
using Baodian.AI.SemanticKernel.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System;
using System.Linq;
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

        // DeepSeek 兼容 OpenAI API
        builder.AddOpenAIChatCompletion(
            modelId: config.ModelName,
            apiKey: config.ApiKey,
            endpoint: new Uri(config.Endpoint),
            httpClient: httpClient
        );

        //// 添加嵌入生成服务
        //var pineconeOptions = _serviceProvider.GetRequiredService<IConfiguration>().GetSection("Pinecone").Get<PineconeOptions>() ?? new PineconeOptions();
        //// 以 OpenAI 为例，实际参数请用你的配置
        //builder.AddOpenAIEmbeddingGenerator(
        //    modelId: pineconeOptions.ModelName,
        //    apiKey: pineconeOptions.ApiKey
        //);

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