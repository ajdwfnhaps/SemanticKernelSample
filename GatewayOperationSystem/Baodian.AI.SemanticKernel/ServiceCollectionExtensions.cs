using Autofac.Core;
using Baodian.AI.SemanticKernel.Abstractions;
using Baodian.AI.SemanticKernel.Configuration;
using Baodian.AI.SemanticKernel.Constants;
using Baodian.AI.SemanticKernel.Factory;
using Baodian.AI.SemanticKernel.Filters;
using Baodian.AI.SemanticKernel.Providers;
using Baodian.AI.SemanticKernel.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using System;
using System.Configuration;
using System.Net.Http;
using System.Reflection;

namespace Baodian.AI.SemanticKernel;


public class SemanticKernelConfig
{
    public string DefaultModel { get; set; } = string.Empty;
    public string Provider { get; set; } = "Mock"; // Mock, OpenAI-Compatible, OpenAI, AzureOpenAI
    public List<ModelConfig> Models { get; set; } = new();
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
        // 手动绑定配置并注册为单例
        var options = new SemanticKernelOptions();
        configuration.GetSection("SemanticKernel").Bind(options);

        services.AddSingleton(options);


        // 注册服务
        services.AddSingleton<IModelConfigFactory, ModelConfigFactory>(p =>
        {
            return new ModelConfigFactory(options);
        });
        services.AddSingleton<IKernelFactory, KernelFactory>();
        services.AddSingleton<SemanticKernelHookFilter>();
        services.AddScoped<ChatService>();

        //注册Providers
        services.AddKernelProviders(typeof(AliyunBailianProvider).Assembly, options);

        //// 添加嵌入生成服务 (使用新的API)
        //var httpClient = services.BuildServiceProvider().GetRequiredService<HttpClient>();



