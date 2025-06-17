namespace GatewayOperationSystem.Core.Models;

/// <summary>
/// 知识库统计信息
/// </summary>
public class KnowledgeStats
{
    /// <summary>
    /// 总记录数
    /// </summary>
    public int Total { get; set; }
    
    /// <summary>
    /// 标签总数
    /// </summary>
    public int TagCount { get; set; }
    
    /// <summary>
    /// 按分类统计
    /// </summary>
    public Dictionary<string, int> Categories { get; set; } = new Dictionary<string, int>();
}
