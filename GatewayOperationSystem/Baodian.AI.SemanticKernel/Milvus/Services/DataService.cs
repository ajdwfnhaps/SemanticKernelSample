using System.Threading.Tasks;
using System.Collections.Generic;
using Baodian.AI.SemanticKernel.Milvus.Models;
using Baodian.AI.SemanticKernel.Milvus.Configuration;

namespace Baodian.AI.SemanticKernel.Milvus.Services
{
    public class DataService : MilvusClient
    {
        public DataService(MilvusOptions options) : base(options)
        {
        }        public async Task<DataResponse> InsertAsync(InsertRequest request)
        {
            return await PostAsync<DataResponse>("v2/vectordb/entities/insert", request);
        }

        public async Task<DataResponse> DeleteAsync(DeleteRequest request)
        {
            return await PostAsync<DataResponse>("v2/vectordb/entities/delete", request);
        }

        public async Task<DataResponse> QueryAsync(QueryRequest request)
        {
            return await PostAsync<DataResponse>("v2/vectordb/entities/query", request);
        }

        public async Task<DataResponse> GetAsync(string collectionName, string id)
        {
            var requestBody = new
            {
                collectionName = collectionName,
                id = new[] { id }
            };
            return await PostAsync<DataResponse>("v2/vectordb/entities/get", requestBody);
        }

        public async Task<DataResponse> GetAllAsync(string collectionName)
        {
            // 使用 query 接口获取所有数据
            var request = new QueryRequest 
            { 
                CollectionName = collectionName,
                Filter = "", // 空过滤条件表示获取所有数据
                OutputFields = new[] { "*" }, // 获取所有字段
                Limit = 10000 // 设置合理的限制
            };
            return await PostAsync<DataResponse>("v2/vectordb/entities/query", request);
        }

        public async Task<DataResponse> GetByIdAsync(string collectionName, string id)
        {
            return await GetAsync(collectionName, id);
        }

        // 用户数据相关方法（演示用）
        public async Task<object> GetUserDataAsync(string userId)
        {
            // 这是一个演示方法，实际应用中需要根据具体业务逻辑实现
            return new
            {
                UserId = userId,
                Name = $"User_{userId}",
                CreatedAt = DateTime.UtcNow,
                LastAccess = DateTime.UtcNow
            };
        }

        public async Task<object> CreateUserAsync(string userId, object userData)
        {
            // 演示方法：创建用户
            await Task.Delay(10); // 模拟异步操作
            return new { Message = "User created", UserId = userId };
        }

        public async Task<object> UpdateUserAsync(string userId, object userData)
        {
            // 演示方法：更新用户
            await Task.Delay(10); // 模拟异步操作
            return new { Message = "User updated", UserId = userId };
        }

        public async Task<object> DeleteUserAsync(string userId)
        {
            // 演示方法：删除用户
            await Task.Delay(10); // 模拟异步操作
            return new { Message = "User deleted", UserId = userId };
        }
    }
}