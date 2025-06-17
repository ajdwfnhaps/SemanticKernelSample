using Microsoft.SemanticKernel;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;
using System;
using System.Text.Json;
using TiktokenSharp;
using System.Linq;
using System.Collections.Generic;

namespace Baodian.AI.SemanticKernel.Filters;

public class SemanticKernelHookFilter : IFunctionInvocationFilter
{
    private readonly ILogger<SemanticKernelHookFilter> _logger;
    private static readonly ActivitySource ActivitySource = new("SemanticKernel");
    private const int MaxTagLength = 1024;
    private static string Truncate(string input) => string.IsNullOrEmpty(input) ? input : (input.Length <= MaxTagLength ? input : input.Substring(0, MaxTagLength) + "...(truncated)");
    private readonly TikToken _tikToken;

    public SemanticKernelHookFilter(ILogger<SemanticKernelHookFilter> logger)
    {
        _logger = logger;
        _tikToken = TikToken.GetEncoding("cl100k_base");
    }

    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        var sw = Stopwatch.StartNew();
        string? error = null;
        string? response = null;
        
        string? requestJson = null;
        string? modelName = null;
        string? promptText = null;

        if (context.Arguments != null)
        {
            if (context.Arguments.TryGetValue("_modelName", out object modelNameObj))
            {
                modelName = modelNameObj?.ToString();
            }
            if (context.Arguments.TryGetValue("_promptText", out object promptTextObj))
            {
                promptText = promptTextObj?.ToString();
            }
        }

        try
        {
            if (context.Arguments != null)
            {
                var filteredArgs = context.Arguments.Where(kv => kv.Key != "_modelName" && kv.Key != "_promptText")
                                                    .ToDictionary(kv => kv.Key, kv => kv.Value);
                requestJson = JsonSerializer.Serialize(filteredArgs, new JsonSerializerOptions { WriteIndented = false });
            }
            await next(context);
            response = context.Result?.ToString();
        }
        catch (Exception ex)
        {
            error = ex.ToString();
            throw;
        }
        finally
        {
            sw.Stop();
            var functionName = context.Function.Name;
            var pluginName = context.Function.PluginName;
            
            string finalRequest = promptText;
            if (!string.IsNullOrEmpty(requestJson) && requestJson != "{}")
            {
                if (!string.IsNullOrEmpty(finalRequest))
                {
                    finalRequest += "\nArguments: ";
                }
                else
                {
                    finalRequest = "Arguments: ";
                }
                finalRequest += requestJson;
            }

            int requestTokens = 0;
            int responseTokens = 0;

            if (_tikToken != null)
            {
                if (!string.IsNullOrEmpty(finalRequest))
                {
                    requestTokens = _tikToken.Encode(finalRequest).Count;
                }
                if (!string.IsNullOrEmpty(response))
                {
                    responseTokens = _tikToken.Encode(response).Count;
                }
            }
            

            using var activity = ActivitySource.StartActivity($"sk.function.{functionName}") ?? Activity.Current?.Source.StartActivity($"sk.function.{functionName}") ?? null;
            if (activity != null)
            {
                activity.SetTag("sk.function.name", functionName);
                activity.SetTag("sk.plugin.name", pluginName);
                activity.SetTag("sk.function.duration_ms", sw.Elapsed.TotalMilliseconds);
                activity.SetTag("sk.function.success", error == null);

                if (!string.IsNullOrEmpty(finalRequest))
                {
                    activity.SetTag("ai.request", Truncate(finalRequest));
                    activity.SetTag("ai.request_tokens", requestTokens);
                }
                if (!string.IsNullOrEmpty(response))
                {
                    activity.SetTag("ai.response", Truncate(response));
                    activity.SetTag("ai.response_tokens", responseTokens);
                }
                if (!string.IsNullOrEmpty(error))
                {
                    activity.SetTag("sk.function.error", Truncate(error));
                    activity.SetStatus(ActivityStatusCode.Error, error);
                }
                
                if (!string.IsNullOrEmpty(modelName))
                {
                    activity.SetTag("ai.model_name", modelName);
                }
            }
            _logger.LogInformation("SK Function {functionName} in plugin {pluginName} executed in {duration:F2}ms. Success: {success}. Model: {modelName}",
                functionName, pluginName, sw.Elapsed.TotalMilliseconds, error == null, modelName ?? "N/A");
        }
    }
}