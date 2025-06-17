using System.Collections.Generic;

namespace Baodian.AI.SemanticKernel.Milvus.Models
{
    public class CreateIndexRequest
    {
        public string CollectionName { get; set; }
        public string FieldName { get; set; }
        public string IndexType { get; set; }
        public Dictionary<string, object> Params { get; set; }
    }

    public class IndexResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public IndexInfo Data { get; set; }
    }

    public class IndexInfo
    {
        public string CollectionName { get; set; }
        public string FieldName { get; set; }
        public string IndexType { get; set; }
        public Dictionary<string, object> Params { get; set; }
    }
} 