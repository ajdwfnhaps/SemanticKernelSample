using System.Threading.Tasks;
using Baodian.AI.SemanticKernel.Milvus.Models;

namespace Baodian.AI.SemanticKernel.Milvus.Services
{
    public class IndexService : MilvusClient
    {
        public IndexService(string host = "localhost", int port = 9091) : base(host, port)
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