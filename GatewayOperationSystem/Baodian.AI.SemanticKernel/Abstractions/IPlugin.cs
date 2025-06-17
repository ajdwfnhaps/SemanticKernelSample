using Microsoft.SemanticKernel;
using System.Collections.Generic;

namespace Baodian.AI.SemanticKernel.Abstractions;

/// <summary>
/// 插件接口
/// </summary>
public interface IPlugin
{
    /// <summary>
    /// 插件名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 插件描述
    /// </summary>
    string Description { get; }

    /// <summary>
    /// 获取插件函数
    /// </summary>
    /// <returns>插件函数集合</returns>
    IEnumerable<KernelFunction> GetFunctions();
} 