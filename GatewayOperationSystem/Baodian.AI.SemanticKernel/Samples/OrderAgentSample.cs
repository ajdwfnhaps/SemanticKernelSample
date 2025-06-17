using Microsoft.SemanticKernel;
using Baodian.Core.Logger;
using System;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Baodian.AI.SemanticKernel.Samples
{
    /// <summary>
    /// 订单处理代理示例
    /// </summary>
    public class OrderAgentSample
    {
        private readonly Kernel _kernel;
        private readonly Baodian.Core.Logger.ILogger _logger;
        private readonly OrderPlugin _orderPlugin;

        public OrderAgentSample(
            Kernel kernel,
            Baodian.Core.Logger.ILogger logger,
            OrderPlugin orderPlugin)
        {
            _kernel = kernel;
            _logger = logger;
            _orderPlugin = orderPlugin;
        }

        public async Task RunAsync()
        {
            try
            {
                _logger.Info("开始运行订单处理示例 (使用 ChatCompletionAgent)");

                // 检查并注册插件（避免重复导入）
                if (!_kernel.Plugins.Contains("OrderPlugin"))
                {
                    _kernel.ImportPluginFromObject(_orderPlugin, "OrderPlugin");
                    Console.WriteLine("OrderPlugin 插件已导入");
                }
                else
                {
                    Console.WriteLine("OrderPlugin 插件已存在，跳过导入");
                }

                // 创建 ChatCompletionAgent
                ChatCompletionAgent orderWorkflowAgent = new()
                {
                    Instructions = @"你是一个订单处理专家，能够协调订单相关的任务。请严格按照以下步骤完成订单处理流程：
1. 首先，使用 GetProductDetailsAsync 获取商品ID为 '123' 的商品详情。
2. 接下来，使用 AddToCartAsync 将商品ID为 '123' 的商品添加到购物车，数量为 2。
3. 然后，使用 CreateOrderAsync 创建订单，购物车ID为 'cart-123'，收货地址为 '北京市朝阳区'。
4. 最后，使用 ProcessPaymentAsync 处理支付，订单号为 'order-123'，支付方式为 '支付宝'。
请在每个步骤完成后，清晰地报告结果。在所有步骤都完成后，总结整个订单处理流程的结果。",
                    Name = "OrderWorkflowAgent",
                    Kernel = _kernel,
                    Arguments = new KernelArguments(new OpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
                };

                // 创建 AgentThread
                ChatHistoryAgentThread agentThread = new();

                // 启动代理并获取响应
                _logger.Info("ChatCompletionAgent 开始执行订单处理流程...");
                await foreach (AgentResponseItem<StreamingChatMessageContent> response in orderWorkflowAgent.InvokeStreamingAsync(agentThread))
                {
                    Console.Write(response.Message.Content);
                    // AgentThread 会自动管理历史，不需要手动添加 response
                }
                _logger.Info("\nChatCompletionAgent 订单处理流程完成。");

                // 打印完整的聊天历史(可选)
                 _logger.Info("完整的聊天历史：");
#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates.
                await foreach (var message in agentThread.GetMessagesAsync())
#pragma warning restore SKEXP0110
                {
                    _logger.Info($"{message.Role}: {message.Content}");
                }

                // 当不再需要时，删除 AgentThread 资源
                await agentThread.DeleteAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "订单处理示例运行失败");
                throw;
            }
        }
    }
} 