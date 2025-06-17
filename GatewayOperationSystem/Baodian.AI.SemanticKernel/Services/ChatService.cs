using Baodian.AI.SemanticKernel.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.AI;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace Baodian.AI.SemanticKernel.Services;

/// <summary>
/// 聊天服务示例
/// </summary>
public class ChatService
{
    private readonly IKernelFactory _kernelFactory;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="kernelFactory">Kernel工厂</param>
    public ChatService(IKernelFactory kernelFactory)
    {
        _kernelFactory = kernelFactory;
    }

    /// <summary>
    /// 发送聊天消息
    /// </summary>
    /// <param name="message">用户消息</param>
    /// <returns>AI回复</returns>
    public async Task<string> SendMessageAsync(string modelName, string message)
    {
        try
        {
            // 创建Kernel实例
            var kernel = _kernelFactory.CreateKernel(modelName);

            // 获取聊天服务
            var chatService = kernel.GetRequiredService<IChatCompletionService>();

            // 创建聊天历史
            var chatHistory = new ChatHistory();
            chatHistory.AddUserMessage(message);

            // 获取回复
            var reply = await chatService.GetChatMessageContentAsync(chatHistory);

            return reply.Content;
        }
        catch (Exception ex)
        {
            return $"发生错误: {ex.Message}";
        }
    }

    /// <summary>
    /// 发送带上下文的聊天消息
    /// </summary>
    /// <param name="messages">消息历史</param>
    /// <returns>AI回复</returns>
    public async Task<string> SendMessageWithHistoryAsync(string modelName, ChatHistory messages)
    {
        try
        {
            // 创建Kernel实例
            var kernel = _kernelFactory.CreateKernel(modelName);

            // 获取聊天服务
            var chatService = kernel.GetRequiredService<IChatCompletionService>();

            // 获取回复
            var reply = await chatService.GetChatMessageContentAsync(messages);

            return reply.Content;
        }
        catch (Exception ex)
        {
            return $"发生错误: {ex.Message}";
        }
    }

    /// <summary>
    /// 使用提示模板执行任务
    /// </summary>
    /// <param name="modelName">模型名称</param>
    /// <param name="prompt">提示模板</param>
    /// <param name="arguments">模板参数</param>
    /// <returns>执行结果</returns>
    public async Task<string> InvokePromptAsync(
        string modelName,
        string prompt,
        Dictionary<string, object>? arguments = null)
    {
        var kernel = _kernelFactory.CreateKernel(modelName);
        var kernelArgs = new KernelArguments();
        if (arguments != null)
        {
            foreach (var kv in arguments)
                kernelArgs[kv.Key] = kv.Value;
        }
        kernelArgs["_modelName"] = modelName;
        kernelArgs["_promptText"] = prompt;

        if (kernel.Plugins.Any())
        {
            // 配置执行选项，启用函数调用
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            kernelArgs.ExecutionSettings = new Dictionary<string, PromptExecutionSettings>
            {
                { "default", executionSettings }
            };
        }

        var result = await kernel.InvokePromptAsync(prompt, kernelArgs);
        return result.ToString();
    }

    /// <summary>
    /// 使用提示模板执行任务(支持函数调用)
    /// </summary>
    /// <param name="modelName">模型名称</param>
    /// <param name="prompt">提示模板</param>
    /// <param name="tools">可用的工具/函数列表</param>
    /// <param name="arguments">模板参数</param>
    /// <returns>执行结果</returns>
    public async Task<string> InvokePromptAsync(
        string modelName,
        string prompt,
        IEnumerable<KernelFunction> tools,
        Dictionary<string, object>? arguments = null)
    {
        var kernel = _kernelFactory.CreateKernel(modelName);

        // 设置参数
        var kernelArgs = new KernelArguments();
        if (arguments != null)
        {
            foreach (var kv in arguments)
                kernelArgs[kv.Key] = kv.Value;
        }

        kernelArgs["_modelName"] = modelName;
        kernelArgs["_promptText"] = prompt;

        // 添加工具到Kernel
        // 在SK 1.54.0中，需要将函数添加到KernelPlugin中
        var toolsPlugin = KernelPluginFactory.CreateFromFunctions("Tools", tools);
        kernel.Plugins.Add(toolsPlugin);

        // 配置执行选项，启用函数调用
        var executionSettings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        kernelArgs.ExecutionSettings = new Dictionary<string, PromptExecutionSettings>
        {
            { "default", executionSettings }
        };

        // 执行提示
        var result = await kernel.InvokePromptAsync(prompt, kernelArgs);
        return result.ToString();
    }

