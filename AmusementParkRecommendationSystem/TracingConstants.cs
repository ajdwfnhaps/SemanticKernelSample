using System.Diagnostics;

namespace AmusementParkRecommendationSystem;

/// <summary>
/// 全局唯一 ActivitySource 实例，供全局链路追踪使用
/// </summary>
public static class TracingConstants
{
    public static readonly ActivitySource ActivitySource = new("TestSource");
}
