//using System;
//using System.Threading.Tasks;
//using System.Collections.Generic;
//using Baodian.AI.SemanticKernel.Milvus.Models;
//using Baodian.AI.SemanticKernel.Milvus.Services;

//namespace Baodian.AI.SemanticKernel.Milvus.Examples
//{
//    public class MilvusExample
//    {
//        private readonly CollectionService _collectionService;
//        private readonly SearchService _searchService;

//        public MilvusExample(string host = "localhost", int port = 9091)
//        {
//            _collectionService = new CollectionService(host, port);
//            _searchService = new SearchService(host, port);
//        }

//        public async Task RunExampleAsync()
//        {
//            try
//            {
//                // 创建集合
//                var createRequest = new CreateCollectionRequest
//                {
//                    Name = "example_collection",
//                    Description = "Example collection for vector search",
//                    Fields = new List<FieldSchema>
//                    {
//                        new FieldSchema
//                        {
//                            Name = "id",
//                            DataType = "Int64",
//                            IsPrimaryKey = true,
//                            AutoId = true
//                        },
//                        new FieldSchema
//                        {
//                            Name = "vector",
//                            DataType = "FloatVector",
//                            TypeParams = new Dictionary<string, object>
//                            {
//                                { "dim", 128 }
//                            }
//                        }
//                    }
//                };

//                var createResponse = await _collectionService.CreateCollectionAsync(createRequest);
//                Console.WriteLine($"Collection created: {createResponse.Status}");

//                // 加载集合
//                var loadResponse = await _collectionService.LoadCollectionAsync("example_collection");
//                Console.WriteLine($"Collection loaded: {loadResponse.Status}");

//                // 执行向量搜索
//                var vector = new float[128];
//                for (int i = 0; i < 128; i++)
//                {
//                    vector[i] = (float)new Random().NextDouble();
//                }

//                var searchResponse = await _searchService.SearchAsync("example_collection", vector);
//                Console.WriteLine($"Search completed: {searchResponse.Status}");

//                // 释放集合
//                var releaseResponse = await _collectionService.ReleaseCollectionAsync("example_collection");
//                Console.WriteLine($"Collection released: {releaseResponse.Status}");

//                // 删除集合
//                await _collectionService.DeleteCollectionAsync("example_collection");
//                Console.WriteLine("Collection deleted");
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error: {ex.Message}");
//            }
//        }
//    }
//} 