using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmusementParkRecommendationSystem.Filter
{
    public sealed class LoggingFilter(ILogger logger) : IFunctionInvocationFilter
    {
        public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
        {
            logger.LogInformation("FunctionInvoking - {PluginName}.{FunctionName}", context.Function.PluginName, context.Function.Name);

            await next(context);

            logger.LogInformation("FunctionInvoked - {PluginName}.{FunctionName}", context.Function.PluginName, context.Function.Name);
        }
    }

    public class SafePromptFilter : IPromptRenderFilter
    {
        public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
        {
            // Example: get function information
            var functionName = context.Function.Name;

            await next(context);

            // Example: override rendered prompt before sending it to AI
            context.RenderedPrompt = "Safe prompt";
        }
    }
}
