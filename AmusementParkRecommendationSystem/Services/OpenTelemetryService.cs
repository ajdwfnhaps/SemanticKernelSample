using AmusementParkRecommendationSystem.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;
using static AmusementParkRecommendationSystem.Services.OpenTelemetryExtensions;

namespace AmusementParkRecommendationSystem.Services;

/// <summary>
/// OpenTelemetry 服务扩展
/// </summary>
public static class OpenTelemetryExtensions
{
    /// <summary>
    /// 添加OpenTelemetry服务
    /// </summary>

    public static IServiceCollection AddXueHuaOpentelemetryCore(this IServiceCollection services, IConfiguration config)
    {
        if (!config.GetSection("OpenTelemetry").Exists())
        {
            return services;
        }

        var opt = new OpenTelemetryConfiguration();
        config.GetSection("OpenTelemetry").Bind(opt);
        var serviceName = "SKAIService" ?? "unknown";

        //区分环境
        var envConfig = config.GetSection("EnvSetting:Env");
        if (envConfig?.Exists() ?? false)
        {
            switch (envConfig.Value.ToLower())
            {
                case "develop":
                    serviceName += $"_dev";
                    break;

                case "release":
                    serviceName += $"_test";
                    break;
            }
        }

        Action<ResourceBuilder> configureResource = r => r.AddService(
                        serviceName: serviceName,
                        serviceVersion: Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown",
                        serviceInstanceId: Environment.MachineName);

        var traceBuilder = services
              .AddOpenTelemetry()
              .ConfigureResource(configureResource)
              .WithTracing(builder =>
              {
                  var excludedSpans = new HashSet<string>(opt.FilterSampler);

                  //services.AddScoped(p =>
                  //{
                  //    return new SpanCountProcessor(opt.SpanCountAlter);
                  //});

                  builder
                   .AddSource(serviceName)
                   .AddSource("Microsoft.SemanticKernel")
                   .SetSampler(new XueHuaSampler(excludedSpans))
                   //.AddProcessor(services.BuildServiceProvider().GetService<SpanCountProcessor>())
                   //.AddProcessor(new XueHuaProcessor(excludedSpans))
                   //.ConfigureResource(configureResource)
                   .AddAspNetCoreInstrumentation(o =>
                   {
                       o.RecordException = true;
                       o.Filter = (c) =>
                       {
                           var request = c.Request;
                           if (request.Path.ToString().Equals("/liveness", StringComparison.OrdinalIgnoreCase)
                             || request.Path.ToString().Equals("/readiness", StringComparison.OrdinalIgnoreCase)
                             || request.Path.ToString().Equals("/quartz", StringComparison.OrdinalIgnoreCase)
                             || request.Path.ToString().Equals("/cap/api/stats", StringComparison.OrdinalIgnoreCase)
                             || request.Path.ToString().Equals("/api/test/get", StringComparison.OrdinalIgnoreCase)
                             || request.Path.ToString().Equals("/biz-metrics", StringComparison.OrdinalIgnoreCase)
                             || request.Path.ToString().Equals("/cap", StringComparison.OrdinalIgnoreCase))
                           {
                               return false;
                           }

                           var path = request.Path.ToString().ToLower();
                           if (opt.FilterPaths?.Count > 0 && opt.FilterPaths.Contains(path))
                           {
                               return false;
                           }

                           return true;
                       };



                   })
                   .AddEntityFrameworkCoreInstrumentation(o =>
                   {
                       o.SetDbStatementForText = true;
                   })
                   .AddGrpcClientInstrumentation(o =>
                   {

                   })
                   .AddHttpClientInstrumentation(o =>
                   {
                       o.RecordException = true;
                       o.FilterHttpRequestMessage = (m) =>
                       {
                           var path = m.RequestUri?.AbsolutePath ?? "";

                           if (path.Equals("/liveness", StringComparison.OrdinalIgnoreCase)
                               || path.Equals("/readiness", StringComparison.OrdinalIgnoreCase)
                               || path.Equals("/quartz", StringComparison.OrdinalIgnoreCase)
                               || path.Equals("/cap/api/stats", StringComparison.OrdinalIgnoreCase)
                               || path.Equals("/api/test/get", StringComparison.OrdinalIgnoreCase)
                               || path.Equals("/biz-metrics", StringComparison.OrdinalIgnoreCase)
                               || path.Equals("/cap", StringComparison.OrdinalIgnoreCase))
                           {
                               return false;
                           }

                           path = path.ToLower();
                           if (opt.FilterPaths?.Count > 0 && opt.FilterPaths.Contains(path))
                           {
                               return false;
                           }

                           return true;
                       };
                   })
                   .AddMySqlDataInstrumentation(o =>
                   {
                       o.RecordException = true;
                       o.SetDbStatement = true;
                   });

                  //add OpenTelemetry.Instrumentation.Process

                  var tracingExporter = opt.UseTracingExporter.ToLowerInvariant();

                  switch (tracingExporter)
                  {
                      //                      case "jaeger":
                      //                          builder.AddJaegerExporter();

                      //                          builder.ConfigureServices(services =>
                      //                          {
                      //                              // Use IConfiguration binding for Jaeger exporter options.
                      //                              services.Configure<JaegerExporterOptions>(o => { o = opt.JaegerExporter; });

                      //#if NET60
                      //                                  // Customize the HttpClient that will be used when JaegerExporter is configured for HTTP transport.
                      //                                  services.AddHttpClient("JaegerExporter", configureClient: (client) => client.DefaultRequestHeaders.Add("X-MyCustomHeader", "XueHua"));
                      //#endif
                      //                          });
                      //                          break;

                      //case "zipkin":
                      //builder.AddZipkinExporter();

                      //builder.ConfigureServices(services =>
                      //{
                      //    // Use IConfiguration binding for Zipkin exporter options.
                      //    services.Configure<ZipkinExporterOptions>(o => { o = opt.ZipkinExporter; });
                      //});
                      //break;

                      case "otlp":

                          builder.AddOtlpExporter(o =>
                          {
                              //o = opt.OTLPExporter;
                              //根据前提条件中获取的接入点信息进行修改
                              o.Endpoint = new Uri(opt.OTLPExporter.Endpoint);
                              o.Headers = opt.OTLPExporter.Headers;
                              o.Protocol = OtlpExportProtocol.Grpc;

                          });
                          break;

                      default:
                          builder.AddConsoleExporter();
                          break;
                  }

                  builder.AddConsoleExporter();

              });


        if (opt.MetricsExporter?.IsOpen ?? false)
        {
            traceBuilder.WithMetrics(b =>
            {
                var sName = $"{serviceName}.AspNetCore";

                b.AddMeter(sName)
                   //.SetExemplarFilter(new TraceBasedExemplarFilter())
                   .AddRuntimeInstrumentation()
                   .AddHttpClientInstrumentation()
                   .AddProcessInstrumentation()
                   .AddAspNetCoreInstrumentation();


                switch (opt.MetricsExporter.ExporterType.ToLowerInvariant())
                {
                    case "prometheus":
#if NET60
                            b.AddPrometheusExporter();
#endif
                        break;
                    case "otlp":
                        b.AddOtlpExporter(otlpOptions =>
                        {
                            // Use IConfiguration directly for Otlp exporter endpoint option.
                            otlpOptions.Endpoint = new Uri(opt.OTLPExporter.Endpoint);
                        });
                        break;
                    default:
                        b.AddConsoleExporter();
                        break;
                }
            });
        }

        return services;
    }

