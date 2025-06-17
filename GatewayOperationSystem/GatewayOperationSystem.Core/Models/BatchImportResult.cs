namespace GatewayOperationSystem.Core.Models;

/// <summary>
/// 批量导入结果
/// </summary>
public class BatchImportResult
{
    /// <summary>
    /// 总处理数量
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// 成功导入数量
    /// </summary>
    public int SuccessCount { get; set; }
    
    /// <summary>
    /// 失败数量
    /// </summary>
    public int FailureCount { get; set; }
    
    /// <summary>
    /// 错误信息列表
    /// </summary>
    public List<string> Errors { get; set; } = new List<string>();
    
    /// <summary>
    /// 是否全部成功
    /// </summary>
    public bool IsAllSuccess => FailureCount == 0;
    
    /// <summary>
    /// 成功率
    /// </summary>
    public double SuccessRate => TotalCount > 0 ? (double)SuccessCount / TotalCount : 0;
}
