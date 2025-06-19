using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Baodian.AI.SemanticKernel.Constants;
using Baodian.AI.SemanticKernel.Memory;
using Baodian.AI.SemanticKernel.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HtmlAgilityPack; // 添加此命名空间引用

namespace GatewayOperationSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MemoryController : ControllerBase
    {
        private readonly MemoryService _memoryService;
        private readonly ChatService _chatService;
        private const string DefaultCollectionName = "BD_CWB_Col";

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
            var collectionName = string.IsNullOrWhiteSpace(dto.CollectionName) ? DefaultCollectionName : dto.CollectionName;
            var item = new MemoryItem
            {
                Id = dto.DocumentId,
                Content = dto.Content,
                Metadata = new Dictionary<string, object> {
                    { "description", dto.Description },
                    { "collectionName", collectionName }
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
            var collectionName = string.IsNullOrWhiteSpace(dto.CollectionName) ? DefaultCollectionName : dto.CollectionName;
            var options = new MemoryRetrievalOptions { TopK = dto.Limit, Filter = null };
            var result = await _memoryService.SearchAsync(dto.Query, options);
            foreach (var item in result)
            {
                item.Vector = Array.Empty<float>();
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
            var collectionName = string.IsNullOrWhiteSpace(dto.CollectionName) ? DefaultCollectionName : dto.CollectionName;
            var options = new MemoryRetrievalOptions { TopK = dto.Limit, Filter = null, UseHybrid = true };
            var result = await _memoryService.SearchAsync(dto.Query, options);
            foreach (var item in result)
            {
                item.Vector = Array.Empty<float>();
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
            var collectionName = string.IsNullOrWhiteSpace(dto.CollectionName) ? DefaultCollectionName : dto.CollectionName;
            var options = new MemoryRetrievalOptions { TopK = dto.MaxResults, Filter = null };
            var result = await _memoryService.SearchAsync(dto.Query, options);
            foreach (var item in result)
            {
                item.Vector = Array.Empty<float>();
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
            var collectionName = string.IsNullOrWhiteSpace(dto.CollectionName) ? DefaultCollectionName : dto.CollectionName;
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
            var col = string.IsNullOrWhiteSpace(collectionName) ? DefaultCollectionName : collectionName;
            var info = _memoryService.GetCollectionInfo(col);
            return Ok(info);
        }

        /// <summary>
        /// 导入文件到知识库。
        /// 支持格式：txt/html/pdf/docx/xlsx
        /// </summary>
        /// <param name="file">上传的文件</param>
        /// <param name="collectionName">目标集合名称</param>
        /// <returns>操作结果</returns>
        [HttpPost("import-file")]
        public async Task<IActionResult> ImportFile([FromForm] IFormFile file, [FromForm] string collectionName)
        {
            var col = string.IsNullOrWhiteSpace(collectionName) ? DefaultCollectionName : collectionName;
            if (file == null || file.Length == 0)
                return BadRequest("未上传文件");
            string content = await ParseFileContentAsync(file);
            var item = new MemoryItem
            {
                Id = Guid.NewGuid().ToString(),
                Content = content,
                Metadata = new Dictionary<string, object>
                {
                    { "filename", file.FileName },
                    { "collectionName", col }
                }
            };
            await _memoryService.AddAsync(item);
            return Ok(new { success = true });
        }

        private async Task<string> ParseFileContentAsync(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLower();
            using var stream = file.OpenReadStream();
            if (ext == ".txt")
            {
                using var reader = new StreamReader(stream);
                return await reader.ReadToEndAsync();
            }
            if (ext == ".html" || ext == ".htm")
            {
                // TODO: 可用HtmlAgilityPack解析正文
                using var reader = new StreamReader(stream);
                return await reader.ReadToEndAsync();
            }
            if (ext == ".pdf")
            {
                // TODO: 引入PdfPig或iText7解析PDF文本
                return "[PDF内容解析待实现]";
            }
            if (ext == ".docx")
            {
                // TODO: 引入OpenXml或NPOI解析Word文本
                return "[Word内容解析待实现]";
            }
            if (ext == ".xlsx")
            {
                // TODO: 引入NPOI解析Excel文本
                return "[Excel内容解析待实现]";
            }
            return "[暂不支持的文件类型]";
        }

        /// <summary>
        /// 导入网页内容到知识库。
        /// 支持自动提取正文，优先 <article>、<main>、<body>。
        /// </summary>
        /// <param name="url">网页地址</param>
        /// <param name="collectionName">目标集合名称</param>
        /// <returns>操作结果</returns>
        [HttpPost("import-url")]
        public async Task<IActionResult> ImportUrl([FromForm] string url, [FromForm] string collectionName)
        {
            var col = string.IsNullOrWhiteSpace(collectionName) ? DefaultCollectionName : collectionName;
            if (string.IsNullOrWhiteSpace(url))
                return BadRequest("未指定URL");
            string content;
            try
            {
                content = await FetchAndExtractWebContentAsync(url);
            }
            catch (Exception ex)
            {
                return BadRequest($"网页抓取失败: {ex.Message}");
            }
            var item = new MemoryItem
            {
                Id = Guid.NewGuid().ToString(),
                Content = content,
                Metadata = new Dictionary<string, object>
                {
                    { "url", url },
                    { "collectionName", col }
                }
            };
            await _memoryService.AddAsync(item);
            return Ok(new { success = true });
        }

        private async Task<string> FetchAndExtractWebContentAsync(string url)
        {
            using var http = new HttpClient();
            var html = await http.GetStringAsync(url);
            // 使用 HtmlAgilityPack 提取正文
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            // 优先 <article>
            var article = doc.DocumentNode.SelectSingleNode("//article");
            if (article != null)
                return CleanText(article.InnerText);
            // 其次 <main>
            var main = doc.DocumentNode.SelectSingleNode("//main");
            if (main != null)
                return CleanText(main.InnerText);
            // 否则 <body>
            var body = doc.DocumentNode.SelectSingleNode("//body");
            if (body != null)
                return CleanText(body.InnerText);
            // 降级为全量文本
            return CleanText(doc.DocumentNode.InnerText);
        }

        private string CleanText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            // 去除多余空白、换行
            return System.Text.RegularExpressions.Regex.Replace(text, "\\s+", " ").Trim();
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