    /// <summary>
    /// 添加OpenTelemetry服务
    /// </summary>
    public static IServiceCollection AddOpenTelemetryServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var otelConfig = configuration.GetSection(OpenTelemetryConfiguration.SectionName)
            .Get<OpenTelemetryConfiguration>() ?? new OpenTelemetryConfiguration();



        

        var serviceName = "SK-Service";

        Action<ResourceBuilder> configureResource = r => r.AddService(
                              serviceName: serviceName,
                              serviceVersion: Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown",
                              serviceInstanceId: Environment.MachineName);

        var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .ConfigureResource(configureResource)
            .AddSource("TestSource")
            .AddOtlpExporter(o =>
            {
                o.Endpoint = new Uri(otelConfig.OTLPExporter.Endpoint);
                o.Headers = otelConfig.OTLPExporter.Headers;
                o.Protocol = OtlpExportProtocol.Grpc;
            })
            .AddConsoleExporter()
            .Build();

        return services;

        // 注册OpenTelemetry
        var builder = services.AddOpenTelemetry()
            .ConfigureResource(configureResource);

        // 配置追踪
        builder.WithTracing(tracingBuilder =>
        {
            tracingBuilder
                .AddSource("TestSource")
                .AddSource("AmusementParkRecommendationSystem*")
                .AddSource("Microsoft.SemanticKernel*")
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation(options =>
                {
                    // 过滤不需要追踪的HTTP请求
                    options.FilterHttpRequestMessage = (httpRequestMessage) =>
                    {
                        var path = httpRequestMessage.RequestUri?.PathAndQuery ?? "";
                        return !otelConfig.FilterPaths.Any(filterPath => path.Contains(filterPath));
                    };
                    options.RecordException = true;
                })
                .SetSampler(new TraceIdRatioBasedSampler(1.0)); // 100%采样

            var tracingExporter = otelConfig.UseTracingExporter.ToLowerInvariant();

            switch (tracingExporter)
            {
                case "otlp":
                    tracingBuilder.AddOtlpExporter(o =>
                    {
                        o.Endpoint = new Uri(otelConfig.OTLPExporter.Endpoint);
                        o.Headers = otelConfig.OTLPExporter.Headers;
                        o.Protocol = OtlpExportProtocol.Grpc;
                    });
                    break;

                default:
                    tracingBuilder.AddConsoleExporter();
                    break;
            }

            tracingBuilder.AddConsoleExporter();
        });

