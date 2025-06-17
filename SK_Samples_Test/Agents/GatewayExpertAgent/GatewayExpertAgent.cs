using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SK_Samples_Test.Agents.GatewayExpertAgent.Plugins;
using SK_Samples_Test.Agents.GatewayExpertAgent.Services;
using System.ComponentModel;

namespace SK_Samples_Test.Agents.GatewayExpertAgent
{
    /// <summary>
    /// 宝点云闸机专家代理 - 基于Semantic Kernel Agent
    /// </summary>
    public class GatewayExpertAgent
    {
        private readonly Kernel _kernel;
        private readonly ChatCompletionAgent _agent;
        private readonly ConfigAnalysisService _configAnalysisService;
        private readonly SolutionGeneratorService _solutionGeneratorService;
        private readonly KnowledgeBaseService _knowledgeBaseService;
        private readonly ILogger<GatewayExpertAgent> _logger;

        public GatewayExpertAgent(
            Kernel kernel,
            ConfigAnalysisService configAnalysisService,
            SolutionGeneratorService solutionGeneratorService,
            KnowledgeBaseService knowledgeBaseService,
            ILogger<GatewayExpertAgent> logger)
        {
            _kernel = kernel;
            _configAnalysisService = configAnalysisService;
            _solutionGeneratorService = solutionGeneratorService;
            _knowledgeBaseService = knowledgeBaseService;
            _logger = logger;            // 添加插件函数到Kernel
            _kernel.Plugins.AddFromObject(new GatewayConfigPlugin(
                _configAnalysisService,
                _solutionGeneratorService),
                "GatewayConfigPlugin");

            _kernel.Plugins.AddFromObject(new TroubleshootingPlugin(
                _solutionGeneratorService,
                _knowledgeBaseService),
                "TroubleshootingPlugin");
            _kernel.Plugins.AddFromObject(new BestPracticePlugin(
          _knowledgeBaseService),
          "BestPracticePlugin");

            // 创建聊天代理
            _agent = new ChatCompletionAgent()
            {
                Instructions = GetSystemInstructions(),
                Name = "GatewayExpert",
                Kernel = _kernel,
                Arguments = new KernelArguments(new OpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
            };
        }

        /// <summary>
        /// 获取系统指令
        /// </summary>
        private string GetSystemInstructions()
        {
            return @"
你是宝点云闸机系统的首席技术顾问，具备5年以上闸机系统部署和运维经验。你的角色是帮助客户完成闸机全生命周期管理，包括配置优化、故障排查、系统集成和性能调优。

核心能力要求​​

​​配置专家​​
能根据场景需求推荐闸机类型（如：人脸识别闸机vs普通闸机）
精通参数配置（包括但不限于：进出闸识别方式、扣费规则、风控设置）
熟悉门票绑定规则和游玩时长设置逻辑
​​故障诊断专家​​
能通过状态提示（如""离线""）快速定位硬件/网络问题
熟悉闸机重启生效机制（如修改开门时间后需重启）
掌握常见错误代码解决方案
​​系统集成顾问​​
指导第三方支付渠道（微信/支付宝）对接
优化会员系统与闸机的数据交互
处理特殊场景（如团体票务、跨场地通行）

[问题诊断]
- 当前状态：离线（P1通道）
- 可能原因：网络中断/电源故障/版本不兼容

[解决方案]
1. 检查网线连接（参考图3状态提示）
2. 验证固件版本是否为5.0.0.43
3. 如需远程协助，请提供闸机日志...

[游乐场场景配置建议]
1. 进出闸设置：
   - 识别方式：人脸+腕带码（参考图4/5）
   - 开门时间：5秒（图7建议范围）
2. 风控规则：
   - 选择""未出闸可进闸""（图6）
3. 超时规则：
   - 阶梯计费（如图9配置）

请根据用户问题，调用相应的插件函数获取准确信息，然后提供专业的回复。
";
        }

        /// <summary>
        /// 处理用户咨询 - 使用SK Agent
        /// </summary>
        public async Task<string> HandleConsultationAsync(string userQuery)
        {
            try
            {
                _logger.LogInformation("收到用户咨询: {Query}", userQuery);

                // 创建聊天历史
                var chatHistory = new ChatHistory();
                chatHistory.AddUserMessage(userQuery);

                // 创建 AgentThread
                ChatHistoryAgentThread agentThread = new(chatHistory);
                await foreach (AgentResponseItem<StreamingChatMessageContent> response in _agent.InvokeStreamingAsync(agentThread))
                {
                    Console.Write(response.Message.Content);
                    // AgentThread 会自动管理历史，不需要手动添加 response
                }

                return "抱歉，处理您的咨询时遇到了问题。";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理用户咨询时发生错误");
                return "抱歉，处理您的咨询时遇到了问题。请稍后重试或联系技术支持。";
            }
        }

        /// <summary>
        /// 启动交互式咨询会话 - 使用SK Agent
        /// </summary>
        public async Task StartInteractiveSessionAsync()
        {
            Console.WriteLine("=== 宝点云闸机专家代理 ===");
            Console.WriteLine("您好！我是宝点云闸机专家，基于Semantic Kernel Agent技术，可以为您提供：");
            Console.WriteLine("1. 智能闸机配置分析和优化建议");
            Console.WriteLine("2. 故障诊断和解决方案");
            Console.WriteLine("3. 最佳实践和维护指导");
            Console.WriteLine("4. 不同场景的部署建议");
            Console.WriteLine();
            Console.WriteLine("请描述您遇到的问题或需要的帮助，输入 'exit' 结束会话。");
            Console.WriteLine(new string('-', 50));

            // 创建持续的聊天历史
            var chatHistory = new ChatHistory();

            while (true)
            {
                Console.Write("\n用户: ");
                var userInput = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(userInput))
                    continue;

                if (userInput.ToLower().Trim() == "exit")
                {
                    Console.WriteLine("\n感谢使用宝点云闸机专家代理！祝您工作顺利！");
                    break;
                }

                Console.WriteLine("\n专家正在分析您的问题...");

                try
                {
                    // 添加用户消息到聊天历史
                    chatHistory.AddUserMessage(userInput);
                    ChatHistoryAgentThread agentThread = new(chatHistory);

                    // 使用Agent处理对话
                    await foreach (AgentResponseItem<StreamingChatMessageContent> response in _agent.InvokeStreamingAsync(agentThread))
                    {
                        Console.Write(response.Message.Content);
                        // AgentThread 会自动管理历史，不需要手动添加 response
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "交互会话中发生错误");
                    Console.WriteLine("\n抱歉，处理您的问题时遇到了错误。请重新描述问题或联系技术支持。");
                }
            }
        }

