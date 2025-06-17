using Baodian.AI.SemanticKernel.Abstractions;

namespace Baodian.AI.SemanticKernel.Abstractions;

/// <summary>
/// 模型配置工厂接口
/// </summary>
public interface IModelConfigFactory
{
    /// <summary>
    /// 创建模型配置
    /// </summary>
    /// <param name="modelName">模型名称</param>
    /// <returns>模型配置实例</returns>
    IModelConfig CreateConfig(string modelName);
} 