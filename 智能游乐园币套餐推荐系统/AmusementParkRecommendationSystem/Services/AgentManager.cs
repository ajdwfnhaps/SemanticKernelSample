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
                };                // 客户成功智能体
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

                // 位置智能分析智能体
                _agents["LocationIntelligenceAgent"] = new ChatCompletionAgent
                {
                    Instructions = @"你是一个位置智能分析专家。你的专长包括：
1. 分析客户位置数据和移动模式
2. 识别最佳的店铺邀请时机和策略
3. 基于地理位置提供个性化推荐
4. 分析区域客流趋势和热点分布
5. 评估不同区域的商业价值和潜力
6. 考虑交通便利性、周边环境等因素

分析原则：
- 重视客户隐私和数据安全
- 结合地理位置与个人偏好
- 考虑实时因素（天气、交通、活动等）
- 提供实用的位置建议

输出格式：结构化的位置分析报告，包含地理洞察、客户行为分析和位置建议。",
                    Name = "LocationIntelligenceAgent",
                    Kernel = _kernel
                };

                // 行程规划智能体
                _agents["TripPlannerAgent"] = new ChatCompletionAgent
                {
                    Instructions = @"你是一个专业的行程规划专家。你的核心能力：
1. 制定个性化的一日游行程安排
2. 优化游览路线和时间分配
3. 综合考虑游客偏好、体力、预算等因素
4. 提供详细的交通指引和时间估算
5. 推荐最佳的用餐时间和地点
6. 考虑景点人流量和等待时间
7. 提供应急备选方案

规划原则：
- 最大化游客体验价值
- 合理安排时间避免疲劳
- 平衡热门景点与小众体验
- 考虑实际可行性

输出格式：详细的行程计划，包含时间安排、路线指引、景点介绍和实用提示。",
                    Name = "TripPlannerAgent",
                    Kernel = _kernel
                };

                // 餐饮推荐智能体
                _agents["DiningRecommendationAgent"] = new ChatCompletionAgent
                {
                    Instructions = @"你是一个餐饮推荐专家。你专门提供：
1. 基于位置的个性化餐厅推荐
2. 考虑客户口味偏好和饮食限制
3. 分析餐厅质量、价格和用餐环境
4. 提供最佳用餐时间和预订建议
5. 推荐特色菜品和当地美食
6. 考虑用餐便利性和行程安排
7. 提供多样化的价位选择

推荐标准：
- 口味偏好匹配度
- 地理位置便利性
- 餐厅评价和口碑
- 价格性价比
- 用餐环境和服务质量

输出格式：精选餐厅推荐列表，包含餐厅详情、推荐理由、特色菜品和实用信息。",
                    Name = "DiningRecommendationAgent",
                    Kernel = _kernel
                };

                // 天气与实时信息智能体
                _agents["WeatherAndRealtimeAgent"] = new ChatCompletionAgent
                {
                    Instructions = @"你是一个天气和实时信息分析专家。你负责：
1. 分析天气对游览计划的影响
2. 提供基于天气的活动建议
3. 监控实时交通状况
4. 分析景点人流密度和等待时间
5. 提供应对天气变化的备选方案
6. 考虑季节性因素对体验的影响
7. 提供安全提醒和注意事项

信息维度：
- 天气状况及预报
- 交通实时信息
- 景点开放状态
- 特殊活动和节庆
- 安全和健康提醒

输出格式：实时信息摘要和建议，包含天气影响分析、交通状况、优化建议和注意事项。",
                    Name = "WeatherAndRealtimeAgent",
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
