namespace GatewayOperationSystem.Core.Models;

/// <summary>
/// 知识库分页查询结果
/// </summary>
public class KnowledgePageResult
{
    /// <summary>
    /// 当前页的数据项
    /// </summary>
    public List<KnowledgeBase> Items { get; set; } = new List<KnowledgeBase>();
    
    /// <summary>
    /// 总记录数
    /// </summary>
    public int Total { get; set; }
    
    /// <summary>
    /// 当前页码
    /// </summary>
    public int Page { get; set; }
    
    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages { get; set; }
}
