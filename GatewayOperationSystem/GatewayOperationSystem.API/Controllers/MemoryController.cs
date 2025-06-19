using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baodian.AI.SemanticKernel.Constants;
using Baodian.AI.SemanticKernel.Memory;
using Baodian.AI.SemanticKernel.Services;
using Microsoft.AspNetCore.Mvc;

namespace GatewayOperationSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MemoryController : ControllerBase
    {
        private readonly MemoryService _memoryService;
        private readonly ChatService _chatService;

        public MemoryController(MemoryService memoryService, ChatService chatService)
        {
            _memoryService = memoryService;
            _chatService = chatService;
        }

        /// <summary>
        /// 存储文档到 Memory。
        /// </summary>
        /// <param name="dto">文档存储请求参数</param>
        /// <returns>操作结果</returns>
        [HttpPost("store")]
        public async Task<IActionResult> Store([FromBody] StoreRequestDto dto)
        {
            var item = new MemoryItem
            {
                Id = dto.DocumentId,
                Content = dto.Content,
                Metadata = new Dictionary<string, object> {
                    { "description", dto.Description },
                    { "collectionName", dto.CollectionName }
                }
            };
            await _memoryService.AddAsync(item);
            return Ok(new { success = true });
        }

        /// <summary>
        /// 检索 Memory，返回相关文档列表。
        /// </summary>
        /// <param name="dto">检索请求参数</param>
        /// <returns>检索结果</returns>
        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] SearchRequestDto dto)
        {
            var options = new MemoryRetrievalOptions { TopK = dto.Limit, Filter = null };
            var result = await _memoryService.SearchAsync(dto.Query, options);
            foreach (var item in result)
            {
                item.Vector = Array.Empty<float>(); // Replace null assignment with an empty array
            }
            return Ok(result);
        }

        /// <summary>
        /// 混合检索 Memory，支持多种检索方式。
        /// </summary>
        /// <param name="dto">混合检索请求参数</param>
        /// <returns>检索结果</returns>
        [HttpPost("hybrid-search")]
        public async Task<IActionResult> HybridSearch([FromBody] HybridSearchRequestDto dto)
        {
            var options = new MemoryRetrievalOptions { TopK = dto.Limit, Filter = null, UseHybrid = true };
            var result = await _memoryService.SearchAsync(dto.Query, options);
            foreach (var item in result)
            {
                item.Vector = Array.Empty<float>(); // Replace null assignment with an empty array
            }
            return Ok(result);
        }

        /// <summary>
        /// 获取 RAG 上下文相关文档。
        /// </summary>
        /// <param name="dto">RAG 上下文请求参数</param>
        /// <returns>相关文档列表</returns>
        [HttpPost("rag-context")]
        public async Task<IActionResult> RagContext([FromBody] RagContextRequestDto dto)
        {
            var options = new MemoryRetrievalOptions { TopK = dto.MaxResults, Filter = null };
            var result = await _memoryService.SearchAsync(dto.Query, options);
            foreach (var item in result)
            {
                item.Vector = Array.Empty<float>(); // Replace null assignment with an empty array
            }
            return Ok(result);
        }

        /// <summary>
        /// 获取 RAG 问答。
        /// </summary>
        /// <param name="dto">RAG 问答请求参数</param>
        /// <returns>问答结果</returns>
        [HttpPost("rag-answer")]
        public async Task<IActionResult> RagAnswer([FromBody] RagContextRequestDto dto)
        {
            var docs = await _memoryService.SearchAsync(dto.Query, new MemoryRetrievalOptions { TopK = dto.MaxResults });
            var context = string.Join("\n", docs.Select(d => d.Content));
            var prompt = $"已知信息：\n{context}\n\n用户问题：{dto.Query}\n请基于已知信息专业、简明地回答用户问题。";
            var answer = await _chatService.InvokePromptAsync(ModelConstants.DeepSeekChat, prompt);
            return Ok(new { answer });
        }

        /// <summary>
        /// 获取所有集合名称。
        /// </summary>
        /// <returns>集合名称列表</returns>
        [HttpGet("collections")]
        public IActionResult GetCollections()
        {
            var collections = _memoryService.GetAllCollections();
            return Ok(collections);
        }

        /// <summary>
        /// 获取指定集合的信息。
        /// </summary>
        /// <param name="collectionName">集合名称</param>
        /// <returns>集合信息</returns>
        [HttpGet("collection/{collectionName}")]
        public IActionResult GetCollectionInfo(string collectionName)
        {
            var info = _memoryService.GetCollectionInfo(collectionName);
            return Ok(info);
        }
    }

    public class StoreRequestDto
    {
        public string CollectionName { get; set; } = string.Empty;
        public string DocumentId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ChunkSize { get; set; } = 1000;
        public int ChunkOverlap { get; set; } = 200;
    }
    public class SearchRequestDto
    {
        public string CollectionName { get; set; } = string.Empty;
        public string Query { get; set; } = string.Empty;
        public int Limit { get; set; } = 5;
        public double MinRelevanceScore { get; set; } = 0.7;
    }
    public class HybridSearchRequestDto
    {
        public string CollectionName { get; set; } = string.Empty;
        public string Query { get; set; } = string.Empty;
        public int Limit { get; set; } = 5;
        public double MinRelevanceScore { get; set; } = 0.7;
    }
    public class RagContextRequestDto
    {
        public string CollectionName { get; set; } = string.Empty;
        public string Query { get; set; } = string.Empty;
        public int MaxResults { get; set; } = 5;
        public double MinRelevanceScore { get; set; } = 0.7;
    }
}