    /// <summary>
    /// 使用提示模板执行任务(支持委托函数调用)
    /// </summary>
    /// <typeparam name="TInput">输入参数类型</typeparam>
    /// <typeparam name="TOutput">输出结果类型</typeparam>
    /// <param name="modelName">模型名称</param>
    /// <param name="prompt">提示模板</param>
    /// <param name="functionName">函数名称</param>
    /// <param name="description">函数描述</param>
    /// <param name="functionDelegate">函数委托</param>
    /// <param name="arguments">模板参数</param>
    /// <returns>执行结果</returns>
    public async Task<string> InvokePromptWithDelegateAsync<TInput, TOutput>(
        string modelName,
        string prompt,
        string functionName,
        string description,
        Func<TInput, KernelArguments, Task<TOutput>> functionDelegate,
        Dictionary<string, object>? arguments = null)
    {
        var kernel = _kernelFactory.CreateKernel(modelName);

        // 设置参数
        var kernelArgs = new KernelArguments();
        if (arguments != null)
        {
            foreach (var kv in arguments)
                kernelArgs[kv.Key] = kv.Value;
        }

        kernelArgs["_modelName"] = modelName;
        kernelArgs["_promptText"] = prompt;

        // 创建函数并添加到插件中
        var function = KernelFunctionFactory.CreateFromMethod(
            (TInput input, KernelArguments args) => functionDelegate(input, args),
            functionName,
            description);

        var plugin = KernelPluginFactory.CreateFromFunctions("CustomTools", new[] { function });
        kernel.Plugins.Add(plugin);

        // 配置执行选项，启用函数调用
        var executionSettings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        kernelArgs.ExecutionSettings = new Dictionary<string, PromptExecutionSettings>
        {
            { "default", executionSettings }
        };

        // 执行提示
        var result = await kernel.InvokePromptAsync(prompt, kernelArgs);
        return result.ToString();
    }

    /// <summary>+
    /// 使用提示模板执行任务(支持多个委托函数调用)
    /// </summary>
    /// <param name="modelName">模型名称</param>
    /// <param name="prompt">提示模板</param>
    /// <param name="delegateFunctions">委托函数集合（键为函数名，值为函数描述和委托对象）</param>
    /// <param name="arguments">模板参数</param>
    /// <returns>执行结果</returns>
    public async Task<string> InvokePromptWithDelegatesAsync(
        string modelName,
        string prompt,
        Dictionary<string, (string Description, Delegate Function)> delegateFunctions,
        Dictionary<string, object>? arguments = null)
    {
        var kernel = _kernelFactory.CreateKernel(modelName);

        // 设置参数
        var kernelArgs = new KernelArguments();
        if (arguments != null)
        {
            foreach (var kv in arguments)
                kernelArgs[kv.Key] = kv.Value;
        }

        kernelArgs["_modelName"] = modelName;
        kernelArgs["_promptText"] = prompt;

        // 创建函数集合
        var functions = new List<KernelFunction>();
        foreach (var func in delegateFunctions)
        {
            var function = KernelFunctionFactory.CreateFromMethod(
                func.Value.Function,
                func.Key,
                func.Value.Description);
            functions.Add(function);
        }

        // 添加函数到插件
        var plugin = KernelPluginFactory.CreateFromFunctions("DelegateFunctions", functions);
        kernel.Plugins.Add(plugin);

        // 配置执行选项，启用函数调用
        var executionSettings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        kernelArgs.ExecutionSettings = new Dictionary<string, PromptExecutionSettings>
        {
            { "default", executionSettings }
        };

        // 执行提示
        var result = await kernel.InvokePromptAsync(prompt, kernelArgs);
        return result.ToString();
    }

    /// <summary>
    /// 使用插件执行任务
    /// </summary>
    /// <param name="modelName">模型名称</param>
    /// <param name="pluginName">插件名称</param>
    /// <param name="functionName">函数名称</param>
    /// <param name="input">输入参数</param>
    /// <returns>执行结果</returns>
    public async Task<string> ExecutePluginFunctionAsync(
        string modelName,
        string pluginName,
        string functionName,
        string input)
    {
        var kernel = _kernelFactory.CreateKernel(modelName);

        // 获取插件函数
        var function = kernel.Plugins.GetFunction(pluginName, functionName);
        if (function == null)
        {
            throw new KeyNotFoundException($"未找到插件函数 {pluginName}.{functionName}");
        }

        // 执行函数
        var kernelArgs = new KernelArguments();
        kernelArgs["input"] = input;
        kernelArgs["_modelName"] = modelName;

        var result = await kernel.InvokeAsync(function, kernelArgs);
        return result.ToString();
    }

    /// <summary>
    /// 获取文本嵌入
    /// </summary>
    /// <param name="modelName">模型名称</param>
    /// <param name="text">文本内容</param>
    /// <returns>嵌入向量</returns>
    public async Task<float[]> GetEmbeddingAsync(string modelName, string text)
    {
        var kernel = _kernelFactory.CreateKernel(modelName);

        // 获取文本嵌入服务
        var embeddingService = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();

        // 生成嵌入向量
        var embeddings = await embeddingService.GenerateAsync(new[] { text });
        var embedding = embeddings.FirstOrDefault();
        return embedding?.Vector.ToArray() ?? Array.Empty<float>();
    }
}