using Baodian.AI.SemanticKernel.Milvus.Configuration;
using Baodian.AI.SemanticKernel.Milvus.Models;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Baodian.AI.SemanticKernel.Milvus.Services
{
    public class IndexService : MilvusClient
    {
        public IndexService(MilvusOptions options) : base(options)
        {
        }

        public async Task<IndexResponse> CreateIndexAsync(CreateIndexRequest request)
        {
            return await PostAsync<IndexResponse>($"collections/{request.CollectionName}/indexes", request);
        }

        public async Task<IndexResponse> GetIndexAsync(string collectionName, string fieldName)
        {
            return await GetAsync<IndexResponse>($"collections/{collectionName}/indexes/{fieldName}");
        }

        public async Task<IndexResponse> ListIndexesAsync(string collectionName)
        {
            return await GetAsync<IndexResponse>($"collections/{collectionName}/indexes");
        }

        public async Task DeleteIndexAsync(string collectionName, string fieldName)
        {
            await DeleteAsync($"collections/{collectionName}/indexes/{fieldName}");
        }
    }
} 