using System.Collections.Generic;
using System.Threading.Tasks;
using Baodian.AI.SemanticKernel.Memory;
using Microsoft.AspNetCore.Mvc;

namespace GatewayOperationSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MemoryController : ControllerBase
    {
        private readonly MemoryService _memoryService;

        public MemoryController(MemoryService memoryService)
        {
            _memoryService = memoryService;
        }

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

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] SearchRequestDto dto)
        {
            var options = new MemoryRetrievalOptions { TopK = dto.Limit, Filter = null };
            var result = await _memoryService.SearchAsync(dto.Query, options);
            return Ok(result);
        }

        [HttpPost("hybrid-search")]
        public async Task<IActionResult> HybridSearch([FromBody] HybridSearchRequestDto dto)
        {
            var options = new MemoryRetrievalOptions { TopK = dto.Limit, Filter = null, UseHybrid = true };
            var result = await _memoryService.SearchAsync(dto.Query, options);
            return Ok(result);
        }

        [HttpPost("rag-context")]
        public async Task<IActionResult> RagContext([FromBody] RagContextRequestDto dto)
        {
            var options = new MemoryRetrievalOptions { TopK = dto.MaxResults, Filter = null };
            var result = await _memoryService.SearchAsync(dto.Query, options);
            return Ok(result);
        }

        [HttpGet("collections")]
        public IActionResult GetCollections()
        {
            return Ok(new[] { "knowledge_base" });
        }

        [HttpGet("collection/{collectionName}")]
        public IActionResult GetCollectionInfo(string collectionName)
        {
            return Ok(new { collectionName, status = "ok" });
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
