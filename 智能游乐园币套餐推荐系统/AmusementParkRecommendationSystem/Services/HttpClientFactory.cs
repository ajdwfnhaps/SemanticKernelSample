using System.Net.Http;

namespace AmusementParkRecommendationSystem.Services;

/// <summary>
/// AI服务专用HttpClient工厂
/// 统一管理AI服务的HttpClient配置，防止超时和性能问题
/// </summary>
public class AIHttpClientFactory
{
    /// <summary>
    /// 创建配置好的HttpClient实例
    /// </summary>
    /// <returns>配置好超时和Headers的HttpClient</returns>
    public static HttpClient CreateConfiguredClient()
    {
        var httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromMinutes(5) // 5分钟超时，适合AI模型响应时间
        };
        
        // 添加统一的User-Agent标识
        httpClient.DefaultRequestHeaders.Add("User-Agent", "AmusementParkRecommendationSystem/1.0");
        
        return httpClient;
    }
    
    /// <summary>
    /// 创建带自定义超时的HttpClient实例
    /// </summary>
    /// <param name="timeoutMinutes">超时时间（分钟）</param>
    /// <returns>配置好的HttpClient</returns>
    public static HttpClient CreateConfiguredClient(int timeoutMinutes)
    {
        var httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromMinutes(timeoutMinutes)
        };
        
        httpClient.DefaultRequestHeaders.Add("User-Agent", "AmusementParkRecommendationSystem/1.0");
        
        return httpClient;
    }
}
