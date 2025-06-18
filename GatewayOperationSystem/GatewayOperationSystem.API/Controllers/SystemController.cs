using Baodian.AI.SemanticKernel.Milvus.Models;
using Baodian.AI.SemanticKernel.Milvus.Services;
using Microsoft.AspNetCore.Mvc;

namespace GatewayOperationSystem.API.Controllers;

/// <summary>
/// 系统管理控制器 - 负责系统级别的管理功能
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class SystemController : ControllerBase
{
    private readonly DataService _dataService;
    private readonly CollectionService _collectionService;
    private readonly SearchService _searchService;

    public SystemController(DataService dataService,
        CollectionService collectionService,
        SearchService searchService)
    {
        _dataService = dataService;
        _collectionService = collectionService;
        _searchService = searchService;
    }

    /// <summary>
    /// 获取系统健康状态
    /// </summary>
    [HttpGet("health")]
    public async Task<ActionResult<object>> GetHealthStatus()
    {
        try
        {
            var collections = await _collectionService.ListCollectionsAsync();
            var knowledgeExists = await _collectionService.ExistsAsync("DB_Gate_Knowledge");

            return Ok(new
            {
                Status = "Healthy",
                DatabaseConnected = true,
                CollectionExists = knowledgeExists,
                Collections = collections,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Status = "Unhealthy", Message = "系统健康检查失败", Error = ex.Message });
        }
    }

    /// <summary>
    /// 初始化系统（创建必要的集合）
    /// </summary>
    [HttpPost("initialize")]
    public async Task<ActionResult<object>> InitializeSystem()
    {
        try
        {
            var collectionName = "DB_Gate_Knowledge";

            // 检查集合是否存在
            var exists = await _collectionService.ExistsAsync(collectionName);

            if (!exists)
            {
                // 创建集合
                var createRequest = new CreateCollectionRequest
                {
                    Name = collectionName,
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
                await _collectionService.LoadCollectionAsync(collectionName);

                return Ok(new
                {
                    Message = $"系统初始化成功，已创建集合 {collectionName}",
                    CollectionName = collectionName,
                    Status = "Initialized",
                    Timestamp = DateTime.UtcNow
                });
            }
            else
            {
                // 确保集合已加载
                await _collectionService.LoadCollectionAsync(collectionName);

                return Ok(new
                {
                    Message = $"系统已初始化，集合 {collectionName} 已存在并已加载",
                    CollectionName = collectionName,
                    Status = "Already Initialized",
                    Timestamp = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "系统初始化失败", Error = ex.Message });
        }
    }

    /// <summary>
    /// 清空指定集合的所有数据（危险操作）
    /// </summary>
    [HttpDelete("collections/{collectionName}/data")]
    public async Task<ActionResult<string>> ClearCollectionData(string collectionName)
    {
        try
        {
            if (await _collectionService.ExistsAsync(collectionName))
            {
                await _collectionService.DeleteCollectionAsync(collectionName);
                return Ok(new { Message = $"集合 {collectionName} 的数据已被清空", Timestamp = DateTime.UtcNow });
            }
            return NotFound(new { Message = $"集合 {collectionName} 不存在" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "清空数据失败", Error = ex.Message });
        }
    }

    /// <summary>
    /// 获取系统统计信息
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<object>> GetSystemStatistics()
    {
        try
        {
            var collections = await _collectionService.ListCollectionsAsync();
            int totalCount = 0;
            if (collections != null)
            {
                // 尝试从 Data 属性获取数量
                if (collections.GetType().GetProperty("Data") != null)
                {
                    var data = collections.GetType().GetProperty("Data")?.GetValue(collections, null);
                    if (data is ICollection<object> col)
                        totalCount = col.Count;
                    else if (data is IEnumerable<object> enumerable)
                        totalCount = enumerable.Count();
                }
                else if (collections is ICollection<object> col)
                {
                    totalCount = col.Count;
                }
                else if (collections is IEnumerable<object> enumerable)
                {
                    totalCount = enumerable.Count();
                }
            }
            return Ok(new
            {
                Collections = collections,
                TotalCollections = totalCount,
                SystemUptime = Environment.TickCount64,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "获取系统统计信息失败", Error = ex.Message });
        }
    }
}