        // 配置指标
        if (otelConfig.MetricsExporter?.IsOpen ?? false)
        {
            builder.WithMetrics(metricsBuilder =>
            {
                var sName = $"{serviceName}.AspNetCore";

                metricsBuilder.AddMeter(sName)
                    .AddRuntimeInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddProcessInstrumentation()
                    .AddAspNetCoreInstrumentation();

                switch (otelConfig.MetricsExporter.ExporterType.ToLowerInvariant())
                {
                    case "prometheus":
#if NET60
                        metricsBuilder.AddPrometheusExporter();
#endif
                        break;
                    case "otlp":
                        metricsBuilder.AddOtlpExporter(otlpOptions =>
                        {
                            otlpOptions.Endpoint = new Uri(otelConfig.OTLPExporter.Endpoint);
                        });
                        break;
                    default:
                        metricsBuilder.AddConsoleExporter();
                        break;
                }
            });
        }

        return services;
    }

    public class XueHuaSampler : Sampler
    {
        private readonly HashSet<string> excludedSpans;

        public XueHuaSampler(HashSet<string> excludedSpans)
        {
            this.excludedSpans = excludedSpans;
        }

        public override SamplingResult ShouldSample(in SamplingParameters samplingParameters)
        {
            //Activity currentActivity = Activity.Current;
            //if (currentActivity is not null && excludedSpans?.Count > 0 && excludedSpans.Contains(currentActivity.DisplayName))
            //{
            //    return new SamplingResult(SamplingDecision.Drop);
            //}

            //using var activity = Activity.Current.Source.StartActivity("Processing",
            //                       ActivityKind.Server,
            //                       ActivityContext.Parse(traceParent, null));


            //return new SamplingResult(SamplingDecision.RecordAndSample);

            //var spanName = samplingParameters.Name;

            var tags = samplingParameters.Tags?.Select(t => t.Value?.ToString() ?? "").ToList();

            if (excludedSpans?.Count > 0 && tags?.Count > 0)
            {
                foreach (var tag in tags)
                {
                    if (excludedSpans.Contains(tag.ToLower()))
                    {
                        return new SamplingResult(SamplingDecision.Drop);
                    }
                }
            }

            return new SamplingResult(SamplingDecision.RecordAndSample);
        }
    }


    /// <summary>
    /// Semantic Kernel 的 OpenTelemetry 追踪增强
    /// </summary>


    /// <summary>
    /// Semantic Kernel 遥测服务
    /// </summary>
    public class SemanticKernelTelemetryService
    {
        private readonly ILogger<SemanticKernelTelemetryService> _logger;
        private static readonly System.Diagnostics.ActivitySource ActivitySource = new("AmusementParkRecommendationSystem.SemanticKernel");

        public SemanticKernelTelemetryService(ILogger<SemanticKernelTelemetryService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 开始追踪一个Semantic Kernel操作
        /// </summary>
        public System.Diagnostics.Activity? StartActivity(string operationName, string? description = null)
        {
            var activity = ActivitySource.StartActivity(operationName);

            if (activity != null && !string.IsNullOrEmpty(description))
            {
                activity.SetTag("operation.description", description);
            }

            return activity;
        }

        /// <summary>
        /// 记录函数调用
        /// </summary>
        public void TrackFunctionInvocation(string functionName, string pluginName, TimeSpan duration, bool success = true, string? error = null)
        {
            using var activity = ActivitySource.StartActivity($"sk.function.{functionName}");

            activity?.SetTag("sk.function.name", functionName);
            activity?.SetTag("sk.plugin.name", pluginName);
            activity?.SetTag("sk.function.duration_ms", duration.TotalMilliseconds);
            activity?.SetTag("sk.function.success", success);

            if (!string.IsNullOrEmpty(error))
            {
                activity?.SetTag("sk.function.error", error);
                activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, error);
            }

            _logger.LogInformation("SK Function {FunctionName} in plugin {PluginName} executed in {Duration}ms. Success: {Success}",
                functionName, pluginName, duration.TotalMilliseconds, success);
        }

        /// <summary>
        /// 记录AI推荐调用
        /// </summary>
        public void TrackRecommendation(string recommendationType, int memberId, TimeSpan duration, bool success = true)
        {
            using var activity = ActivitySource.StartActivity($"recommendation.{recommendationType}");

            activity?.SetTag("recommendation.type", recommendationType);
            activity?.SetTag("recommendation.member_id", memberId);
            activity?.SetTag("recommendation.duration_ms", duration.TotalMilliseconds);
            activity?.SetTag("recommendation.success", success);

            _logger.LogInformation("Recommendation {Type} for member {MemberId} completed in {Duration}ms. Success: {Success}",
                recommendationType, memberId, duration.TotalMilliseconds, success);
        }
    }



}

public static class SemanticKernelTelemetryExtensions
{
    /// <summary>
    /// 为Semantic Kernel添加自定义追踪
    /// </summary>
    public static void AddSemanticKernelTracing(this IServiceCollection services)
    {
        // 这里可以添加自定义的Semantic Kernel追踪逻辑
        // 例如：函数调用追踪、插件执行追踪等
        services.AddSingleton<SemanticKernelTelemetryService>();
    }
}