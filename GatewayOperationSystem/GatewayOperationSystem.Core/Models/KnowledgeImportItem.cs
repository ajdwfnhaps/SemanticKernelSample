namespace GatewayOperationSystem.Core.Models;

/// <summary>
/// 知识库导入项
/// </summary>
public class KnowledgeImportItem
{
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 内容
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// 分类
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// 标签列表
    /// </summary>
    public List<string>? Tags { get; set; }
}
