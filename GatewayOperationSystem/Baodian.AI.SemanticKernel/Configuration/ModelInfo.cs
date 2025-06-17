using Baodian.AI.SemanticKernel.Abstractions;
using Baodian.AI.SemanticKernel.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baodian.AI.SemanticKernel.Configuration
{
    /// <summary>
    /// 模型信息类
    /// </summary>
    public class ModelInfo
    {
        /// <summary>
        /// 模型提供商实例
        /// </summary>
        public IKernelProvider KernelProvider { get; set; }

        /// <summary>
        /// 模型名称
        /// </summary>
        public string ModelName { get; set; }
    }
}
