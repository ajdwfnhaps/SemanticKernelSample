using Baodian.AI.SemanticKernel.Milvus.Services;
using GatewayOperationSystem.Core.Models;
using GatewayOperationSystem.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;

namespace GatewayOperationSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GatewayAIController : ControllerBase
{
    private readonly Kernel _kernel;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingService;
    private readonly ILogger<GatewayAIController> _logger;
    private readonly DataService _dataService;
    private readonly CollectionService _collectionService;
    private readonly SearchService _searchService;

    public GatewayAIController(
        Kernel kernel,
        DataService dataService,
        CollectionService collectionService,
        SearchService searchService,
        IEmbeddingGenerator<string, Embedding<float>> embeddingService,
        ILogger<GatewayAIController> logger)
    {
        _kernel = kernel;
        _dataService = dataService;
        _collectionService = collectionService;
        _searchService = searchService;
        _embeddingService = embeddingService;
        _logger = logger;
    }

    /// <summary>
    /// 智能问答接口
    /// </summary>
    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Question))
                return BadRequest("问题不能为空");

            // 生成嵌入向量用于搜索相关知识
            var embeddings = await _embeddingService.GenerateAsync([request.Question]);
            var queryEmbedding = embeddings.First().Vector.ToArray();            // 搜索相关知识库内容
            var relatedKnowledge = await _searchService.SearchAsync("DB_Gate_Knowledge", queryEmbedding, 3);

            // 构建上下文 - 从 SearchResponse.Data 中提取数据
            var contextList = new List<string>();
            if (relatedKnowledge?.Data != null)
            {
                foreach (var result in relatedKnowledge.Data)
                {
                    if (result.Hits != null)
                    {
                        foreach (var hit in result.Hits)
                        {
                            var fields = hit.Fields;
                            var title = fields.ContainsKey("title") ? fields["title"]?.ToString() : "";
                            var content = fields.ContainsKey("content") ? fields["content"]?.ToString() : "";
                            contextList.Add($"- {title}: {content}");
                        }
                    }
                }
            }
            var context = string.Join("\n", contextList);

            // 使用 Semantic Kernel 生成回答
            var prompt = $@"
你是一个专业的闸机设备管理专家。请根据以下相关知识回答用户问题：

相关知识：
{context}

用户问题：{request.Question}

请提供准确、专业的回答：";

            var response = await _kernel.InvokePromptAsync(prompt);
            var answer = response.GetValue<string>() ?? "抱歉，无法生成回答。";

            // 构建相关知识列表
            var relatedKnowledgeList = new List<object>();
            if (relatedKnowledge?.Data != null)
            {
                foreach (var result in relatedKnowledge.Data)
                {
                    if (result.Hits != null)
                    {
                        foreach (var hit in result.Hits)
                        {
                            var fields = hit.Fields;
                            relatedKnowledgeList.Add(new
                            {
                                Title = fields.ContainsKey("title") ? fields["title"]?.ToString() : "",
                                Category = fields.ContainsKey("category") ? fields["category"]?.ToString() : "",
                                Score = hit.Score
                            });
                        }
                    }
                }
            }

            return Ok(new
            {
                Question = request.Question,
                Answer = answer,
                RelatedKnowledge = relatedKnowledgeList,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "智能问答失败: {Question}", request.Question);
            return StatusCode(500, new { error = "智能问答服务暂时不可用，请稍后重试。" });
        }
    }

    /// <summary>
    /// 获取解决方案建议
    /// </summary>
    [HttpPost("solutions")]
    public async Task<IActionResult> GetSolutions([FromBody] SolutionRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Problem))
                return BadRequest("问题描述不能为空");

            // 生成嵌入向量
            var embeddings = await _embeddingService.GenerateAsync([request.Problem]);
            var queryEmbedding = embeddings.First().Vector.ToArray();            // 搜索相关解决方案
            var solutions = await _searchService.SearchAsync("DB_Gate_Knowledge", queryEmbedding, 5);

            // 按分类整理解决方案 - 从 SearchResponse.Data 中提取数据
            var categorizedSolutions = new Dictionary<string, List<object>>();
            var totalSolutions = 0;
            
            if (solutions?.Data != null)
            {
                foreach (var result in solutions.Data)
                {
                    if (result.Hits != null)
                    {
                        foreach (var hit in result.Hits)
                        {
                            var fields = hit.Fields;
                            var category = fields.ContainsKey("category") ? fields["category"]?.ToString() ?? "未分类" : "未分类";
                            
                            if (!categorizedSolutions.ContainsKey(category))
                            {
                                categorizedSolutions[category] = new List<object>();
                            }
                            
                            categorizedSolutions[category].Add(new
                            {
                                Title = fields.ContainsKey("title") ? fields["title"]?.ToString() : "",
                                Content = fields.ContainsKey("content") ? fields["content"]?.ToString() : "",
                                Score = hit.Score
                            });
                            
                            totalSolutions++;
                        }
                    }
                }
            }

            return Ok(new
            {
                Problem = request.Problem,
                Solutions = categorizedSolutions,
                TotalSolutions = totalSolutions,
                Categories = categorizedSolutions.Keys.ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取解决方案失败: {Problem}", request.Problem);
            return StatusCode(500, new { error = "解决方案服务暂时不可用，请稍后重试。" });
        }
    }
}

public class ChatRequest
{
    public string Question { get; set; } = string.Empty;
}

public class SolutionRequest
{
    public string Problem { get; set; } = string.Empty;
    public string? Category { get; set; }
}
