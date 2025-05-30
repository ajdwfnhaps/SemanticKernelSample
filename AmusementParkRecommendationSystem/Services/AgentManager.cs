using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.Logging;

namespace AmusementParkRecommendationSystem.Services
{
    /// <summary>
    /// 智能体管理器 - 统一管理和协调各种专业智能体
    /// </summary>
    public class AgentManager
    {
        private readonly Kernel _kernel;
        private readonly ILogger<AgentManager> _logger;
        private readonly Dictionary<string, ChatCompletionAgent> _agents;

        public AgentManager(Kernel kernel, ILogger<AgentManager> logger)
        {
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _agents = new Dictionary<string, ChatCompletionAgent>();

            InitializeAgents();
        }

        /// <summary>
        /// 初始化所有专业智能体
        /// </summary>
        private void InitializeAgents()
        {
            try
            {
                // 业务分析智能体
                _agents["BusinessAnalyst"] = new ChatCompletionAgent
                {
                    Instructions = @"你是一个专业的游乐园业务分析师。你的任务是：
1. 深入分析游乐园的业务数据和运营指标
2. 识别关键趋势、模式和业务机会
3. 提供基于数据的洞察和可执行的建议
4. 关注收入优化、客户满意度和运营效率

分析风格：专业、数据驱动、具体可行。
输出格式：结构化分析报告，包含关键指标、趋势分析、风险评估和改进建议。",
                    Name = "BusinessAnalyst",
                    Kernel = _kernel
                };

                // 定价策略智能体
                _agents["PricingStrategist"] = new ChatCompletionAgent
                {
                    Instructions = @"你是一个专业的定价策略分析师。你的专长包括：
1. 分析当前定价结构的有效性
2. 识别定价优化机会
3. 评估价格变动对收入的潜在影响
4. 考虑市场竞争和客户接受度
5. 制定动态定价策略

目标：制定能够最大化收入同时保持客户满意度的定价策略。
输出格式：详细的定价分析报告，包含价格建议、影响评估和实施方案。",
                    Name = "PricingStrategist",
                    Kernel = _kernel
                };

                // 客户细分智能体
                _agents["CustomerAnalyst"] = new ChatCompletionAgent
                {
                    Instructions = @"你是一个客户行为分析专家。你的核心能力：
1. 基于消费行为、偏好和人口统计学特征对客户进行细分
2. 识别不同客户群体的独特需求和价值
3. 为每个细分群体制定针对性的营销和服务策略
4. 预测客户生命周期价值和流失风险
5. 设计客户旅程优化方案

方法：数据驱动的客户洞察，个性化服务建议。
输出格式：客户细分报告，包含群体特征、价值分析和营销策略。",
                    Name = "CustomerAnalyst",
                    Kernel = _kernel
                };

                // 运营优化智能体
                _agents["OperationsExpert"] = new ChatCompletionAgent
                {
                    Instructions = @"你是一个运营效率优化专家。你专注于：
1. 分析运营流程和效率指标
2. 识别瓶颈和改进机会
3. 制定可执行的运营优化建议
4. 平衡成本控制与服务质量
5. 设计运营流程改进方案

目标：提升运营效率，优化客户体验，降低运营成本。
输出格式：运营优化报告，包含问题识别、解决方案和实施计划。",
                    Name = "OperationsExpert",
                    Kernel = _kernel
                };

                // 个性化服务智能体
                _agents["PersonalizationExpert"] = new ChatCompletionAgent
                {
                    Instructions = @"你是一个个性化服务专家。你的使命：
1. 深度分析个体客户的行为模式和偏好
2. 生成高度个性化的产品和服务推荐
3. 考虑客户的历史数据、当前需求和潜在兴趣
4. 提供能够提升客户满意度和价值的建议
5. 设计个性化客户体验方案

风格：贴心、精准、价值导向的个性化服务。
输出格式：个性化推荐报告，语言亲切，内容具体实用。",
                    Name = "PersonalizationExpert",
                    Kernel = _kernel
                };

                // 数据科学智能体
                _agents["DataScientist"] = new ChatCompletionAgent
                {
                    Instructions = @"你是一个数据科学专家。你的能力包括：
1. 进行复杂的数据分析和统计建模
2. 识别数据中的模式和异常
3. 预测业务趋势和客户行为
4. 验证业务假设和实验结果
5. 提供数据驱动的决策支持

方法：统计分析、机器学习、预测建模。
输出格式：数据分析报告，包含统计洞察、预测结果和建议。",
                    Name = "DataScientist",
                    Kernel = _kernel
                };

                // 客户成功智能体
                _agents["CustomerSuccessManager"] = new ChatCompletionAgent
                {
                    Instructions = @"你是一个客户成功管理专家。你专注于：
1. 提升客户满意度和忠诚度
2. 预防客户流失和提升留存率
3. 识别客户成功的关键因素
4. 设计客户成功计划和里程碑
5. 优化客户生命周期价值

目标：确保客户获得最大价值，建立长期合作关系。
输出格式：客户成功分析报告，包含风险评估、成功策略和行动计划。",
                    Name = "CustomerSuccessManager",
                    Kernel = _kernel
                };

                _logger.LogInformation("智能体管理器初始化完成，共加载 {Count} 个专业智能体", _agents.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "智能体管理器初始化失败");
            }
        }

