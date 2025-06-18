#pragma warning disable SKEXP0010
using Baodian.AI.SemanticKernel.Abstractions;
using Baodian.AI.SemanticKernel.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace Baodian.AI.SemanticKernel.Providers;

/// <summary>
/// Azure OpenAI Kernel提供者
/// </summary>
public class AzureOpenAIKernelProvider : IKernelProvider
{
    private readonly IServiceProvider _serviceProvider;

    public AzureOpenAIKernelProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public Kernel CreateKernel(IModelConfig config)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        var builder = Kernel.CreateBuilder();
        var httpClient = _serviceProvider.GetRequiredService<HttpClient>();

        // 添加文本生成服务
        builder.AddAzureOpenAIChatCompletion(
            deploymentName: config.DeploymentName,
            endpoint: config.Endpoint,
            apiKey: config.ApiKey,
            httpClient: httpClient
        );

        //// 关键：注册 Azure OpenAI Embedding 服务
        //builder.AddAzureOpenAITextEmbeddingGeneration(
        //    deploymentName: $"{config.DeploymentName}-embedding",
        //    endpoint: config.Endpoint,
        //    apiKey: config.ApiKey,
        //    httpClient: httpClient
        //);

        // 添加嵌入生成服务
        var pineconeOptions = _serviceProvider.GetRequiredService<IConfiguration>().GetSection("Pinecone").Get<PineconeOptions>() ?? new PineconeOptions();
        // 以 OpenAI 为例，实际参数请用你的配置
        builder.AddOpenAIEmbeddingGenerator(
            modelId: pineconeOptions.ModelName,
            apiKey: pineconeOptions.ApiKey
        );

        // 添加文本嵌入服务
#pragma warning disable SKEXP0011
        builder.AddAzureOpenAITextEmbeddingGeneration(
            deploymentName: $"{config.DeploymentName}-embedding",
            endpoint: config.Endpoint,
            apiKey: config.ApiKey);
#pragma warning restore SKEXP0011

        var kernel = builder.Build();
        RegisterPlugins(kernel);
        return kernel;
    }

    //private void RegisterPlugins(Kernel kernel)
    //{
    //    var pluginTypes = AppDomain.CurrentDomain.GetAssemblies()
    //        .SelectMany(a => a.GetTypes())
    //        .Where(t => t.IsClass && !t.IsAbstract && typeof(IPlugin).IsAssignableFrom(t));

    //    foreach (var pluginType in pluginTypes)
    //    {
    //        var plugin = ActivatorUtilities.CreateInstance(_serviceProvider, pluginType);
    //        kernel.Plugins.AddFromObject(plugin, pluginType.Name);
    //    }
    //}


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