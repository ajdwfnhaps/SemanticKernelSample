using System.Threading.Tasks;
namespace Baodian.AI.SemanticKernel.Services;

/// <summary>
/// 数据服务
/// </summary>
public class DataService
{
    /// <summary>
    /// 获取用户数据
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>用户数据</returns>
    public async Task<object> GetUserDataAsync(string userId)
    {
        // 这里应该是实际的数据获取逻辑
        // 这里仅作为示例返回模拟数据
        await Task.Delay(100); // 模拟API调用延迟
        return new { UserId = userId, Name = "测试用户" };
    }
} 