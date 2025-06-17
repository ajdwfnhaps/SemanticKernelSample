using GatewayOperationSystem.Core.Models;

namespace GatewayOperationSystem.Core.Services;

public interface IMilvusService
{
    Task<string> UpsertKnowledgeAsync(KnowledgeBase knowledge, float[] embedding);
    Task<List<KnowledgeSearchResult>> SearchSimilarAsync(float[] queryEmbedding, int topK = 5);
    Task<bool> DeleteKnowledgeAsync(string id);
    Task<List<KnowledgeSearchResult>> SearchByTagsAsync(List<string> tags, int topK = 5);
    Task<List<KnowledgeBase>> GetAllKnowledgeAsync();
    Task<KnowledgeBase?> GetKnowledgeByIdAsync(string id);
    Task<bool> UpdateKnowledgeAsync(KnowledgeBase knowledge, float[] embedding);
    Task<bool> KnowledgeExistsAsync(string id);
    Task InitializeCollectionAsync();
    Task<bool> TestConnectionAsync();
    
    // 新增方法
    Task<KnowledgeStats> GetKnowledgeStatsAsync();
    Task<KnowledgePageResult> GetKnowledgePageAsync(int page = 1, int pageSize = 10);
    Task<BatchImportResult> BatchImportKnowledgeAsync(List<KnowledgeBase> knowledgeList, Func<string, Task<float[]>> embeddingGenerator);
    Task<bool> DeleteCollectionAsync(string collectionName);
    
    // 集合管理方法
    Task DeleteCollectionAsync();
    Task RecreateCollectionAsync();
}