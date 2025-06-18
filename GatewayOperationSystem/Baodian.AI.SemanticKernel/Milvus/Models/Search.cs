using System.Collections.Generic;
using System.Text.Json;

namespace Baodian.AI.SemanticKernel.Milvus.Models
{
    public class SearchRequest
    {
        public string CollectionName { get; set; }
        public List<float[]> Data { get; set; }  // V2 API 使用 data 字段
        public string AnnsField { get; set; } = "vector";  // V2 API 需要指定向量字段名
        public string[] OutputFields { get; set; }
        public int Limit { get; set; }  // V2 API 使用 limit 而不是 TopK
        public string Filter { get; set; }  // V2 API 使用 filter 而不是 Expr
        public Dictionary<string, object> SearchParams { get; set; }
        
        // 兼容性属性，内部转换
        public List<float[]> Vectors 
        { 
            get => Data; 
            set => Data = value; 
        }
        public int TopK 
        { 
            get => Limit; 
            set => Limit = value; 
        }
        public string Expr 
        { 
            get => Filter; 
            set => Filter = value; 
        }
        public string OutputFields_Legacy
        {
            get => OutputFields != null ? string.Join(",", OutputFields) : "*";
            set => OutputFields = string.IsNullOrEmpty(value) ? new[] { "*" } : value.Split(',');
        }
    }

    public class SearchResult
    {
        public List<SearchHit> Hits { get; set; }
        public float Score { get; set; }
    }

    public class SearchHit
    {
        public Dictionary<string, object> Fields { get; set; }
        public float Score { get; set; }
    }

    public class SearchResponse
    {
        public int Code { get; set; }
        public JsonElement Data { get; set; }
    }
} 