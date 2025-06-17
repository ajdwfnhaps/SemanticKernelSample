using Microsoft.SemanticKernel;
using System;

namespace Baodian.AI.SemanticKernel.Abstractions;

/// <summary>
/// Kernel提供者接口
/// </summary>
public interface IKernelProvider
{
    /// <summary>
    /// 创建Kernel实例
    /// </summary>
    /// <param name="config">模型配置</param>
    /// <returns>Kernel实例</returns>
    Kernel CreateKernel(IModelConfig config);

   
} 