using Microsoft.SemanticKernel;
using System.Threading.Tasks;

namespace Baodian.AI.SemanticKernel.Abstractions;

/// <summary>
/// Agent接口
/// </summary>
public interface IAgent
{
    /// <summary>
    /// Agent名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 执行任务
    /// </summary>
    /// <param name="input">输入内容</param>
    /// <returns>执行结果</returns>
    Task<string> ExecuteAsync(string input);

    /// <summary>
    /// 注册插件
    /// </summary>
    /// <param name="plugin">插件实例</param>
    void RegisterPlugin(IPlugin plugin);
} 