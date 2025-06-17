using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Baodian.AI.SemanticKernel.Milvus;
using Baodian.AI.SemanticKernel.Milvus.Configuration;
using Baodian.AI.SemanticKernel.Milvus.Models;
using Baodian.AI.SemanticKernel.Milvus.Services;
using GatewayOperationSystem.Core.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GatewayOperationSystem.Infrastructure.Services
{
    public interface IMilvusVectorStore
    {
        Task InitializeCollectionAsync(string collectionName = "");
        Task<bool> UpsertAsync(string id, float[] vector);
        Task<List<(string Id, float Score)>> SearchAsync(float[] vector, int topK = 5);
        Task<bool> DeleteAsync(string id);
    }

    public class MilvusVectorStore : IMilvusVectorStore
    {
        private readonly CollectionService _collectionService;
        private readonly SearchService _searchService;
        private readonly DataService _dataService;
        private readonly IndexService _indexService;
        private readonly MilvusSettings _settings;
        private readonly ILogger<MilvusVectorStore> _logger;

        public MilvusVectorStore(
            IOptions<MilvusSettings> settings,
            ILogger<MilvusVectorStore> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            var options = new MilvusOptions
            {
                Endpoint = _settings.Endpoint,
                Port = int.Parse(_settings.Port),
                EnableSsl = _settings.EnableSsl,
                Username = _settings.Username,
                Password = _settings.Password,
                Timeout = 30,
                ApiKey = _settings.ApiKey,
                Token = _settings.Token,
                Database = _settings.DatabaseName,
                 
            };

            _collectionService = new CollectionService(options);
            _searchService = new SearchService(options);
            _dataService = new DataService(options);
            _indexService = new IndexService(options);
        }

        public async Task InitializeCollectionAsync(string collectionName = "")
        {
            try
            {
                if (string.IsNullOrEmpty(collectionName))
                {
                    collectionName = _settings.CollectionName;
                }
                // 检查集合是否存在
                var collections = await _collectionService.ListCollectionsAsync();
                var exists = collections.Data?.Name == collectionName;

                if (!exists)
                {
                    // 创建集合
                    var fieldSchema = new FieldSchema
                    {
                        Name = "id",
                        DataType = "Int64",
                        IsPrimaryKey = true
                    };

                    var vectorFieldSchema = new FieldSchema
                    {
                        Name = "vector",
                        DataType = "FloatVector",
                        TypeParams = new Dictionary<string, object>
                        {
                            { "dim", _settings.VectorDimension }
                        }
                    };

                    await _collectionService.CreateCollectionAsync(new CreateCollectionRequest
                    {
                        Name = collectionName,
                        Fields = new List<FieldSchema> { fieldSchema, vectorFieldSchema }
                    });

                    // 创建索引
                    await _indexService.CreateIndexAsync(new CreateIndexRequest
                    {
                        CollectionName = collectionName,
                        FieldName = "vector",
                        IndexType = _settings.IndexType,
                        Params = new Dictionary<string, object>
                        {
                            { "M", 8 },
                            { "efConstruction", 64 },
                            { "metric_type", _settings.MetricType }
                        }
                    });

                    // 加载集合
                    await _collectionService.LoadCollectionAsync(collectionName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "初始化 Milvus 集合失败");
                throw;
            }
        }

        public async Task<bool> UpsertAsync(string id, float[] vector)
        {
            try
            {
                var data = new InsertRequest
                {
                    CollectionName = _settings.CollectionName,
                    Data = new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object>
                        {
                            { "id", long.Parse(id) },
                            { "vector", vector }
                        }
                    }
                };

                await _dataService.InsertAsync(data);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "插入向量数据失败");
                return false;
            }
        }

        public async Task<List<(string Id, float Score)>> SearchAsync(float[] vector, int topK = 5)
        {
            try
            {
                var searchRequest = new SearchRequest
                {
                    CollectionName = _settings.CollectionName,
                    Vectors = new List<float[]> { vector },
                    TopK = topK,
                    SearchParams = new Dictionary<string, object>
                    {
                        { "ef", 64 }
                    }
                };

                var results = await _searchService.SearchAsync(searchRequest);
                var searchResults = new List<(string Id, float Score)>();

                if (results.Data != null)
                {
                    foreach (var result in results.Data)
                    {
                        foreach (var hit in result.Hits)
                        {
                            if (hit.Fields.TryGetValue("id", out var idObj))
                            {
                                searchResults.Add((idObj.ToString(), hit.Score));
                            }
                        }
                    }
                }

                return searchResults;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "搜索向量失败");
                return new List<(string Id, float Score)>();
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var deleteRequest = new DeleteRequest
                {
                    CollectionName = _settings.CollectionName,
                    Expr = $"id == {id}"
                };

                await _dataService.DeleteAsync(deleteRequest);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除向量数据失败");
                return false;
            }
        }
    }
}