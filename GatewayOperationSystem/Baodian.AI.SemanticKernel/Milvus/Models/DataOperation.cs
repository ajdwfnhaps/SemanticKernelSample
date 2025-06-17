using System.Collections.Generic;

namespace Baodian.AI.SemanticKernel.Milvus.Models
{
    public class InsertRequest
    {
        public string CollectionName { get; set; }
        public List<Dictionary<string, object>> Data { get; set; }
        public string PartitionName { get; set; }
    }    public class DeleteRequest
    {
        public string CollectionName { get; set; }
        public string Filter { get; set; }  // V2 API 使用 filter 而不是 Expr
        public string PartitionName { get; set; }
        
        // 兼容性属性
        public string Expr 
        { 
            get => Filter; 
            set => Filter = value; 
        }
    }

    public class QueryRequest
    {
        public string CollectionName { get; set; }
        public string Filter { get; set; }  // V2 API 使用 filter 而不是 Expr
        public string[] OutputFields { get; set; }  // V2 API 使用数组
        public string PartitionName { get; set; }
        public int? Limit { get; set; }
        public int? Offset { get; set; }
        
        // 兼容性属性
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

    public class DataResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
} 