using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Baodian.AI.SemanticKernel.Milvus.Models;

namespace Baodian.AI.SemanticKernel.Milvus.Services
{
    public class SearchService : MilvusClient
    {
        public SearchService(string host = "localhost", int port = 9091) : base(host, port)
        {
        }        public async Task<SearchResponse> SearchAsync(SearchRequest request)
        {
            return await PostAsync<SearchResponse>("v2/vectordb/entities/search", request);
        }        public async Task<SearchResponse> SearchAsync(string collectionName, float[] vector, int topK = 10, string outputFields = "*", string expr = null)
        {
            var request = new SearchRequest
            {
                CollectionName = collectionName,
                Data = new List<float[]> { vector },
                AnnsField = "vector",
                Limit = topK,
                OutputFields = string.IsNullOrEmpty(outputFields) ? new[] { "*" } : outputFields.Split(','),
                Filter = expr
            };
            return await SearchAsync(request);
        }        // 根据标签搜索（演示实现）
        public async Task<SearchResponse> SearchByTagsAsync(string collectionName, List<string> tags, int topK = 10)
        {
            // 构建标签过滤表达式
            var tagExpressions = tags.Select(tag => $"tags like '%{tag}%'");
            var filter = string.Join(" OR ", tagExpressions);
            
            // 使用空向量进行搜索，主要依赖 filter 过滤
            var dummyVector = new float[768]; // 假设使用 768 维向量
            
            var request = new SearchRequest
            {
                CollectionName = collectionName,
                Data = new List<float[]> { dummyVector },
                AnnsField = "vector",
                Limit = topK,
                OutputFields = new[] { "*" },
                Filter = filter
            };
            return await SearchAsync(request);
        }
    }
}