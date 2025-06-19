using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Baodian.AI.SemanticKernel.Constants;
using Baodian.AI.SemanticKernel.Memory;
using Baodian.AI.SemanticKernel.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HtmlAgilityPack; // ��Ӵ������ռ�����

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
        /// �洢�ĵ��� Memory��
        /// </summary>
        /// <param name="dto">�ĵ��洢�������</param>
        /// <returns>�������</returns>
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
        /// ���� Memory����������ĵ��б�
        /// </summary>
        /// <param name="dto">�����������</param>
        /// <returns>�������</returns>
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
        /// ��ϼ��� Memory��֧�ֶ��ּ�����ʽ��
        /// </summary>
        /// <param name="dto">��ϼ����������</param>
        /// <returns>�������</returns>
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
        /// ��ȡ RAG ����������ĵ���
        /// </summary>
        /// <param name="dto">RAG �������������</param>
        /// <returns>����ĵ��б�</returns>
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
        /// ��ȡ RAG �ʴ�
        /// </summary>
        /// <param name="dto">RAG �ʴ��������</param>
        /// <returns>�ʴ���</returns>
        [HttpPost("rag-answer")]
        public async Task<IActionResult> RagAnswer([FromBody] RagContextRequestDto dto)
        {
            var collectionName = string.IsNullOrWhiteSpace(dto.CollectionName) ? DefaultCollectionName : dto.CollectionName;
            var docs = await _memoryService.SearchAsync(dto.Query, new MemoryRetrievalOptions { TopK = dto.MaxResults });
            var context = string.Join("\n", docs.Select(d => d.Content));
            var prompt = $"��֪��Ϣ��\n{context}\n\n�û����⣺{dto.Query}\n�������֪��Ϣרҵ�������ػش��û����⡣";
            var answer = await _chatService.InvokePromptAsync(ModelConstants.DeepSeekChat, prompt);
            return Ok(new { answer });
        }

        /// <summary>
        /// ��ȡ���м������ơ�
        /// </summary>
        /// <returns>���������б�</returns>
        [HttpGet("collections")]
        public IActionResult GetCollections()
        {
            var collections = _memoryService.GetAllCollections();
            return Ok(collections);
        }

        /// <summary>
        /// ��ȡָ�����ϵ���Ϣ��
        /// </summary>
        /// <param name="collectionName">��������</param>
        /// <returns>������Ϣ</returns>
        [HttpGet("collection/{collectionName}")]
        public IActionResult GetCollectionInfo(string collectionName)
        {
            var col = string.IsNullOrWhiteSpace(collectionName) ? DefaultCollectionName : collectionName;
            var info = _memoryService.GetCollectionInfo(col);
            return Ok(info);
        }

        /// <summary>
        /// �����ļ���֪ʶ�⡣
        /// ֧�ָ�ʽ��txt/html/pdf/docx/xlsx
        /// </summary>
        /// <param name="file">�ϴ����ļ�</param>
        /// <param name="collectionName">Ŀ�꼯������</param>
        /// <returns>�������</returns>
        [HttpPost("import-file")]
        public async Task<IActionResult> ImportFile([FromForm] IFormFile file, [FromForm] string collectionName)
        {
            var col = string.IsNullOrWhiteSpace(collectionName) ? DefaultCollectionName : collectionName;
            if (file == null || file.Length == 0)
                return BadRequest("δ�ϴ��ļ�");
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
                // TODO: ����HtmlAgilityPack��������
                using var reader = new StreamReader(stream);
                return await reader.ReadToEndAsync();
            }
            if (ext == ".pdf")
            {
                // TODO: ����PdfPig��iText7����PDF�ı�
                return "[PDF���ݽ�����ʵ��]";
            }
            if (ext == ".docx")
            {
                // TODO: ����OpenXml��NPOI����Word�ı�
                return "[Word���ݽ�����ʵ��]";
            }
            if (ext == ".xlsx")
            {
                // TODO: ����NPOI����Excel�ı�
                return "[Excel���ݽ�����ʵ��]";
            }
            return "[�ݲ�֧�ֵ��ļ�����]";
        }

        /// <summary>
        /// ������ҳ���ݵ�֪ʶ�⡣
        /// ֧���Զ���ȡ���ģ����� <article>��<main>��<body>��
        /// </summary>
        /// <param name="url">��ҳ��ַ</param>
        /// <param name="collectionName">Ŀ�꼯������</param>
        /// <returns>�������</returns>
        [HttpPost("import-url")]
        public async Task<IActionResult> ImportUrl([FromForm] string url, [FromForm] string collectionName)
        {
            var col = string.IsNullOrWhiteSpace(collectionName) ? DefaultCollectionName : collectionName;
            if (string.IsNullOrWhiteSpace(url))
                return BadRequest("δָ��URL");
            string content;
            try
            {
                content = await FetchAndExtractWebContentAsync(url);
            }
            catch (Exception ex)
            {
                return BadRequest($"��ҳץȡʧ��: {ex.Message}");
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
            // ʹ�� HtmlAgilityPack ��ȡ����
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            // ���� <article>
            var article = doc.DocumentNode.SelectSingleNode("//article");
            if (article != null)
                return CleanText(article.InnerText);
            // ��� <main>
            var main = doc.DocumentNode.SelectSingleNode("//main");
            if (main != null)
                return CleanText(main.InnerText);
            // ���� <body>
            var body = doc.DocumentNode.SelectSingleNode("//body");
            if (body != null)
                return CleanText(body.InnerText);
            // ����Ϊȫ���ı�
            return CleanText(doc.DocumentNode.InnerText);
        }

        private string CleanText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            // ȥ������հס�����
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