        /// <summary>
        /// 处理紧急故障 - 使用SK Agent
        /// </summary>
        public async Task<string> HandleEmergencyIssueAsync(string emergencyDescription)
        {
            try
            {
                _logger.LogWarning("收到紧急故障报告: {Description}", emergencyDescription);

                var emergencyPrompt = $@"
        【紧急故障处理】
        故障描述: {emergencyDescription}

        请立即提供：
        1. 问题快速诊断
        2. 紧急处理步骤
        3. 临时解决方案
        4. 后续修复建议

        这是紧急情况，请优先提供可立即执行的解决方案！
        ";
                var chatHistory = new ChatHistory();
                chatHistory.AddUserMessage(emergencyPrompt);

                ChatHistoryAgentThread agentThread = new(chatHistory);
                await foreach (AgentResponseItem<StreamingChatMessageContent> response in _agent.InvokeStreamingAsync(agentThread))
                {
                    Console.Write(response.Message.Content);
                    // AgentThread 会自动管理历史，不需要手动添加 response
                }

                return "抱歉，处理紧急故障时遇到问题，请立即联系技术支持！";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理紧急故障时发生错误");
                return "系统故障，请立即联系技术支持热线！";
            }
        }

        /// <summary>
        /// 获取场景化部署指南 - 使用SK Agent
        /// </summary>
        public async Task<string> GetDeploymentGuideAsync(string scenario, string requirements)
        {
            try
            {
                // 创建 ChatCompletionAgent
                ChatCompletionAgent orderWorkflowAgent = new()
                {
                    Instructions = $@"
        【场景化部署咨询】
        应用场景: {scenario}
        具体需求: {requirements}

        请提供：
        1. 场景分析和需求评估
        2. 推荐的闸机配置方案
        3. 详细的部署步骤
        4. 配置参数建议
        5. 维护和优化建",
                    Name = "OrderWorkflowAgent",
                    Kernel = _kernel,
                    Arguments = new KernelArguments(new OpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
                };


                // With the following implementation:
                ChatHistoryAgentThread agentThread = new(); // 创建自定义的AgentThread实例
                await foreach (AgentResponseItem<StreamingChatMessageContent> response in orderWorkflowAgent.InvokeStreamingAsync(agentThread))
                {
                    Console.Write(response.Message.Content);
                    // AgentThread 会自动管理历史，不需要手动添加 response
                }

                return "抱歉，生成部署指南时遇到问题。";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成部署指南时发生错误");
                return "抱歉，生成部署指南时遇到了问题。请稍后重试。";
            }
        }

        /// <summary>
        /// 简单的一次性咨询 - 不需要对话历史
        /// </summary>
        public async Task<string> HandleSimpleQueryAsync(string query)
        {
            try
            {
                _logger.LogInformation("处理简单查询: {Query}", query);

                // 直接使用Kernel处理，不需要chatHistory
                var result = await _kernel.InvokePromptAsync($@"
{GetSystemInstructions()}

用户问题: {query}

请提供专业的回答。
");

                return result.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理简单查询时发生错误");
                return "抱歉，处理查询时遇到问题。";
            }
        }
    }
}
