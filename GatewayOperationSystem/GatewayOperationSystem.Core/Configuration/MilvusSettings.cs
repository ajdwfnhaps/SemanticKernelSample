namespace GatewayOperationSystem.Core.Configuration;

public class MilvusSettings
{
    public string Endpoint { get; set; } = string.Empty;
    public string Port { get; set; } = "19530";
    public string CollectionName { get; set; } = "gateway_knowledge";
    public int Dimension { get; set; } = 1536;
    public string IndexType { get; set; } = "HNSW";
    public string MetricType { get; set; } = "COSINE";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    
    // Zilliz Cloud 专用配置
    public string ApiKey { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "default";
    public bool EnableSsl { get; set; } = true;
    public int VectorDimension { get; set; } = 1536;
}