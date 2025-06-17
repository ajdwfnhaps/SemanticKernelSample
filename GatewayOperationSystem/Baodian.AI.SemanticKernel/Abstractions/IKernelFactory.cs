using Microsoft.SemanticKernel;
using System;

namespace Baodian.AI.SemanticKernel.Abstractions;

/// <summary>
/// Kernel工厂接口
/// </summary>
public interface IKernelFactory
{
    /// <summary>
    /// 创建Kernel实例
    /// </summary>
    /// <param name="modelName">模型名称</param>
    /// <returns>Kernel实例</returns>
    Kernel CreateKernel(string modelName);

    /// <summary>
    /// 注册Kernel提供者
    /// </summary>
    /// <param name="modelName">模型名称</param>
    /// <param name="provider">Kernel提供者</param>
    void RegisterProvider(string modelName, IKernelProvider provider);
} 