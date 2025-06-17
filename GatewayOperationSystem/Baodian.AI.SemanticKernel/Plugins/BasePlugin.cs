using Baodian.AI.SemanticKernel.Abstractions;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Baodian.AI.SemanticKernel.Plugins;

/// <summary>
/// 插件基类
/// </summary>
public abstract class BasePlugin : IPlugin
{
    /// <inheritdoc/>
    public abstract string Name { get; }

    /// <inheritdoc/>
    public abstract string Description { get; }

    /// <inheritdoc/>
    public abstract IEnumerable<KernelFunction> GetFunctions();

    /// <summary>
    /// 创建函数
    /// </summary>
    /// <param name="name">函数名称</param>
    /// <param name="description">函数描述</param>
    /// <param name="function">函数实现</param>
    /// <returns>Kernel函数</returns>
    protected KernelFunction CreateFunction(string name, string description, Func<string, Task<string>> function)
    {
        return KernelFunctionFactory.CreateFromMethod(
            method: function,
            functionName: name,
            description: description);
    }
} 