# 嵌入服务使用指南

## 问题解决

如果您在控制器中遇到以下错误：
```
System.InvalidOperationException: Unable to resolve service for type 'Microsoft.Extensions.AI.IEmbeddingGenerator`2[System.String,Microsoft.Extensions.AI.Embedding`1[System.Single]]'
```

这是因为没有正确注册嵌入生成器服务。

## 解决方案

### 1. 服务注册

在您的 `Startup.cs` 或 `Program.cs` 中，确保调用 `RegisterEmbeddingServices` 方法：

```csharp
// 配置 SemanticKernel
var semanticKernelOptions = new SemanticKernelOptions
{
    DefaultModel = "gpt-3.5-turbo",
    Models = new List<ModelConfig>
    {
        new OpenAIConfig
        {
            ModelName = "gpt-3.5-turbo",
            ApiKey = "your-api-key",
            // 其他配置...
        }
    }
};

// 注册 SemanticKernel 服务
services.AddSemanticKernel(semanticKernelOptions);

// 注册嵌入服务 - 这是关键步骤！
services.RegisterEmbeddingServices(semanticKernelOptions);
```

### 2. 支持的注入方式

现在您可以使用以下任一方式注入嵌入服务：

#### 方式一：使用自定义接口（推荐）
```csharp
[ApiController]
[Route("api/[controller]")]
public class KnowledgeBaseController : ControllerBase
{
    private readonly IEmbeddingService _embeddingService;

    public KnowledgeBaseController(IEmbeddingService embeddingService)
    {
        _embeddingService = embeddingService;
    }

    [HttpPost("generate-embedding")]
    public async Task<IActionResult> GenerateEmbedding([FromBody] string text)
    {
        var embedding = await _embeddingService.GetEmbeddingAsync("gpt-3.5-turbo", text);
        return Ok(embedding);
    }
}
```

#### 方式二：使用标准 Microsoft.Extensions.AI 接口
```csharp
[ApiController]
[Route("api/[controller]")]
public class KnowledgeBaseController : ControllerBase
{
    private readonly Microsoft.Extensions.AI.IEmbeddingGenerator<string, Microsoft.Extensions.AI.Embedding<float>> _embeddingGenerator;

    public KnowledgeBaseController(
        Microsoft.Extensions.AI.IEmbeddingGenerator<string, Microsoft.Extensions.AI.Embedding<float>> embeddingGenerator)
    {
        _embeddingGenerator = embeddingGenerator;
    }

    [HttpPost("generate-embedding")]
    public async Task<IActionResult> GenerateEmbedding([FromBody] string text)
    {
        var result = await _embeddingGenerator.GenerateAsync(new[] { text });
        var embedding = result.FirstOrDefault()?.Vector.ToArray() ?? Array.Empty<float>();
        return Ok(embedding);
    }
}
```

### 3. 服务特性

- **自动适配**：服务会自动检测可用的嵌入接口，优先使用新版本的 `IEmbeddingGenerator`，降级到旧版本的 `ITextEmbeddingGenerationService`
- **多模型支持**：支持通过模型名称指定不同的嵌入模型
- **错误处理**：包含详细的异常信息，便于调试
- **向前兼容**：同时支持新旧版本的 Microsoft.SemanticKernel

### 4. 配置示例

```csharp
var semanticKernelOptions = new SemanticKernelOptions
{
    DefaultModel = "text-embedding-3-small",
    Models = new List<ModelConfig>
    {
        new OpenAIConfig
        {
            ModelName = "text-embedding-3-small",
            ApiKey = Configuration["OpenAI:ApiKey"],
            MaxTokens = 8192
        },
        new AzureOpenAIConfig
        {
            ModelName = "text-embedding-ada-002",
            ApiKey = Configuration["AzureOpenAI:ApiKey"],
            Endpoint = Configuration["AzureOpenAI:Endpoint"],
            DeploymentName = "text-embedding-ada-002"
        }
    }
};

services.AddSemanticKernel(semanticKernelOptions);
services.RegisterEmbeddingServices(semanticKernelOptions);
```

## 注意事项

1. **确保模型配置正确**：在 `SemanticKernelOptions.Models` 中至少配置一个模型
2. **API 密钥**：确保相应的 API 密钥已正确配置
3. **依赖注入顺序**：`RegisterEmbeddingServices` 必须在 `AddSemanticKernel` 之后调用
4. **版本兼容性**：当前实现支持 Microsoft.SemanticKernel 1.x 版本

## 故障排除

如果仍然遇到问题，请检查：

1. 是否调用了 `services.RegisterEmbeddingServices(semanticKernelOptions)`
2. `SemanticKernelOptions.Models` 是否包含有效的模型配置
3. 相关的 API 密钥是否正确配置
4. 日志中是否有相关的异常信息

## 更多示例

查看项目中的 `Samples/` 目录获取更多使用示例。
