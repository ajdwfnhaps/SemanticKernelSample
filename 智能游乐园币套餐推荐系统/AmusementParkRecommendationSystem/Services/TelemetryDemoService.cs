using System.Diagnostics;
using Microsoft.Extensions.Logging;
using static AmusementParkRecommendationSystem.TracingConstants;

namespace AmusementParkRecommendationSystem.Services;

/// <summary>
/// OpenTelemetry 演示和监控服务
/// </summary>
public class TelemetryDemoService
{
    private readonly ILogger<TelemetryDemoService> _logger;
    // 使用全局 ActivitySource
    // private static readonly ActivitySource ActivitySource = new ActivitySource(nameof(TelemetryDemoService));

    public TelemetryDemoService(ILogger<TelemetryDemoService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 演示 OpenTelemetry 追踪功能
    /// </summary>
    public async Task<string> DemonstrateTracingAsync(string operationName)
    {
        using var activity = TracingConstants.ActivitySource.StartActivity($"demo.{operationName}");
        activity?.SetTag("demo.operation", operationName);
        activity?.SetTag("demo.timestamp", DateTimeOffset.UtcNow.ToString());

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("开始演示操作: {OperationName}", operationName);

            // 模拟一些工作
            await Task.Delay(Random.Shared.Next(100, 500));

            // 模拟嵌套的活动
            using var nestedActivity = TracingConstants.ActivitySource.StartActivity($"demo.{operationName}.nested");
            nestedActivity?.SetTag("demo.nested", true);

            await Task.Delay(Random.Shared.Next(50, 200));

            var result = $"操作 '{operationName}' 成功完成，耗时 {stopwatch.ElapsedMilliseconds}ms";

            activity?.SetTag("demo.result", result);
            activity?.SetTag("demo.success", true);

            _logger.LogInformation("演示操作完成: {OperationName} - {Result}", operationName, result);

            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("demo.error", ex.Message);
            activity?.SetTag("demo.success", false);

            _logger.LogError(ex, "演示操作失败: {OperationName}", operationName);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            activity?.SetTag("demo.duration_ms", stopwatch.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// 创建自定义指标演示
    /// </summary>
    public void RecordCustomMetrics(string metricName, double value, Dictionary<string, object> tags)
    {
        // 在实际应用中，这里会记录自定义指标
        _logger.LogInformation("记录自定义指标: {MetricName} = {Value}, 标签: {Tags}",
            metricName, value, string.Join(", ", tags.Select(kvp => $"{kvp.Key}={kvp.Value}")));
    }

    /// <summary>
    /// 获取当前活动追踪信息
    /// </summary>
    public string GetCurrentTraceInfo()
    {
        var activity = Activity.Current;
        if (activity == null)
        {
            return "当前没有活动的追踪";
        }

        return $"追踪ID: {activity.TraceId}, Span ID: {activity.SpanId}, 操作名: {activity.OperationName}";
    }
}
