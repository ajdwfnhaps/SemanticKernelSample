using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Baodian.AI.SemanticKernel.Local;

/// <summary>
/// 本地模拟聊天服务，不依赖任何外部 API
/// </summary>
public class MockChatCompletionService : IChatCompletionService
{
    public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();

    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        // 模拟处理延迟
        await Task.Delay(100, cancellationToken);
        
        // 获取最后一条用户消息
        var lastMessage = chatHistory.LastOrDefault()?.Content ?? "您好";
        
        // 生成模拟回复
        var response = GenerateMockResponse(lastMessage);
        
        return new List<ChatMessageContent>
        {
            new(AuthorRole.Assistant, response)
        };
    }

    public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var response = GenerateMockResponse(chatHistory.LastOrDefault()?.Content ?? "您好");
        
        // 模拟流式响应
        var words = response.Split(' ');
        foreach (var word in words)
        {
            await Task.Delay(50, cancellationToken);
            yield return new StreamingChatMessageContent(AuthorRole.Assistant, word + " ");
        }
    }

    private string GenerateMockResponse(string userInput)
    {
        // 基于关键词的简单回复逻辑
        var input = userInput.ToLowerInvariant();
        
        if (input.Contains("闸机") || input.Contains("gateway"))
        {
            return "基于您的闸机问题，我建议您检查设备连接状态，确保电源供应正常，并验证网络连接。如需更详细的技术支持，请提供具体的错误信息。";
        }
        
        if (input.Contains("维护") || input.Contains("maintenance"))
        {
            return "设备维护建议：1. 定期清洁设备表面和传感器；2. 检查机械部件润滑情况；3. 验证软件版本并及时更新；4. 测试所有安全功能。";
        }
        
        if (input.Contains("故障") || input.Contains("问题") || input.Contains("error"))
        {
            return "故障排查步骤：1. 记录详细的错误现象；2. 检查系统日志；3. 验证硬件连接；4. 尝试重启设备；5. 如问题持续，请联系技术支持。";
        }
        
        if (input.Contains("配置") || input.Contains("config"))
        {
            return "配置建议：请确保参数设置符合现场环境要求，包括通行速度、安全检测级别、网络参数等。建议备份当前配置后再进行修改。";
        }
        
        return "感谢您的咨询。作为游乐园闸机运营专家，我可以为您提供设备维护、故障排查、配置优化等方面的专业建议。请告诉我您具体遇到的问题。";
    }
}

/// <summary>
/// 本地模拟嵌入生成服务
/// </summary>
public class MockEmbeddingGenerationService : IEmbeddingGenerator<string, Embedding<float>>
{
    public EmbeddingGeneratorMetadata Metadata => new("mock-embedding");

    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(50, cancellationToken);
        
        var embeddings = new List<Embedding<float>>();
        foreach (var value in values)
        {
            var embedding = GenerateMockEmbedding(value);
            embeddings.Add(embedding);
        }
        
        return new GeneratedEmbeddings<Embedding<float>>(embeddings);
    }

    public async Task<Embedding<float>> GenerateAsync(string value, EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
        await Task.Delay(50, cancellationToken);
        return GenerateMockEmbedding(value);
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        if (serviceType == typeof(IEmbeddingGenerator<string, Embedding<float>>))
        {
            return this;
        }
        return null;
    }

    private Embedding<float> GenerateMockEmbedding(string text)
    {
        // 生成一个简单的基于文本哈希的固定维度向量
        var hash = text.GetHashCode();
        var random = new Random(hash);
        var vector = new float[768]; // 常见的嵌入维度
        
        for (int i = 0; i < vector.Length; i++)
        {
            vector[i] = (float)(random.NextDouble() - 0.5) * 2; // -1 到 1 之间
        }
        
        // 归一化向量
        var magnitude = Math.Sqrt(vector.Sum(x => x * x));
        for (int i = 0; i < vector.Length; i++)
        {
            vector[i] = (float)(vector[i] / magnitude);
        }
        
        return new Embedding<float>(vector);
    }

    public void Dispose()
    {
        // 无需清理资源
    }
}
