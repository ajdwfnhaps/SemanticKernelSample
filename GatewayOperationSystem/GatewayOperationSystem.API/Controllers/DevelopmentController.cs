using Baodian.AI.SemanticKernel.Milvus.Models;
using Baodian.AI.SemanticKernel.Milvus.Services;
using GatewayOperationSystem.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace GatewayOperationSystem.API.Controllers;

/// <summary>
/// 开发测试控制器 - 仅用于开发和测试环境，包含各种测试功能
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class DevelopmentController : ControllerBase
{
    private readonly DataService _dataService;
    private readonly CollectionService _collectionService;
    private readonly SearchService _searchService;

    public DevelopmentController(DataService dataService,
        CollectionService collectionService,
        SearchService searchService)
    {
        _dataService = dataService;
        _collectionService = collectionService;
        _searchService = searchService;
    }

    /// <summary>
    /// 添加测试数据
    /// </summary>
    [HttpPost("test-data")]
    public async Task<ActionResult<string>> AddTestData()
    {
        try
        {
            // 确保collection存在
            if (!await _collectionService.ExistsAsync("DB_Gate_Knowledge"))
            {
                var createRequest = new CreateCollectionRequest
                {
                    Name = "DB_Gate_Knowledge",
                    Description = "Gateway Knowledge Base Collection",
                    Dimension = 768,
                    Fields = new List<FieldSchema>
                    {
                        new FieldSchema
                        {
                            Name = "id",
                            DataType = "VarChar",
                            IsPrimaryKey = true,
                            TypeParams = new Dictionary<string, object> { ["max_length"] = 100 }
                        },
                        new FieldSchema
                        {
                            Name = "vector",
                            DataType = "FloatVector",
                            TypeParams = new Dictionary<string, object> { ["dim"] = 768 }
                        },
                        new FieldSchema
                        {
                            Name = "title",
                            DataType = "VarChar",
                            TypeParams = new Dictionary<string, object> { ["max_length"] = 500 }
                        },
                        new FieldSchema
                        {
                            Name = "content",
                            DataType = "VarChar",
                            TypeParams = new Dictionary<string, object> { ["max_length"] = 5000 }
                        }
                    },
                    EnableDynamicField = true
                };
                await _collectionService.CreateCollectionAsync(createRequest);
                await _collectionService.LoadCollectionAsync("DB_Gate_Knowledge");
            }

            var testKnowledges = GenerateTestKnowledges(5);

            var insertRequests = new List<Dictionary<string, object>>();
            foreach (var knowledge in testKnowledges)
            {
                var mockEmbedding = GenerateMockEmbedding(knowledge.Content);
                insertRequests.Add(new Dictionary<string, object>
                {
                    ["id"] = knowledge.Id.ToString(),
                    ["vector"] = mockEmbedding,
                    ["title"] = knowledge.Title,
                    ["content"] = knowledge.Content,
                    ["summary"] = knowledge.Summary,
                    ["category"] = knowledge.Category,
                    ["tags"] = knowledge.Tags != null ? string.Join(",", knowledge.Tags) : "",
                    ["created_at"] = knowledge.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    ["updated_at"] = knowledge.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }

            var insertRequest = new InsertRequest
            {
                CollectionName = "DB_Gate_Knowledge",
                Data = insertRequests
            };

            var result = await _dataService.InsertAsync(insertRequest);
            return Ok(new { Message = $"成功添加 {testKnowledges.Count} 条测试数据", InsertId = result, Timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "添加测试数据失败", Error = ex.Message });
        }
    }

    /// <summary>
    /// 测试向量搜索
    /// </summary>
    [HttpPost("vector-search")]
    public async Task<ActionResult<List<SearchResultItem>>> TestVectorSearch([FromBody] TestSearchRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Query))
                return BadRequest("查询内容不能为空");

            var queryEmbedding = GenerateMockEmbedding(request.Query);

            var searchRequest = new SearchRequest
            {
                CollectionName = "DB_Gate_Knowledge",
                Vectors = new List<float[]> { queryEmbedding },
                OutputFields = new[] { "id", "title", "content", "category", "summary" },
                TopK = request.TopK,
                SearchParams = new Dictionary<string, object>(),
                Expr = ""
            };

            var results = await _searchService.SearchAsync(searchRequest);
            var searchResults = new List<SearchResultItem>();

            if (results?.Data.ValueKind == JsonValueKind.Array)
            {
                foreach (var resultGroup in results.Data.EnumerateArray())
                {
                    if (resultGroup.TryGetProperty("Hits", out var hits) && hits.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var hit in hits.EnumerateArray())
                        {
                            searchResults.Add(new SearchResultItem
                            {
                                Id = hit.TryGetProperty("id", out var id) ? id.GetString() : null,
                                Title = hit.TryGetProperty("title", out var title) ? title.GetString() : null,
                                Content = hit.TryGetProperty("content", out var content) ? content.GetString() : null,
                                Score = hit.TryGetProperty("score", out var score) ? score.GetSingle() : 0
                            });
                        }
                    }
                }
            }

            return Ok(new { Query = request.Query, Results = searchResults, Timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "向量搜索测试失败", Error = ex.Message });
        }
    }

    /// <summary>
    /// 全面功能测试
    /// </summary>
    [HttpPost("comprehensive-test")]
    public async Task<ActionResult<object>> ComprehensiveTest()
    {
        try
        {
            string upsertResult = "";
            List<SearchResultItem>? searchResults = null;

            // 1. 连接测试
            var connectionTest = await _collectionService.ExistsAsync("DB_Gate_Knowledge");

            // 2. 确保集合存在
            if (!connectionTest)
            {
                var createRequest = new CreateCollectionRequest
                {
                    Name = "DB_Gate_Knowledge",
                    Description = "Gateway Knowledge Base Collection",
                    Dimension = 768,
                    Fields = new List<FieldSchema>
                    {
                        new FieldSchema
                        {
                            Name = "id",
                            DataType = "VarChar",
                            IsPrimaryKey = true,
                            TypeParams = new Dictionary<string, object> { ["max_length"] = 100 }
                        },
                        new FieldSchema
                        {
                            Name = "vector",
                            DataType = "FloatVector",
                            TypeParams = new Dictionary<string, object> { ["dim"] = 768 }
                        }
                    },
                    EnableDynamicField = true
                };
                await _collectionService.CreateCollectionAsync(createRequest);
                await _collectionService.LoadCollectionAsync("DB_Gate_Knowledge");
            }

            // 3. 插入测试数据
            var testKnowledge = GenerateTestKnowledges(1).First();
            var testVector = GenerateMockEmbedding(testKnowledge.Content);

            var insertRequest = new InsertRequest
            {
                CollectionName = "DB_Gate_Knowledge",
                Data = new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object>
                    {
                        ["id"] = testKnowledge.Id.ToString(),
                        ["vector"] = testVector,
                        ["title"] = testKnowledge.Title,
                        ["content"] = testKnowledge.Content,
                        ["summary"] = testKnowledge.Summary ?? "",
                        ["category"] = testKnowledge.Category,
                        ["tags"] = testKnowledge.Tags != null ? string.Join(",", testKnowledge.Tags) : string.Empty,
                        ["created_at"] = testKnowledge.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        ["updated_at"] = testKnowledge.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                    }
                }
            };

            var insertResult = await _dataService.InsertAsync(insertRequest);
            if (insertResult.Code == 0 && insertResult.Data.ValueKind != System.Text.Json.JsonValueKind.Null)
            {
                upsertResult = insertResult.Data.ToString();
            }
            else
            {
                upsertResult = "Insert failed or no specific data returned";
            }

            // 4. 搜索测试
            var searchRequest = new SearchRequest
            {
                CollectionName = "DB_Gate_Knowledge",
                Vectors = new List<float[]> { testVector },
                OutputFields = new[] { "id", "title", "content", "summary" },
                TopK = 5,
                SearchParams = new Dictionary<string, object>(),
                Expr = ""
            };

            var searchResponse = await _searchService.SearchAsync(searchRequest);
            searchResults = new List<SearchResultItem>();

            if (searchResponse?.Data.ValueKind == JsonValueKind.Array)
            {
                foreach (var resultGroup in searchResponse.Data.EnumerateArray())
                {
                    if (resultGroup.TryGetProperty("Hits", out var hits) && hits.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var hit in hits.EnumerateArray())
                        {
                            searchResults.Add(new SearchResultItem
                            {
                                Id = hit.TryGetProperty("id", out var id) ? id.GetString() : null,
                                Title = hit.TryGetProperty("title", out var title) ? title.GetString() : null,
                                Content = hit.TryGetProperty("content", out var content) ? content.GetString() : null,
                                Score = hit.TryGetProperty("score", out var score) ? score.GetSingle() : 0
                            });
                        }
                    }
                }
            }

            // 5. 删除测试数据
            var deleteRequest = new DeleteRequest
            {
                CollectionName = "DB_Gate_Knowledge",
                Expr = $"id == \"{testKnowledge.Id}\"",
                PartitionName = ""
            };

            await _dataService.DeleteAsync(deleteRequest);

            return Ok(new
            {
                Message = "系统全面测试完成",
                TestResults = new
                {
                    ConnectionTest = connectionTest,
                    UpsertSuccess = insertResult.Code == 0,
                    SearchResultsCount = searchResults?.Count ?? 0,
                    FirstSearchResult = searchResults?.FirstOrDefault()
                },
                TestData = new
                {
                    Id = testKnowledge.Id,
                    Title = testKnowledge.Title,
                    VectorId = upsertResult,
                    VectorDimension = testVector.Length,
                    SearchScore = searchResults?.FirstOrDefault()?.Score
                },
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "全面测试失败", Error = ex.Message, StackTrace = ex.StackTrace });
        }
    }

    /// <summary>
    /// 获取所有记录（仅用于调试）
    /// </summary>
    [HttpGet("all-records")]
    public async Task<ActionResult<IEnumerable<object>>> GetAllRecords()
    {
        try
        {
            var queryRequest = new QueryRequest
            {
                CollectionName = "DB_Gate_Knowledge",
                Expr = "",
                OutputFields = new[] { "id", "title", "content", "summary", "category", "tags", "created_at", "updated_at" },
                PartitionName = ""
            };

            var allKnowledge = await _dataService.QueryAsync(queryRequest);
            var records = new List<object>();

            if (allKnowledge?.Data.ValueKind == JsonValueKind.Array)
            {
                var dataList = JsonSerializer.Deserialize<IEnumerable<Dictionary<string, object>>>(allKnowledge.Data.GetRawText());
                if (dataList != null)
                {
                    foreach (var item in dataList)
                    {
                        records.Add(new
                        {
                            Id = item.ContainsKey("id") ? item["id"] : null,
                            Title = item.ContainsKey("title") ? item["title"] : null,
                            Content = item.ContainsKey("content") ? item["content"] : null,
                            Summary = item.ContainsKey("summary") ? item["summary"] : null,
                            Category = item.ContainsKey("category") ? item["category"] : null,
                            Tags = item.ContainsKey("tags") ? item["tags"] : null,
                            CreatedAt = item.ContainsKey("created_at") ? item["created_at"] : null,
                            UpdatedAt = item.ContainsKey("updated_at") ? item["updated_at"] : null
                        });
                    }
                }
            }

            return Ok(new { Records = records, Count = records.Count, Timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "获取所有记录失败", Error = ex.Message });
        }
    }

    #region 私有方法

    private static List<KnowledgeBase> GenerateTestKnowledges(int count)
    {
        var knowledges = new List<KnowledgeBase>();
        for (int i = 0; i < count; i++)
        {
            knowledges.Add(new KnowledgeBase
            {
                Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Title = $"测试知识{i + 1}",
                Content = $"这是第{i + 1}个测试知识的内容，包含了丰富的信息和数据。用于测试Milvus向量数据库的存储和检索功能。",
                Summary = $"测试知识{i + 1}的摘要",
                Category = i % 2 == 0 ? "技术" : "业务",
                Tags = new List<string> { "测试", $"标签{i + 1}" },
                CreatedAt = DateTime.UtcNow.AddDays(-i),
                UpdatedAt = DateTime.UtcNow
            });
        }
        return knowledges;
    }

    private static float[] GenerateMockEmbedding(string content)
    {
        var random = new Random(content.GetHashCode());
        var embedding = new float[768];
        for (int i = 0; i < 768; i++)
        {
            embedding[i] = (float)(random.NextDouble() * 2.0 - 1.0);
        }

        // 归一化向量
        var norm = Math.Sqrt(embedding.Sum(x => x * x));
        if (norm > 0)
        {
            for (int i = 0; i < embedding.Length; i++)
            {
                embedding[i] = (float)(embedding[i] / norm);
            }
        }

        return embedding;
    }

    #endregion
}

public class TestSearchRequest
{
    public string Query { get; set; } = string.Empty;
    public int TopK { get; set; } = 5;
}

public class SearchResultItem
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }  
    public float Score { get; set; }
}
