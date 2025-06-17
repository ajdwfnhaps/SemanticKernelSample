using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.Diagnostics;
using System.Text.Json;
using static AmusementParkRecommendationSystem.TracingConstants;

namespace AmusementParkRecommendationSystem.Filter;

/// <summary>
/// OpenTelemetry 函数调用过滤器
/// </summary>
public class OpenTelemetryFunctionFilter : IFunctionInvocationFilter
{
    private readonly ILogger<OpenTelemetryFunctionFilter> _logger;
    // 使用全局 ActivitySource
    // private static readonly ActivitySource ActivitySource = Activity.Current?.Source ?? new ActivitySource("TestSource");

    public OpenTelemetryFunctionFilter(ILogger<OpenTelemetryFunctionFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        var functionName = context.Function.Name;
        var pluginName = context.Function.PluginName ?? "Unknown";
        
        using var activity = TracingConstants.ActivitySource.StartActivity($"sk.function.{functionName}");
        activity?.SetTag("sk.function.name", functionName);
        activity?.SetTag("sk.plugin.name", pluginName);
        activity?.SetTag("sk.function.description", context.Function.Description);

        var stopwatch = Stopwatch.StartNew();
        Exception? exception = null;

        try
        {
            // 记录输入参数
            if (context.Arguments.Count > 0)
            {
                var argumentsJson = JsonSerializer.Serialize(context.Arguments.ToDictionary(
                    kvp => kvp.Key, 
                    kvp => kvp.Value?.ToString() ?? "null"));
                activity?.SetTag("sk.function.arguments", argumentsJson);
            }

            _logger.LogInformation("开始执行 SK 函数: {PluginName}.{FunctionName}", pluginName, functionName);            await next(context);

            // 记录输出结果
            if (context.Result != null)
            {
                var resultPreview = context.Result.ToString();
                if (resultPreview?.Length > 500)
                {
                    resultPreview = resultPreview.Substring(0, 500) + "...";
                }
                activity?.SetTag("sk.function.result_preview", resultPreview);
            }
        }
        catch (Exception ex)
        {
            exception = ex;
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("sk.function.error", ex.Message);
            activity?.SetTag("sk.function.error_type", ex.GetType().Name);
            
            _logger.LogError(ex, "SK 函数执行失败: {PluginName}.{FunctionName}", pluginName, functionName);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            var duration = stopwatch.Elapsed;
            
            activity?.SetTag("sk.function.duration_ms", duration.TotalMilliseconds);
            activity?.SetTag("sk.function.success", exception == null);

            _logger.LogInformation(
                "SK 函数执行完成: {PluginName}.{FunctionName} - 耗时: {Duration}ms - 成功: {Success}",
                pluginName, functionName, duration.TotalMilliseconds, exception == null);
        }
    }
}

/// <summary>
/// OpenTelemetry 提示渲染过滤器
/// </summary>
public class OpenTelemetryPromptFilter : IPromptRenderFilter
{
    private readonly ILogger<OpenTelemetryPromptFilter> _logger;
    // 使用全局 ActivitySource
    // private static readonly ActivitySource ActivitySource = Activity.Current?.Source ?? new ActivitySource("TestSource");

    public OpenTelemetryPromptFilter(ILogger<OpenTelemetryPromptFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
    {
        using var activity = TracingConstants.ActivitySource.StartActivity("sk.prompt.render");
        
        var stopwatch = Stopwatch.StartNew();
        Exception? exception = null;

        try
        {
            // 记录提示模板信息
            if (context.Function != null)
            {
                activity?.SetTag("sk.prompt.function_name", context.Function.Name);
                activity?.SetTag("sk.prompt.plugin_name", context.Function.PluginName);
            }

            await next(context);

            // 记录渲染后的提示长度
            if (context.RenderedPrompt != null)
            {
                activity?.SetTag("sk.prompt.rendered_length", context.RenderedPrompt.Length);
                
                // 记录提示预览（前200字符）
                var promptPreview = context.RenderedPrompt.Length > 200 
                    ? context.RenderedPrompt.Substring(0, 200) + "..."
                    : context.RenderedPrompt;
                activity?.SetTag("sk.prompt.preview", promptPreview);
            }
        }
        catch (Exception ex)
        {
            exception = ex;
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("sk.prompt.error", ex.Message);

            _logger.LogError(ex, "提示渲染失败");
            throw;
        }
        finally
        {
            stopwatch.Stop();
            activity?.SetTag("sk.prompt.duration_ms", stopwatch.Elapsed.TotalMilliseconds);
            activity?.SetTag("sk.prompt.success", exception == null);
        }
    }
}
