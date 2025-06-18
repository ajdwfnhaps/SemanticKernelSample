using Baodian.AI.SemanticKernel.Milvus.Services;
using Baodian.AI.SemanticKernel.Milvus.Models;
using GatewayOperationSystem.Core.Models;
using GatewayOperationSystem.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace GatewayOperationSystem.API.Controllers;

/// <summary>
/// 知识库管理控制器 - 负责知识库的CRUD、搜索和数据管理
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class KnowledgeBaseController : ControllerBase
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingService;
    private readonly ILogger<KnowledgeBaseController> _logger;
    private readonly DataService _dataService;
    private readonly CollectionService _collectionService;
    private readonly SearchService _searchService;

    public KnowledgeBaseController(
        DataService dataService,
        CollectionService collectionService,
        SearchService searchService,
        IEmbeddingGenerator<string, Embedding<float>> embeddingService,
        ILogger<KnowledgeBaseController> logger)
    {
        _dataService = dataService;
        _collectionService = collectionService;
        _searchService = searchService;
        _embeddingService = embeddingService;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有知识库记录
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<KnowledgeBase>>> GetKnowledgeBases()
    {
        try
        {
            var results = await _dataService.GetAllAsync("DB_Gate_Knowledge");
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取知识库列表失败");
            return StatusCode(500, $"获取知识库列表失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 根据ID获取知识库记录
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<KnowledgeBase>> GetKnowledgeBase(string id)
    {
        try
        {
            var knowledgeBase = await _dataService.GetByIdAsync("DB_Gate_Knowledge", id);
            if (knowledgeBase == null)
            {
                return NotFound($"未找到ID为 {id} 的知识库");
            }
            return Ok(knowledgeBase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取知识库失败: {Id}", id);
            return StatusCode(500, $"获取知识库失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 创建新的知识库记录
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<string>> CreateKnowledge([FromBody] CreateKnowledgeRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest("标题和内容不能为空");
            }
            var knowledge = new KnowledgeBase
            {
                //Id = Guid.NewGuid(),
                Title = request.Title,
                Content = request.Content,
                Summary = request.Summary,
                Category = request.Category ?? "未分类",
                Tags = request.Tags ?? new List<string>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }; var embeddings = await _embeddingService.GenerateAsync([request.Content]);
            var embedding = embeddings.First().Vector.ToArray();
            await _dataService.InsertAsync(new InsertRequest
            {
                CollectionName = "DB_Gate_Knowledge",
                Data = new List<Dictionary<string, object>> {
                    new Dictionary<string, object> {
                        ["id"] = knowledge.Id.ToString(),
                        ["vector"] = embedding,
                        ["title"] = knowledge.Title,
                        ["content"] = knowledge.Content,
                        ["category"] = knowledge.Category,
                        ["tags"] = knowledge.Tags != null ? string.Join(",", knowledge.Tags) : string.Empty,
                        ["created_at"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        ["updated_at"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    }
                }
            });
            _logger.LogInformation("成功创建知识库记录: {Id}", knowledge.Id);
            return Ok(new { Id = knowledge.Id, Message = "知识库记录创建成功" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建知识库失败");
            return StatusCode(500, $"创建知识库失败: {ex.Message}");
        }
    }    /// <summary>
         /// 更新知识库记录
         /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateKnowledge(string id, [FromBody] UpdateKnowledgeRequest request)
    {
        try
        {
            var existingKnowledgeResponse = await _dataService.GetByIdAsync("DB_Gate_Knowledge", id);
            if (existingKnowledgeResponse.Code != 0 || existingKnowledgeResponse.Data.ValueKind == JsonValueKind.Null)
            {
                return NotFound($"未找到ID为 {id} 的知识库");
            }

            // 从 DataResponse.Data 中解析现有数据
            // 预期 Data 是一个包含字典的 JSON 数组
            var dataList = new List<Dictionary<string, object>>();
            if (existingKnowledgeResponse.Data.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in existingKnowledgeResponse.Data.EnumerateArray())
                {
                    var dict = new Dictionary<string, object>();
                    foreach (var prop in element.EnumerateObject())
                    {
                        dict[prop.Name] = prop.Value.GetString() ?? prop.Value.ToString(); // 尝试获取字符串，否则使用 ToString()
                    }
                    dataList.Add(dict);
                }
            }

            if (dataList.Count == 0)
            {
                return NotFound($"未找到ID为 {id} 的知识库数据");
            }

            var existingData = dataList[0];
            var currentTitle = existingData.ContainsKey("title") ? existingData["title"]?.ToString() : "";
            var currentContent = existingData.ContainsKey("content") ? existingData["content"]?.ToString() : "";
            var currentSummary = existingData.ContainsKey("summary") ? existingData["summary"]?.ToString() : "";
            var currentCategory = existingData.ContainsKey("category") ? existingData["category"]?.ToString() : "";
            var currentTags = existingData.ContainsKey("tags") ? existingData["tags"]?.ToString() : "";

            // 更新字段值
            var updatedTitle = request.Title ?? currentTitle;
            var updatedContent = request.Content ?? currentContent;
            var updatedSummary = request.Summary ?? currentSummary;
            var updatedCategory = request.Category ?? currentCategory;
            var updatedTags = request.Tags != null ? string.Join(",", request.Tags) : currentTags; var embeddings = await _embeddingService.GenerateAsync([updatedContent ?? ""]);
            var embedding = embeddings.First().Vector.ToArray();            // 删除旧数据并插入新数据（因为没有真正的 UpdateAsync）
            await _dataService.DeleteAsync(new DeleteRequest
            {
                CollectionName = "DB_Gate_Knowledge",
                Expr = $"id == '{id}'"
            });

            await _dataService.InsertAsync(new InsertRequest
            {
                CollectionName = "DB_Gate_Knowledge",
                Data = new List<Dictionary<string, object>> {
                    new Dictionary<string, object> {
                        ["id"] = id,
                        ["vector"] = embedding,
                        ["title"] = updatedTitle ?? "",
                        ["content"] = updatedContent ?? "",
                        ["summary"] = updatedSummary ?? "",
                        ["category"] = updatedCategory ?? "",
                        ["tags"] = updatedTags ?? "",
                        ["updated_at"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    }
                }
            });

            _logger.LogInformation("成功更新知识库记录: {Id}", id);
            return Ok(new { Message = "知识库记录更新成功" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新知识库失败: {Id}", id);
            return StatusCode(500, $"更新知识库失败: {ex.Message}");
        }
    }    /// <summary>
         /// 删除知识库记录
         /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteKnowledge(string id)
    {
        try
        {
            await _dataService.DeleteAsync(new DeleteRequest
            {
                CollectionName = "DB_Gate_Knowledge",
                Expr = $"id == '{id}'"
            });
            _logger.LogInformation("成功删除知识库记录: {Id}", id);
            return Ok(new { Message = "知识库记录删除成功" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除知识库失败: {Id}", id);
            return StatusCode(500, $"删除知识库失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 搜索知识库
    /// </summary>
    [HttpPost("search")]
    public async Task<ActionResult<List<KnowledgeSearchResult>>> SearchKnowledge([FromBody] SearchRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest("搜索关键词不能为空");
            }
            var embeddings = await _embeddingService.GenerateAsync([request.Query]);
            var queryEmbedding = embeddings.First().Vector.ToArray(); var results = await _searchService.SearchAsync("DB_Gate_Knowledge", queryEmbedding, request.TopK);
            var resultCount = results?.Data.ValueKind == JsonValueKind.Array ? results.Data.GetArrayLength() : 0;
            _logger.LogInformation("搜索知识库: {Query}, 结果数量: {Count}", request.Query, resultCount);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "搜索知识库失败: {Query}", request.Query);
            return StatusCode(500, $"搜索知识库失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 根据标签搜索知识库
    /// </summary>
    [HttpPost("search-by-tags")]
    public async Task<ActionResult<List<KnowledgeSearchResult>>> SearchByTags([FromBody] TagSearchRequest request)
    {
        try
        {
            if (request.Tags == null || !request.Tags.Any())
            {
                return BadRequest("标签不能为空");
            }
            var results = await _searchService.SearchByTagsAsync("DB_Gate_Knowledge", request.Tags, request.TopK);
            var resultCount = results?.Data.ValueKind == JsonValueKind.Array ? results.Data.GetArrayLength() : 0;
            _logger.LogInformation("按标签搜索知识库: {Tags}, 结果数量: {Count}", string.Join(", ", request.Tags), resultCount);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "按标签搜索知识库失败: {Tags}", string.Join(", ", request.Tags ?? new List<string>()));
            return StatusCode(500, $"按标签搜索知识库失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取知识库统计信息
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult> GetKnowledgeStats()
    {
        try
        {
            // 临时实现，使用现有服务获取统计信息
            var allKnowledge = await _dataService.GetAllAsync("DB_Gate_Knowledge");
            var dataList = new List<Dictionary<string, object>>();
            if (allKnowledge.Data.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in allKnowledge.Data.EnumerateArray())
                {
                    var dict = new Dictionary<string, object>();
                    foreach (var prop in element.EnumerateObject())
                    {
                        dict[prop.Name] = prop.Value.ValueKind == JsonValueKind.String
                            ? prop.Value.GetString()
                            : prop.Value.ToString();
                    }
                    dataList.Add(dict);
                }
            }
            var totalCount = dataList.Count;

            var stats = new
            {
                TotalCount = totalCount,
                Categories = new List<string> { "设备介绍", "故障处理", "维护保养" },
                LastUpdated = DateTime.UtcNow
            };
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取知识库统计信息失败");
            return StatusCode(500, $"获取知识库统计信息失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 分页获取知识库记录
    /// </summary>
    [HttpGet("list")]
    public async Task<ActionResult> GetKnowledgeList(int page = 1, int pageSize = 10)
    {
        try
        {
            // 临时实现，使用现有服务获取分页数据
            var allKnowledge = await _dataService.GetAllAsync("DB_Gate_Knowledge");
            var dataList = new List<Dictionary<string, object>>();
            if (allKnowledge.Data.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in allKnowledge.Data.EnumerateArray())
                {
                    var dict = new Dictionary<string, object>();
                    foreach (var prop in element.EnumerateObject())
                    {
                        dict[prop.Name] = prop.Value.ValueKind == JsonValueKind.String
                            ? prop.Value.GetString()
                            : prop.Value.ToString();
                    }
                    dataList.Add(dict);
                }
            }
            var totalCount = dataList.Count;

            var pagedData = dataList.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var result = new
            {
                Data = pagedData,
                Page = page,
                PageSize = pageSize,
                Total = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取知识库列表失败");
            return StatusCode(500, $"获取知识库列表失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 批量导入知识库
    /// </summary>
    [HttpPost("import")]
    public async Task<ActionResult> ImportKnowledge(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("请选择有效的文件");
            }

            var knowledgeList = new List<KnowledgeBase>();

            using (var stream = file.OpenReadStream())
            {
                if (file.ContentType == "application/json" || file.FileName.EndsWith(".json"))
                {
                    // 处理 JSON 文件
                    var jsonContent = await new StreamReader(stream).ReadToEndAsync();
                    var jsonData = JsonSerializer.Deserialize<List<KnowledgeImportItem>>(jsonContent);

                    if (jsonData != null)
                    {
                        knowledgeList = jsonData.Select(item => new KnowledgeBase
                        {
                            //Id = Guid.NewGuid(),
                            Title = item.Title,
                            Content = item.Content,
                            Category = item.Category ?? "未分类",
                            Tags = item.Tags ?? new List<string>(),
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }).ToList();
                    }
                }
                else if (file.ContentType == "text/csv" || file.FileName.EndsWith(".csv"))
                {
                    // 处理 CSV 文件
                    using (var reader = new StreamReader(stream))
                    {
                        var header = await reader.ReadLineAsync();
                        while (!reader.EndOfStream)
                        {
                            var line = await reader.ReadLineAsync();
                            if (!string.IsNullOrEmpty(line))
                            {
                                var fields = line.Split(',');
                                if (fields.Length >= 2)
                                {
                                    knowledgeList.Add(new KnowledgeBase
                                    {
                                        //Id = Guid.NewGuid(),
                                        Title = fields[0].Trim('"'),
                                        Content = fields[1].Trim('"'),
                                        Category = fields.Length > 2 ? fields[2].Trim('"') : "未分类",
                                        Tags = fields.Length > 3 ? fields[3].Trim('"').Split(';').ToList() : new List<string>(),
                                        CreatedAt = DateTime.UtcNow,
                                        UpdatedAt = DateTime.UtcNow
                                    });
                                }
                            }
                        }
                    }
                }
                else
                {
                    return BadRequest("不支持的文件格式，请使用 JSON 或 CSV 文件");
                }
            }

            if (knowledgeList.Count == 0)
            {
                return BadRequest("文件中没有有效的知识库数据");
            }            // 批量导入
            var successCount = 0;
            var failureCount = 0;

            foreach (var knowledge in knowledgeList)
            {
                try
                {
                    var embeddings = await _embeddingService.GenerateAsync([knowledge.Content]);
                    var embedding = embeddings.First().Vector.ToArray(); await _dataService.InsertAsync(new InsertRequest
                    {
                        CollectionName = "DB_Gate_Knowledge",
                        Data = new List<Dictionary<string, object>> {
                            new Dictionary<string, object> {
                                ["id"] = knowledge.Id.ToString(),
                                ["vector"] = embedding,
                                ["title"] = knowledge.Title,
                                ["content"] = knowledge.Content,
                                ["category"] = knowledge.Category ?? "未分类",
                                ["tags"] = knowledge.Tags != null ? string.Join(",", knowledge.Tags) : string.Empty,
                                ["created_at"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                                ["updated_at"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                            }
                        }
                    });

                    successCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "导入知识库失败: {Title}", knowledge.Title);
                    failureCount++;
                }
            }

            var result = new
            {
                TotalCount = knowledgeList.Count,
                SuccessCount = successCount,
                FailureCount = failureCount
            };

            _logger.LogInformation("批量导入完成: 总数={TotalCount}, 成功={SuccessCount}, 失败={FailureCount}",
                result.TotalCount, result.SuccessCount, result.FailureCount);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量导入知识库失败");
            return StatusCode(500, $"批量导入知识库失败: {ex.Message}");
        }
    }

    // 请求模型
    public class CreateKnowledgeRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? Category { get; set; }
        public List<string>? Tags { get; set; }
    }

    public class UpdateKnowledgeRequest
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Summary { get; set; }
        public string? Category { get; set; }
        public List<string>? Tags { get; set; }
    }

    public class SearchRequest
    {
        public string Query { get; set; } = string.Empty;
        public int TopK { get; set; } = 5;
    }

    public class TagSearchRequest
    {
        public List<string> Tags { get; set; } = new();
        public int TopK { get; set; } = 5;
    }

    public class KnowledgeImportItem
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Category { get; set; }
        public List<string>? Tags { get; set; }
    }
}
