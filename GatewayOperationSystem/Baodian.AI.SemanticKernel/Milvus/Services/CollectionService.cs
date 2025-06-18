using System.Threading.Tasks;
using Baodian.AI.SemanticKernel.Milvus.Models;

namespace Baodian.AI.SemanticKernel.Milvus.Services
{
    public class CollectionService : MilvusClient
    {
        public CollectionService(string host = "localhost", int port = 9091) : base(host, port)
        {
        }

        /// <summary>
        /// 创建集合 - POST /v2/vectordb/collections/create
        /// </summary>
        public async Task<CollectionResponse> CreateCollectionAsync(CreateCollectionRequest request)
        {
            return await PostAsync<CollectionResponse>("v2/vectordb/collections/create", request);
        }

        /// <summary>
        /// 获取集合详情 - POST /v2/vectordb/collections/describe
        /// </summary>
        public async Task<CollectionResponse> GetCollectionAsync(string collectionName)
        {
            var requestBody = new { collectionName = collectionName };
            return await PostAsync<CollectionResponse>("v2/vectordb/collections/describe", requestBody);
        }

        /// <summary>
        /// 列出所有集合 - POST /v2/vectordb/collections/list (空body)
        /// </summary>
        public async Task<CollectionResponse> ListCollectionsAsync()
        {
            return await PostAsync<CollectionResponse>("v2/vectordb/collections/list", new { });
        }

        /// <summary>
        /// 删除集合 - POST /v2/vectordb/collections/drop
        /// </summary>
        public async Task DeleteCollectionAsync(string collectionName)
        {
            var requestBody = new { collectionName = collectionName };
            await PostAsync<object>("v2/vectordb/collections/drop", requestBody);
        }

        /// <summary>
        /// 检查集合是否存在
        /// </summary>
        public async Task<bool> ExistsAsync(string collectionName)
        {
            try
            {
                var response = await GetCollectionAsync(collectionName);
                return response != null && response.Code == 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 加载集合到内存 - POST /v2/vectordb/collections/load
        /// </summary>
        public async Task<CollectionResponse> LoadCollectionAsync(string collectionName)
        {
            var requestBody = new { collectionName = collectionName };
            return await PostAsync<CollectionResponse>("v2/vectordb/collections/load", requestBody);
        }

        /// <summary>
        /// 从内存释放集合 - POST /v2/vectordb/collections/release
        /// </summary>
        public async Task<CollectionResponse> ReleaseCollectionAsync(string collectionName)
        {
            var requestBody = new { collectionName = collectionName };
            return await PostAsync<CollectionResponse>("v2/vectordb/collections/release", requestBody);
        }

        /// <summary>
        /// 获取集合加载状态 - POST /v2/vectordb/collections/get_load_state
        /// </summary>
        public async Task<CollectionResponse> GetLoadStateAsync(string collectionName)
        {
            var requestBody = new { collectionName = collectionName };
            return await PostAsync<CollectionResponse>("v2/vectordb/collections/get_load_state", requestBody);
        }

        /// <summary>
        /// 重命名集合 - POST /v2/vectordb/collections/rename
        /// </summary>
        public async Task<CollectionResponse> RenameCollectionAsync(string oldName, string newName)
        {
            var requestBody = new { oldName = oldName, newName = newName };
            return await PostAsync<CollectionResponse>("v2/vectordb/collections/rename", requestBody);
        }
    }
}