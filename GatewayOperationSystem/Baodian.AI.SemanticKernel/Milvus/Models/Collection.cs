using System.Collections.Generic;
using System.Text.Json;

namespace Baodian.AI.SemanticKernel.Milvus.Models
{
    public class Collection
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<FieldSchema> Fields { get; set; }
        public bool EnableDynamicField { get; set; }
    }

    public class FieldSchema
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool AutoId { get; set; }
        public Dictionary<string, object> TypeParams { get; set; }
        public Dictionary<string, object> IndexParams { get; set; }
    }    public class CreateCollectionRequest
    {
        public string CollectionName { get; set; }  // V2 API 使用 collectionName
        public string Description { get; set; }
        public List<FieldSchema> Fields { get; set; }
        public bool EnableDynamicField { get; set; }

        public int Dimension { get; set; }

        // 兼容性属性，映射到 CollectionName
        public string Name 
        { 
            get => CollectionName; 
            set => CollectionName = value; 
        }
    }

    public class CollectionResponse
    {
        public int Code { get; set; }
        public JsonElement Data { get; set; }
    }
} 