        /// <summary>
        /// 获取指定的智能体
        /// </summary>
        public ChatCompletionAgent? GetAgent(string agentName)
        {
            _agents.TryGetValue(agentName, out var agent);
            return agent;
        }

        /// <summary>
        /// 获取所有可用的智能体名称
        /// </summary>
        public IEnumerable<string> GetAvailableAgents()
        {
            return _agents.Keys;
        }

        /// <summary>
        /// 检查智能体是否存在
        /// </summary>
        public bool HasAgent(string agentName)
        {
            return _agents.ContainsKey(agentName);
        }

        /// <summary>
        /// 多智能体协作分析
        /// </summary>
        public async Task<string> CollaborativeAnalysisAsync(string topic, string data, params string[] agentNames)
        {
            try
            {
                _logger.LogInformation("开始多智能体协作分析，主题: {Topic}", topic);

                var results = new List<string>();

                foreach (var agentName in agentNames)
                {
                    if (!_agents.TryGetValue(agentName, out var agent))
                    {
                        _logger.LogWarning("智能体 {AgentName} 不存在，跳过", agentName);
                        continue;
                    }

                    var chatHistory = new Microsoft.SemanticKernel.ChatCompletion.ChatHistory();
                    if (!string.IsNullOrEmpty(agent.Instructions))
                    {
                        chatHistory.AddSystemMessage(agent.Instructions);
                    }

                    var prompt = $@"针对主题 '{topic}' 进行专业分析：

数据信息：
{data}

请从你的专业角度提供分析和建议。";

                    chatHistory.AddUserMessage(prompt); var chatCompletionService = _kernel.GetRequiredService<Microsoft.SemanticKernel.ChatCompletion.IChatCompletionService>();
                    var responses = await chatCompletionService.GetChatMessageContentsAsync(chatHistory, null, _kernel);
                    var response = responses.FirstOrDefault();

                    results.Add($"=== {agentName} 分析 ===\n{response?.Content ?? "无响应"}\n");
                }

                var collaborativeResult = string.Join("\n", results);
                _logger.LogInformation("多智能体协作分析完成");

                return collaborativeResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "多智能体协作分析失败");
                return "协作分析过程中发生错误，请稍后重试。";
            }
        }

        /// <summary>
        /// 智能体性能监控
        /// </summary>
        public Dictionary<string, object> GetAgentStatistics()
        {
            return new Dictionary<string, object>
            {
                ["TotalAgents"] = _agents.Count,
                ["AvailableAgents"] = _agents.Keys.ToList(),
                ["InitializationTime"] = DateTime.Now,
                ["Status"] = "Active"
            };
        }
    }
}
