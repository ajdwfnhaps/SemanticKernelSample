using Baodian.AI.SemanticKernel.Providers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baodian.AI.SemanticKernel.Enums
{
    /// <summary>
    /// 模型提供商枚举
    /// </summary>
    public enum ModelProviderEnum
    {
        /// <summary>
        /// 深度求索AI模型提供商
        /// </summary>
        [Description("深度求索AI")]
        DeepSeekAI = 0,
        
        /// <summary>
        /// OpenAI模型提供商
        /// </summary>
        [Description("OpenAI")]
        OpenAI = 1,
        
        /// <summary>
        /// 微软Azure OpenAI模型提供商
        /// </summary>
        [Description("Azure OpenAI")]
        AzureOpenAI = 2,
        
        /// <summary>
        /// 阿里云通义千问模型提供商
        /// </summary>
        [Description("阿里云通义千问")]
        AliyunBailia = 3,
    }
}
