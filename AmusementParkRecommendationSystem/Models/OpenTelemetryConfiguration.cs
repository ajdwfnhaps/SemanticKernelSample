using System.ComponentModel.DataAnnotations;

namespace AmusementParkRecommendationSystem.Models;

/// <summary>
/// OpenTelemetry 配置模型
/// </summary>
public class OpenTelemetryConfiguration
{
    public const string SectionName = "OpenTelemetry";

    /// <summary>
    /// 使用的追踪导出器类型
    /// </summary>
    public string UseTracingExporter { get; set; } = "otlp";

    /// <summary>
    /// 指标导出器配置
    /// </summary>
    public MetricsExporterConfig MetricsExporter { get; set; } = new();

    /// <summary>
    /// OTLP导出器配置
    /// </summary>
    public OTLPExporterConfig OTLPExporter { get; set; } = new();

    /// <summary>
    /// 采样过滤器路径
    /// </summary>
    public List<string> FilterSampler { get; set; } = new();

    /// <summary>
    /// 过滤路径列表
    /// </summary>
    public List<string> FilterPaths { get; set; } = new();

    /// <summary>
    /// Span计数警报阈值
    /// </summary>
    public int SpanCountAlter { get; set; } = 500;
}

/// <summary>
/// 指标导出器配置
/// </summary>
public class MetricsExporterConfig
{
    /// <summary>
    /// 是否开启指标导出
    /// </summary>
    public bool IsOpen { get; set; } = true;

    /// <summary>
    /// 导出器类型
    /// </summary>
    public string ExporterType { get; set; } = "prometheus";

    /// <summary>
    /// 禁用HTTP请求指标的服务列表
    /// </summary>
    public List<string> DisableHttpRequestMetricsServices { get; set; } = new();
}

/// <summary>
/// OTLP导出器配置
/// </summary>
public class OTLPExporterConfig
{
    /// <summary>
    /// OTLP端点
    /// </summary>
    [Required]
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// 请求头信息
    /// </summary>
    public string Headers { get; set; } = string.Empty;

    /// <summary>
    /// 获取解析后的请求头字典
    /// </summary>
    public Dictionary<string, string> GetHeadersDictionary()
    {
        var headers = new Dictionary<string, string>();
        
        if (string.IsNullOrEmpty(Headers))
            return headers;

        // 解析 "Key1=Value1,Key2=Value2" 格式的请求头
        var headerPairs = Headers.Split(',', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var pair in headerPairs)
        {
            var keyValue = pair.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);
            if (keyValue.Length == 2)
            {
                headers[keyValue[0].Trim()] = keyValue[1].Trim();
            }
        }

        return headers;
    }
}
