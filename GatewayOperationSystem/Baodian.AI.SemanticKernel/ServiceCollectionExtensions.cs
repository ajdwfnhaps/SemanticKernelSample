#pragma warning disable SKEXP0010

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Baodian.AI.SemanticKernel.Local;
using Baodian.AI.SemanticKernel.Milvus.Services;
using Baodian.AI.SemanticKernel.Milvus.Configuration;
using System;
using System.Net.Http;
using System.Linq;

namespace Baodian.AI.SemanticKernel;

public class SemanticKernelConfig
{
    public string DefaultModel { get; set; } = string.Empty;
    public string Provider { get; set; } = "Mock"; // Mock, OpenAI-Compatible, OpenAI, AzureOpenAI
    public List<ModelConfig> Models { get; set; } = new();
}

public class ModelConfig
{
    public string Provider { get; set; } = "Mock";
    public string ModelName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = ""; // 本地模拟服务不需要密钥
    public string Endpoint { get; set; } = ""; // 本地模拟服务不需要端点
    public int MaxTokens { get; set; } = 2000;
    public double Temperature { get; set; } = 0.7;    public bool EnableStreaming { get; set; } = false;
}

/// <summary>
/// 服务集合扩展
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加Semantic Kernel服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddBaodianSemanticKernel(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var skConfig = configuration.GetSection("SemanticKernel").Get<SemanticKernelConfig>();

        if (skConfig == null || !skConfig.Models.Any())
        {
            throw new InvalidOperationException("Semantic Kernel configuration is missing or invalid.");
        }        services.AddScoped<Kernel>(sp =>
        {
            var kernelBuilder = Kernel.CreateBuilder();

            foreach (var modelConfig in skConfig.Models)
            {
                ConfigureProvider(kernelBuilder, modelConfig);
            }

            var kernel = kernelBuilder.Build();
            return kernel;
        });

        // 注册嵌入生成服务 - 使用独立的注册方式
        RegisterEmbeddingServices(services, skConfig);

        return services;
    }    /// <summary>
    /// 注册嵌入生成服务
    /// </summary>
    private static void RegisterEmbeddingServices(IServiceCollection services, SemanticKernelConfig skConfig)
    {
        var embeddingModel = skConfig.Models.FirstOrDefault();
        if (embeddingModel == null) 
        {
            // 如果没有配置模型，使用默认的 Mock 服务
            services.AddScoped<IEmbeddingGenerator<string, Embedding<float>>, MockEmbeddingGenerationService>();
            return;
        }

        switch (embeddingModel.Provider.ToLowerInvariant())
        {
            case "mock":
                services.AddScoped<IEmbeddingGenerator<string, Embedding<float>>, MockEmbeddingGenerationService>();
                break;
            case "openai-compatible":
            case "openai":
            case "azureopenai":
                // 暂时都使用 Mock 服务，直到我们有正确的 OpenAI 包配置
                services.AddScoped<IEmbeddingGenerator<string, Embedding<float>>, MockEmbeddingGenerationService>();
                break;
            default:
                // 默认使用 Mock 服务
                services.AddScoped<IEmbeddingGenerator<string, Embedding<float>>, MockEmbeddingGenerationService>();
                break;
        }
    }

    private static void ConfigureProvider(IKernelBuilder kernelBuilder, ModelConfig modelConfig)
    {
        switch (modelConfig.Provider.ToLowerInvariant())
        {
            case "mock":
                ConfigureMock(kernelBuilder, modelConfig);
                break;
            case "openai-compatible":
                ConfigureOpenAICompatible(kernelBuilder, modelConfig);
                break;
            case "openai":
                ConfigureOpenAI(kernelBuilder, modelConfig);
                break;
            case "azureopenai":
                ConfigureAzureOpenAI(kernelBuilder, modelConfig);
                break;
            default:
                throw new NotSupportedException($"Provider '{modelConfig.Provider}' is not supported.");
        }
    }
    private static void ConfigureMock(IKernelBuilder kernelBuilder, ModelConfig modelConfig)
    {
        // 注册本地模拟服务，完全不依赖外部 API
        kernelBuilder.Services.AddSingleton<MockChatCompletionService>();
        kernelBuilder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>, MockEmbeddingGenerationService>();

        // 将 Mock 服务添加到 Kernel 构建器
        kernelBuilder.Services.AddKeyedSingleton<IChatCompletionService, MockChatCompletionService>(modelConfig.ModelName);
    }

    private static void ConfigureOpenAICompatible(IKernelBuilder kernelBuilder, ModelConfig modelConfig)
    {
        // 用于兼容 OpenAI API 的免费或本地服务
        var httpClient = new HttpClient();
        if (!string.IsNullOrEmpty(modelConfig.Endpoint))
        {
            httpClient.BaseAddress = new Uri(modelConfig.Endpoint);
        }

        // 使用假的 API Key 对于不需要认证的服务
        var apiKey = string.IsNullOrEmpty(modelConfig.ApiKey) ? "dummy-key" : modelConfig.ApiKey;

        kernelBuilder.AddOpenAIChatCompletion(
            modelId: modelConfig.ModelName,
            apiKey: apiKey,
            httpClient: httpClient);

        kernelBuilder.AddOpenAIEmbeddingGenerator(
            modelId: modelConfig.ModelName,
            apiKey: apiKey,
            httpClient: httpClient);
    }

    private static void ConfigureOpenAI(IKernelBuilder kernelBuilder, ModelConfig modelConfig)
    {
        var httpClient = new HttpClient();
        if (!string.IsNullOrEmpty(modelConfig.Endpoint))
        {
            httpClient.BaseAddress = new Uri(modelConfig.Endpoint);
        }

        kernelBuilder.AddOpenAIChatCompletion(
            modelId: modelConfig.ModelName,
            apiKey: modelConfig.ApiKey,
            httpClient: httpClient);

        kernelBuilder.AddOpenAIEmbeddingGenerator(
            modelId: modelConfig.ModelName,
            apiKey: modelConfig.ApiKey,
            httpClient: httpClient);
    }

    private static void ConfigureAzureOpenAI(IKernelBuilder kernelBuilder, ModelConfig modelConfig)
    {
        if (string.IsNullOrEmpty(modelConfig.Endpoint) || string.IsNullOrEmpty(modelConfig.ApiKey))
        {
            throw new InvalidOperationException("Azure OpenAI requires both Endpoint and ApiKey to be configured.");
        }

        kernelBuilder.AddAzureOpenAIChatCompletion(
            deploymentName: modelConfig.ModelName,
            endpoint: modelConfig.Endpoint,
            apiKey: modelConfig.ApiKey); kernelBuilder.AddAzureOpenAIEmbeddingGenerator(
            deploymentName: modelConfig.ModelName,
            endpoint: modelConfig.Endpoint,
            apiKey: modelConfig.ApiKey);
    }

    /// <summary>
    /// 添加 Milvus 向量数据库服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configure">配置委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddMilvus(this IServiceCollection services, Action<MilvusOptions> configure)
    {
        var options = new MilvusOptions();
        configure(options);
        return services.AddMilvus(options);
    }

    /// <summary>
    /// 添加 Milvus 向量数据库服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="options">Milvus 配置选项</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddMilvus(this IServiceCollection services, MilvusOptions options)
    {
        // 注册 Milvus 服务
        services.AddScoped<DataService>(provider => 
            new DataService(options.Host, options.Port));
        
        services.AddScoped<CollectionService>(provider => 
            new CollectionService(options.Host, options.Port));
        
        services.AddScoped<SearchService>(provider => 
            new SearchService(options.Host, options.Port));

        return services;
    }
}