        return services;
    }






    /// <summary>
    /// 自动注册程序集中所有实现了IKernelProvider接口的类
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="assembly">包含Provider的程序集，默认为当前调用程序集</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddKernelProviders(this IServiceCollection services, Assembly? assembly = null, SemanticKernelOptions options = null)
    {
        assembly ??= Assembly.GetCallingAssembly();

        var providerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && typeof(IKernelProvider).IsAssignableFrom(t));

        foreach (var providerType in providerTypes)
        {
            services.AddSingleton(providerType);
            //services.AddSingleton(typeof(IKernelProvider), providerType);
        }

        //var pineconeOptions = services.BuildServiceProvider().GetRequiredService<IConfiguration>().GetSection("Pinecone").Get<PineconeOptions>() ?? new PineconeOptions();
        var models = options?.Models ?? null;
        if (models != null)
        {
            // 注册嵌入生成服务 - 提前注册，而不是在 AddScoped 的委托中调用
            foreach (var model in models)
            {
                // 注册嵌入生成服务 - 使用独立的注册方式
                services.RegisterEmbeddingServices(model.ModelName);
            }

            foreach (var model in models)
            {
                services.AddScoped<Kernel>(p =>
                {
                    var kernelFactory = p.GetRequiredService<IKernelFactory>();
                    return kernelFactory.CreateKernel(model.ModelName);
                });
            }
        }


        return services;
    }

    /// <summary>
    /// 注册插件
    /// </summary>
    /// <typeparam name="TPlugin">插件类型</typeparam>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddPlugin<TPlugin>(this IServiceCollection services)
        where TPlugin : class, IPlugin
    {
        services.AddSingleton<IPlugin, TPlugin>();
        return services;
    }

    /// <summary>
    /// 注册AI模型提供者
    /// </summary>
    /// <typeparam name="TProvider">提供者类型</typeparam>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddModelProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, IKernelProvider
    {
        services.AddSingleton<IKernelProvider, TProvider>();
        return services;
    }

    /// <summary>
    /// 配置并使用宝典AI服务
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    /// <param name="modelInfos">模型信息列表</param>
    /// <returns>应用程序构建器</returns>
    public static IApplicationBuilder UseBaodianAISemanticKernel(this IApplicationBuilder app, params ModelInfo[] modelInfos)
    {
        var kernelFactory = app.ApplicationServices.GetRequiredService<IKernelFactory>();

        // 注册每个模型
        foreach (var modelInfo in modelInfos.Where(m => m.KernelProvider != null && !string.IsNullOrEmpty(m.ModelName)))
        {
            kernelFactory.RegisterProvider(modelInfo.ModelName, modelInfo.KernelProvider);
        }

        return app;
    }

    #region 添加聊天完成模型配置
    /// <summary>
    /// 添加OpenAI聊天完成模型配置
    /// </summary>
    /// <param name="options">Semantic Kernel配置选项</param>
    /// <param name="configure">配置委托</param>
    /// <returns>Semantic Kernel配置选项</returns>
    public static SemanticKernelOptions UseOpenAIChatCompletion(
        this SemanticKernelOptions options,
        Action<OpenAIConfig> configure)
    {
        var config = new OpenAIConfig();
        configure(config);
        options.Models.Add(config);
        return options;
    }

    /// <summary>
    /// 添加Azure OpenAI聊天完成模型配置
    /// </summary>
    /// <param name="options">Semantic Kernel配置选项</param>
    /// <param name="configure">配置委托</param>
    /// <returns>Semantic Kernel配置选项</returns>
    public static SemanticKernelOptions UseAzureOpenAIChatCompletion(
        this SemanticKernelOptions options,
        Action<AzureOpenAIConfig> configure)
    {
        var config = new AzureOpenAIConfig();
        configure(config);
        options.Models.Add(config);
        return options;
    }

    /// <summary>
    /// 添加DeepSeek聊天完成模型配置
    /// </summary>
    /// <param name="options">Semantic Kernel配置选项</param>
    /// <param name="configure">配置委托</param>
    /// <returns>Semantic Kernel配置选项</returns>
    public static SemanticKernelOptions UseDeepSeekChatCompletion(
        this SemanticKernelOptions options,
        Action<OpenAIConfig> configure) // DeepSeek Provider 也使用 OpenAIConfig
    {
        var config = new OpenAIConfig();
        configure(config);
        options.Models.Add(config);
        return options;
    }

    /// <summary>
    /// 添加阿里云百炼聊天完成模型配置
    /// </summary>
    /// <param name="options">Semantic Kernel配置选项</param>
    /// <param name="configure">配置委托</param>
    /// <returns>Semantic Kernel配置选项</returns>
    public static SemanticKernelOptions UseAliyunBailianChatCompletion(
        this SemanticKernelOptions options,
        Action<OpenAIConfig> configure) // 阿里云百炼 Provider 也使用 OpenAIConfig
    {
        var config = new OpenAIConfig();
        configure(config);
        options.Models.Add(config);
        return options;
    }
    #endregion


    #region Use模型方法
    /// <summary>
    /// 使用宝典AI的DeepSeek模型
    /// </summary>
    /// <param name="app"></param>
    /// <param name="modelName"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseBaodianAIDeepSeek(this IApplicationBuilder app, string modelName = "")
    {
        var kernelFactory = app.ApplicationServices.GetRequiredService<IKernelFactory>();

        if (string.IsNullOrEmpty(modelName))
        {
            modelName = ModelConstants.DeepSeekChat;
        }

        kernelFactory.RegisterProvider(modelName, app.ApplicationServices.GetRequiredService<DeepSeekProvider>());

        return app;
    }

    /// <summary>
    /// 使用宝典AI的OpenAI模型
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    /// <param name="modelName">模型名称，如果为空则使用默认值</param>
    /// <returns>应用程序构建器</returns>
    public static IApplicationBuilder UseBaodianAIOpenAI(this IApplicationBuilder app, string modelName = "")
    {
        var kernelFactory = app.ApplicationServices.GetRequiredService<IKernelFactory>();

        if (string.IsNullOrEmpty(modelName))
        {
            // 假设这里的默认模型名称是 "gpt-3.5-turbo" 或您在 ModelConstants 中定义的常量
            modelName = ModelConstants.Gpt35Turbo;
        }

        kernelFactory.RegisterProvider(modelName, app.ApplicationServices.GetRequiredService<OpenAIKernelProvider>());

        return app;
    }

    /// <summary>
    /// 使用宝典AI的Azure OpenAI模型
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    /// <param name="modelName">模型名称 (通常是部署名称)，如果为空则使用默认值</param>
    /// <returns>应用程序构建器</returns>
    public static IApplicationBuilder UseBaodianAIAzureOpenAI(this IApplicationBuilder app, string modelName = "")
    {
        var kernelFactory = app.ApplicationServices.GetRequiredService<IKernelFactory>();

        if (string.IsNullOrEmpty(modelName))
        {
            // 假设这里的默认模型名称是 "azure-gpt-35-turbo-deployment" 或您在 ModelConstants 中定义的常量
            modelName = ModelConstants.AzureGpt35TurboDeployment;
        }

        kernelFactory.RegisterProvider(modelName, app.ApplicationServices.GetRequiredService<AzureOpenAIKernelProvider>());

        return app;
    }

    /// <summary>
    /// 使用宝典AI的阿里云百炼模型
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    /// <param name="modelName">模型名称，如果为空则使用默认值</param>
    /// <returns>应用程序构建器</returns>
    public static IApplicationBuilder UseBaodianAIAliyunBailian(this IApplicationBuilder app, string modelName = "")
    {
        var kernelFactory = app.ApplicationServices.GetRequiredService<IKernelFactory>();

        if (string.IsNullOrEmpty(modelName))
        {
            // 假设这里的默认模型名称是 "qwen-max" 或您在 ModelConstants 中定义的常量
            modelName = ModelConstants.AliQwenMax;
        }

        kernelFactory.RegisterProvider(modelName, app.ApplicationServices.GetRequiredService<AliyunBailianProvider>());

        return app;
    }    /// <summary>    /// <summary>
         /// 注册嵌入生成服务（Embedding Services）
         /// </summary>
         /// <param name="services">服务集合</param>
         /// <param name="options">SemanticKernel 配置</param>
         /// <returns>服务集合</returns>
    public static IServiceCollection RegisterEmbeddingServices(this IServiceCollection services, string modelName)
    {
        // 移除本地IEmbeddingService接口和EmbeddingService实现相关注册

        // 注册 Microsoft.Extensions.AI.IEmbeddingGenerator 接口
        // 使用默认模型或第一个可用模型的嵌入生成器
        services.AddSingleton<Microsoft.Extensions.AI.IEmbeddingGenerator<string, Microsoft.Extensions.AI.Embedding<float>>>(serviceProvider =>
        {
            var kernelFactory = serviceProvider.GetRequiredService<IKernelFactory>();
            var kernel = kernelFactory.CreateKernel(modelName);

            // 优先获取新接口，否则用适配器
            var embeddingGenerator = kernel.Services.GetService<Microsoft.Extensions.AI.IEmbeddingGenerator<string, Microsoft.Extensions.AI.Embedding<float>>>();
            if (embeddingGenerator != null)
            {
                return embeddingGenerator;
            }

            // 适配阿里云 DashScope 的 ITextEmbeddingGenerationService
            return new EmbeddingGeneratorAdapter(kernel);
        });

        return services;
    }/// <summary>
     /// 嵌入生成器适配器，用于将旧版本的 ITextEmbeddingGenerationService 适配为新版本的 IEmbeddingGenerator
     /// </summary>
    internal class EmbeddingGeneratorAdapter : Microsoft.Extensions.AI.IEmbeddingGenerator<string, Microsoft.Extensions.AI.Embedding<float>>
    {
        private readonly Kernel _kernel;

        public EmbeddingGeneratorAdapter(Kernel kernel)
        {
            _kernel = kernel;
        }

        public async Task<Microsoft.Extensions.AI.GeneratedEmbeddings<Microsoft.Extensions.AI.Embedding<float>>> GenerateAsync(
            IEnumerable<string> values,
            Microsoft.Extensions.AI.EmbeddingGenerationOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
#pragma warning disable CS0618 // Type or member is obsolete
                var embeddingService = _kernel.Services.GetService<ITextEmbeddingGenerationService>();
                if (embeddingService != null)
                {
                    var valuesList = values.ToList();
                    var embeddings = await embeddingService.GenerateEmbeddingsAsync(valuesList, cancellationToken: cancellationToken);
                    var results = embeddings.Select(e =>
                        new Microsoft.Extensions.AI.Embedding<float>(e.IsEmpty ? Array.Empty<float>() : e.ToArray())
                    ).ToList();

                    return new Microsoft.Extensions.AI.GeneratedEmbeddings<Microsoft.Extensions.AI.Embedding<float>>(results);
                }
#pragma warning restore CS0618 // Type or member is obsolete

                throw new InvalidOperationException("No embedding service found in kernel");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to generate embeddings: {ex.Message}", ex);
            }
        }

        public void Dispose()
        {
            // Nothing to dispose
        }

        public Microsoft.Extensions.AI.EmbeddingGeneratorMetadata Metadata =>
            new("SemanticKernel-Adapter", null, null); public TService? GetService<TService>(object? serviceKey = null) where TService : class
        {
            return _kernel.Services.GetService<TService>();
        }

        public object? GetService(Type serviceType, object? serviceKey = null)
        {
            return _kernel.Services.GetService(serviceType);
        }
    }
    #endregion

    /// <summary>
    /// 添加阿里云Text Embedding服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="apiKey">阿里云API Key</param>
    /// <param name="endpoint">阿里云text-embedding-v1接口地址</param>
    /// <returns></returns>
    public static IServiceCollection AddAliyunTextEmbeddingService(this IServiceCollection services, string apiKey, string endpoint, string model = "text-embedding-v4", int dimension = 768)
    {
        services.AddSingleton<IEmbeddingService>(sp => new AliyunTextEmbeddingService(apiKey, endpoint, model, dimension));
        return services;
    }
}