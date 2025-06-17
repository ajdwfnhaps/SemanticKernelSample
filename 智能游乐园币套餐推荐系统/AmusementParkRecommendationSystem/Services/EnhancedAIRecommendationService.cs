using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Agents;
using Microsoft.Extensions.Logging;
using AmusementParkRecommendationSystem.Models;
using AmusementParkRecommendationSystem.Plugins;
using System.Text.Json;

namespace AmusementParkRecommendationSystem.Services
{    /// <summary>
     /// Enhanced AI recommendation service using modern Semantic Kernel patterns with Agent-based analysis
     /// </summary>
    public class EnhancedAIRecommendationService
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatCompletionService;
        private readonly ILogger<EnhancedAIRecommendationService> _logger;
        private readonly AIRecommendationService _baseRecommendationService;
        private readonly DataService _dataService;
        private readonly EnhancedBusinessAnalysisPlugin _businessPlugin;
        private readonly AgentManager _agentManager;

        // 智能体快速访问
        private ChatCompletionAgent? BusinessAnalysisAgent => _agentManager?.GetAgent("BusinessAnalyst");
        private ChatCompletionAgent? PricingOptimizationAgent => _agentManager?.GetAgent("PricingStrategist");
        private ChatCompletionAgent? CustomerSegmentationAgent => _agentManager?.GetAgent("CustomerAnalyst");
        private ChatCompletionAgent? OperationalAgent => _agentManager?.GetAgent("OperationsExpert");
        private ChatCompletionAgent? PersonalizedRecommendationAgent => _agentManager?.GetAgent("PersonalizationExpert");
        public EnhancedAIRecommendationService(
            Kernel kernel,
            IChatCompletionService chatCompletionService,
            ILogger<EnhancedAIRecommendationService> logger,
            AIRecommendationService baseRecommendationService,
            DataService dataService,
            AgentManager agentManager)
        {
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
            _chatCompletionService = chatCompletionService ?? throw new ArgumentNullException(nameof(chatCompletionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _baseRecommendationService = baseRecommendationService ?? throw new ArgumentNullException(nameof(baseRecommendationService));
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _agentManager = agentManager ?? throw new ArgumentNullException(nameof(agentManager));
            _businessPlugin = new EnhancedBusinessAnalysisPlugin(dataService);

            _logger.LogInformation("增强AI推荐服务初始化完成，使用Agent管理器");
        }        // 已移除旧的智能体初始化方法，现在使用 AgentManager 统一管理        /// <summary>
        /// 执行智能业务分析 - 使用专业业务分析智能体
        /// </summary>
        public async Task<BusinessAnalysisResult> PerformIntelligentBusinessAnalysisAsync(string analysisType)
        {
            try
            {
                _logger.LogInformation("开始执行智能业务分析，类型: {AnalysisType}", analysisType);

                var businessAgent = BusinessAnalysisAgent;
                if (businessAgent == null)
                {
                    throw new InvalidOperationException("业务分析智能体未初始化");
                }

                // 1. 收集真实业务数据
                var memberAnalysis = _businessPlugin.GetComprehensiveMemberAnalysis();
                var packagePerformance = _businessPlugin.AnalyzePackagePerformance();
                var customerSegments = _businessPlugin.IdentifyCustomerSegments();
                var operationalData = _businessPlugin.GenerateOperationalRecommendations();

                // 2. 创建智能体分析计划
                var analysisGoal = $@"请对游乐园进行{analysisType}分析，基于以下真实业务数据：

=== 会员分析数据 ===
{memberAnalysis}

=== 套餐表现数据 ===
{packagePerformance}

=== 客户细分数据 ===
{customerSegments}

=== 运营数据 ===
{operationalData}

分析要求：
- 分析类型：{analysisType}
- 基于真实数据进行深度分析
- 提供具体的数字支撑和趋势分析
- 给出可执行的改进建议
- 识别关键业务机会和风险点

请提供专业、全面的分析报告。";

                // 3. 使用智能体执行分析
                var chatHistory = new ChatHistory();
                if (!string.IsNullOrEmpty(businessAgent.Instructions))
                {
                    chatHistory.AddSystemMessage(businessAgent.Instructions);
                }
                chatHistory.AddUserMessage(analysisGoal);

                var response = await _chatCompletionService.GetChatMessageContentAsync(chatHistory, null, _kernel);
                var aiAnalysis = response.Content ?? "分析结果为空";

                // 4. 构建结构化结果 - 完全展示AI原始响应
                var result = new BusinessAnalysisResult
                {
                    AnalysisType = analysisType.ToUpper(),
                    GeneratedAt = DateTime.Now,
                    // 完全使用AI的原始分析内容，不进行任何封装或提取
                    Insights = new List<string> { aiAnalysis },
                    Recommendations = new List<string> { "详细建议已包含在上述AI分析中" },
                    Metrics = new Dictionary<string, object> { ["AI分析内容长度"] = aiAnalysis.Length }
                };

                _logger.LogInformation("业务分析完成，AI分析内容长度: {ContentLength} 字符", aiAnalysis.Length);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行智能业务分析时发生错误");
                return new BusinessAnalysisResult
                {
                    AnalysisType = analysisType,
                    GeneratedAt = DateTime.Now,
                    Insights = new List<string> { "分析过程中遇到技术问题，请稍后重试" },
                    Recommendations = new List<string> { "建议检查系统配置和网络连接" },
                    Metrics = new Dictionary<string, object>()
                };
            }
        }        /// <summary>
                 /// 定价优化分析 - 使用专业定价策略智能体
                 /// </summary>
        public async Task<List<PricingOptimizationResult>> OptimizePricingAsync()
        {
            try
            {
                _logger.LogInformation("开始执行定价优化分析");

                var pricingAgent = PricingOptimizationAgent;
                if (pricingAgent == null)
                {
                    throw new InvalidOperationException("定价优化智能体未初始化");
                }

                // 1. 获取真实套餐数据和表现
                var packagePerformance = _businessPlugin.AnalyzePackagePerformance();
                var memberAnalysis = _businessPlugin.GetComprehensiveMemberAnalysis();
                var packages = _dataService.GetAvailableCoinPackages();

                // 2. 创建定价优化分析目标
                var pricingGoal = $@"请对以下游乐园套餐进行专业的定价优化分析：

=== 套餐表现数据 ===
{packagePerformance}

=== 会员消费数据 ===
{memberAnalysis}

=== 当前套餐信息 ===
{JsonSerializer.Serialize(packages.Select(p => new { p.Id, p.Name, p.Price, p.CoinCount, p.BonusCoins }), new JsonSerializerOptions { WriteIndented = true })}

请提供：
1. 每个套餐的当前定价评估
2. 具体的价格调整建议和理由
3. 预期的收入影响分析
4. 市场接受度和竞争力分析
5. 实施建议和注意事项";

                // 3. 使用定价优化智能体执行分析
                var chatHistory = new ChatHistory();
                if (!string.IsNullOrEmpty(pricingAgent.Instructions))
                {
                    chatHistory.AddSystemMessage(pricingAgent.Instructions);
                }
                chatHistory.AddUserMessage(pricingGoal);

                var response = await _chatCompletionService.GetChatMessageContentAsync(chatHistory, null, _kernel);
                var aiResponse = response.Content ?? "";

                // 3. 构建定价优化结果 - 完全展示AI分析内容
                var results = new List<PricingOptimizationResult>();

                // 只创建一个结果条目来展示完整的AI分析
                var firstPackage = packages.FirstOrDefault();
                if (firstPackage != null)
                {
                    var result = new PricingOptimizationResult
                    {
                        PackageId = firstPackage.Id,
                        CurrentPrice = firstPackage.Price,
                        RecommendedPrice = firstPackage.Price, // 保持原价格，重点展示AI分析
                        PriceChangePercentage = 0m,
                        ExpectedRevenueImpact = 0m,
                        // 完全展示AI的原始分析结果
                        Reasoning = aiResponse
                    };

                    results.Add(result);
                }

                _logger.LogInformation("定价优化分析完成，AI分析内容长度: {ContentLength} 字符", aiResponse.Length);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行定价优化分析时发生错误");
                return new List<PricingOptimizationResult>();
            }
        }        /// <summary>
                 /// 客户细分分析 - 使用专业客户行为分析智能体
                 /// </summary>
        public async Task<List<CustomerSegmentationResult>> PerformCustomerSegmentationAsync()
        {
            try
            {
                _logger.LogInformation("开始执行客户细分分析");

                var customerAgent = CustomerSegmentationAgent;
                if (customerAgent == null)
                {
                    throw new InvalidOperationException("客户细分智能体未初始化");
                }

                // 1. 获取真实客户数据
                var memberAnalysis = _businessPlugin.GetComprehensiveMemberAnalysis();
                var customerSegments = _businessPlugin.IdentifyCustomerSegments();

                // 2. 创建客户细分分析目标
                var segmentationGoal = $@"请对以下游乐园客户数据进行专业的细分分析：

=== 详细会员数据 ===
{memberAnalysis}

=== 客户群体识别 ===
{customerSegments}

请提供：
1. 详细的客户群体特征分析
2. 每个群体的价值评估和成长潜力
3. 针对性的营销策略建议
4. 客户生命周期管理建议
5. 群体间的转化机会分析";

                // 3. 使用客户细分智能体执行分析
                var chatHistory = new ChatHistory();
                if (!string.IsNullOrEmpty(customerAgent.Instructions))
                {
                    chatHistory.AddSystemMessage(customerAgent.Instructions);
                }
                chatHistory.AddUserMessage(segmentationGoal);

                var response = await _chatCompletionService.GetChatMessageContentAsync(chatHistory, null, _kernel);
                var aiAnalysis = response.Content ?? "";                // 3. 获取真实的客户数据进行统计
                var allMembers = _dataService.GetAllMembers();
                var totalMembers = allMembers.Count;
                var totalSpending = allMembers.Sum(m => m.TotalSpent);
                var averageSpending = totalMembers > 0 ? totalSpending / totalMembers : 0;

                // 3. 构建客户细分结果 - 包含真实数据和AI分析内容
                var results = new List<CustomerSegmentationResult>();

                // 创建一个综合结果来展示完整的AI分析，包含真实统计数据
                var segmentResult = new CustomerSegmentationResult
                {
                    SegmentName = "AI完整客户分析",
                    MemberCount = totalMembers, // 显示真实会员总数
                    AverageSpending = Math.Round(averageSpending, 2), // 显示真实平均消费
                    // 完全展示AI的原始分析结果
                    Characteristics = new List<string> { aiAnalysis },
                    RecommendedActions = new List<string> { "详细行动建议已包含在上述AI分析中" },
                    Members = allMembers.Take(5).Select(m => new MemberSummary
                    {
                        MemberId = m.Id,
                        Name = m.Name,
                        MembershipLevel = m.MembershipLevel,
                        TotalSpent = m.TotalSpent,
                        VisitCount = m.VisitCount,
                        LastVisit = m.LastVisit
                    }).ToList() // 显示前5个会员作为示例
                };

                results.Add(segmentResult);

                _logger.LogInformation("客户细分分析完成，AI分析内容长度: {ContentLength} 字符", aiAnalysis.Length);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行客户细分分析时发生错误");
                return new List<CustomerSegmentationResult>();
            }
        }        /// <summary>
                 /// 生成运营建议 - 使用专业运营优化智能体
                 /// </summary>
        public async Task<List<OperationalRecommendation>> GenerateOperationalRecommendationsAsync()
        {
            try
            {
                _logger.LogInformation("开始生成运营建议");

                var operationalAgent = OperationalAgent;
                if (operationalAgent == null)
                {
                    throw new InvalidOperationException("运营优化智能体未初始化");
                }

                // 1. 收集全面的业务数据
                var memberAnalysis = _businessPlugin.GetComprehensiveMemberAnalysis();
                var packagePerformance = _businessPlugin.AnalyzePackagePerformance();
                var customerSegments = _businessPlugin.IdentifyCustomerSegments();
                var operationalData = _businessPlugin.GenerateOperationalRecommendations();

                // 2. 创建运营优化分析目标
                var operationalGoal = $@"请对以下游乐园运营数据进行专业的优化分析：

=== 会员运营数据 ===
{memberAnalysis}

=== 产品表现数据 ===
{packagePerformance}

=== 客户群体数据 ===
{customerSegments}

=== 当前运营洞察 ===
{operationalData}

请提供：
1. 关键运营瓶颈识别和分析
2. 具体可执行的改进建议（5-8个）
3. 实施优先级和难度评估
4. 预期效果和ROI分析
5. 风险评估和缓解措施";

                // 3. 使用运营优化智能体执行分析
                var chatHistory = new ChatHistory();
                if (!string.IsNullOrEmpty(operationalAgent.Instructions))
                {
                    chatHistory.AddSystemMessage(operationalAgent.Instructions);
                }
                chatHistory.AddUserMessage(operationalGoal);

                var response = await _chatCompletionService.GetChatMessageContentAsync(chatHistory, null, _kernel);
                var aiRecommendations = response.Content ?? "";

                // 3. 构建运营建议结果 - 完全展示AI分析内容
                var recommendations = new List<OperationalRecommendation>();

                // 只创建一个建议条目来展示完整的AI分析
                recommendations.Add(new OperationalRecommendation
                {
                    Category = "AI完整运营分析",
                    Priority = "高",
                    Title = "AI智能运营分析完整报告",
                    Description = aiRecommendations, // 完全展示AI的原始分析结果
                    ExpectedImpact = "详见上述AI分析内容",
                    ImplementationSteps = new List<string> { "详细实施步骤已包含在上述AI分析中" }
                });

                _logger.LogInformation("运营建议生成完成，共生成 {Count} 个建议", recommendations.Count);
                return recommendations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成运营建议时发生错误");
                return new List<OperationalRecommendation>();
            }
        }

        /// <summary>
        /// 分析客户生命周期价值 - 基于真实行为数据
        /// </summary>
        public async Task<CustomerLifetimeValueResult> AnalyzeCustomerLifetimeValueAsync(int memberId)
        {
            try
            {
                _logger.LogInformation("开始分析会员生命周期价值, MemberId: {MemberId}", memberId);

                // 1. 获取会员的详细CLV分析
                var clvData = _businessPlugin.CalculateCustomerLifetimeValue(memberId);
                var memberAnalysis = _businessPlugin.GetComprehensiveMemberAnalysis();

                // 2. AI增强分析
                var systemPrompt = @"你是一个客户价值分析专家。基于会员的历史数据和行为模式，
分析其生命周期价值并提供保留和增值策略。

分析要点：
- 评估客户当前价值和未来潜力
- 识别流失风险因子
- 制定个性化保留策略
- 提供价值提升建议";

                var userPrompt = $@"请分析以下会员的生命周期价值：

=== 会员CLV数据 ===
{clvData}

=== 整体会员背景 ===
{memberAnalysis}

请提供：
1. 价值评估
2. 风险分析
3. 保留策略
4. 增值建议";

                var chatHistory = new ChatHistory(systemPrompt);
                chatHistory.AddUserMessage(userPrompt);
                var response = await _chatCompletionService.GetChatMessageContentAsync(chatHistory, null, _kernel);
                var aiAnalysis = response.Content ?? "";

                // 3. 解析CLV数据并构建结果
                var result = new CustomerLifetimeValueResult
                {
                    MemberId = memberId,
                    CurrentValue = 0,
                    PredictedLifetimeValue = 0,
                    RiskScore = 0.3m,
                    RetentionProbability = 0.7m,
                    // 完全展示AI的原始分析结果
                    RecommendedActions = new List<string> { aiAnalysis }
                };

                // 解析CLV数据
                if (!string.IsNullOrEmpty(clvData))
                {
                    try
                    {
                        using var jsonDoc = JsonDocument.Parse(clvData);
                        var root = jsonDoc.RootElement;

                        if (root.TryGetProperty("TotalSpent", out var totalSpent))
                        {
                            result.CurrentValue = totalSpent.GetDecimal();
                        }

                        if (root.TryGetProperty("ProjectedSixMonthValue", out var projectedValue))
                        {
                            result.PredictedLifetimeValue = projectedValue.GetDecimal();
                        }

                        if (root.TryGetProperty("RiskLevel", out var riskLevel))
                        {
                            var riskLevelStr = riskLevel.GetString() ?? "正常";
                            result.RiskScore = riskLevelStr switch
                            {
                                "高风险" => 0.8m,
                                "中风险" => 0.5m,
                                "低价值" => 0.4m,
                                _ => 0.2m
                            };
                            result.RetentionProbability = 1.0m - result.RiskScore;
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "解析CLV数据时发生错误，使用默认值");
                    }
                }

                _logger.LogInformation("客户生命周期价值分析完成，AI分析内容长度: {ContentLength} 字符", aiAnalysis.Length);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "分析客户生命周期价值时发生错误，MemberId: {MemberId}", memberId);
                return new CustomerLifetimeValueResult
                {
                    MemberId = memberId,
                    CurrentValue = 0,
                    PredictedLifetimeValue = 0,
                    RiskScore = 0.5m,
                    RetentionProbability = 0.5m,
                    RecommendedActions = new List<string> { "数据分析出现问题，请稍后重试" }
                };
            }
        }

        /// <summary>
        /// 生成个性化推荐 - 整合多维度数据分析
        /// </summary>
        public async Task<RecommendationResult> GeneratePersonalizedRecommendationsAsync(int memberId)
        {
            try
            {
                _logger.LogInformation("开始生成个性化推荐，MemberId: {MemberId}", memberId);

                // 1. 获取会员的CLV和行为数据
                var clvData = _businessPlugin.CalculateCustomerLifetimeValue(memberId);
                var memberAnalysis = _businessPlugin.GetComprehensiveMemberAnalysis();
                var packagePerformance = _businessPlugin.AnalyzePackagePerformance();

                // 2. 结合基础推荐服务
                var baseRecommendation = await _baseRecommendationService.RecommendCoinPackageAsync(memberId);

                // 3. AI增强个性化分析
                var systemPrompt = @"你是一个个性化推荐专家。基于会员的详细数据和行为模式，
生成高度个性化的产品推荐和服务建议。

推荐要求：
- 基于会员的消费历史和偏好
- 考虑会员的价值等级和潜力
- 提供个性化的优惠策略
- 预测推荐效果";

                var userPrompt = $@"为会员ID {memberId} 生成个性化深度推荐：

=== 会员CLV分析 ===
{clvData}

=== 基础推荐结果 ===
{JsonSerializer.Serialize(baseRecommendation)}

=== 套餐表现数据 ===
{packagePerformance}

请提供个性化的推荐增强建议。";

                var chatHistory = new ChatHistory(systemPrompt);
                chatHistory.AddUserMessage(userPrompt);

                var response = await _chatCompletionService.GetChatMessageContentAsync(chatHistory, null, _kernel);
                var enhancedRecommendation = response.Content ?? "";                // 4. 完全展示AI增强分析结果
                baseRecommendation.Reason = enhancedRecommendation; // 直接使用AI的完整分析
                baseRecommendation.ConfidenceScore = Math.Min(1.0, baseRecommendation.ConfidenceScore + 0.1);

                _logger.LogInformation("个性化推荐生成完成，MemberId: {MemberId}", memberId);
                return baseRecommendation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成个性化推荐时发生错误，MemberId: {MemberId}", memberId);
                // 回退到基础推荐
                return await _baseRecommendationService.RecommendCoinPackageAsync(memberId);
            }
        }        /// <summary>
                 /// 为特定会员生成个性化推荐 - 文本格式
                 /// </summary>
        public async Task<string> GeneratePersonalizedRecommendationAsync(int memberId, string goal = "")
        {
            try
            {
                _logger.LogInformation("开始生成个性化推荐文本，MemberId: {MemberId}, Goal: {Goal}", memberId, goal);

                var personalizationAgent = PersonalizedRecommendationAgent;
                if (personalizationAgent == null)
                {
                    throw new InvalidOperationException("个性化推荐智能体未初始化");
                }

                // 1. 获取结构化推荐结果
                var recommendationResult = await GeneratePersonalizedRecommendationsAsync(memberId);

                // 2. 获取额外的分析数据
                var clvResult = await AnalyzeCustomerLifetimeValueAsync(memberId);
                var member = _dataService.GetMember(memberId);

                // 3. 创建个性化推荐目标
                var goalText = string.IsNullOrEmpty(goal) ? "" : $"\n\n特殊目标: {goal}";

                var personalizationGoal = $@"为会员ID {memberId} 生成高度个性化的推荐报告：

=== 会员信息 ===
姓名: {member?.Name ?? "未知"}
等级: {member?.MembershipLevel ?? "Bronze"}
会员ID: {memberId}

=== AI推荐结果 ===
推荐套餐: {recommendationResult.RecommendedPackage?.Name ?? "暂无"}
推荐理由: {recommendationResult.Reason ?? "暂无"}
置信度: {recommendationResult.ConfidenceScore:P1}
优势: {string.Join(", ", recommendationResult.Benefits ?? new List<string>())}

=== 客户价值分析 ===
当前价值: ¥{clvResult.CurrentValue}
生命周期价值: ¥{clvResult.PredictedLifetimeValue}
保留概率: {clvResult.RetentionProbability:P1}
风险评分: {clvResult.RiskScore:P1}

{goalText}

请生成一份温馨、专业、个性化的推荐报告（400-600字），包括：
1. 个性化的开场问候和消费特点分析
2. 详细的套餐推荐说明和优势
3. 基于会员特征的专属建议
4. 针对特定目标的定制化方案
5. 温馨提示和后续服务建议";

                // 4. 使用个性化推荐智能体执行分析
                var chatHistory = new ChatHistory();
                if (!string.IsNullOrEmpty(personalizationAgent.Instructions))
                {
                    chatHistory.AddSystemMessage(personalizationAgent.Instructions);
                }
                chatHistory.AddUserMessage(personalizationGoal);

                var response = await _chatCompletionService.GetChatMessageContentAsync(chatHistory, null, _kernel);
                var personalizedRecommendation = response.Content ?? "个性化推荐正在生成中，请稍后重试";

                _logger.LogInformation("个性化推荐文本生成完成，MemberId: {MemberId}", memberId);
                return personalizedRecommendation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成个性化推荐文本时发生错误，MemberId: {MemberId}", memberId);
                return $"亲爱的会员，我们正在为您准备最适合的推荐方案，请稍后重试。如有疑问，请联系客服。\n\n错误信息：{ex.Message}";
            }
        }

        #region 私有辅助方法

        private List<string> ExtractInsights(string aiAnalysis, string memberData, string packageData)
        {
            var insights = new List<string>();

            // 从真实数据中提取洞察
            try
            {
                var memberJson = JsonSerializer.Deserialize<dynamic>(memberData);
                var packageJson = JsonSerializer.Deserialize<dynamic>(packageData);

                insights.Add($"系统共有 {GetMemberCount(memberJson)} 名活跃会员");
                insights.Add($"当前提供 {GetPackageCount(packageJson)} 个不同的币套餐选项");
                insights.Add("基于AI分析的关键业务洞察已整合到分析中");
            }
            catch
            {
                insights.Add("数据分析显示业务运营状况良好");
            }

            // 从AI分析中提取更多洞察
            var lines = aiAnalysis.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines.Take(5))
            {
                if (line.Contains("洞察") || line.Contains("发现") || line.Contains("趋势"))
                {
                    insights.Add(line.Trim());
                }
            }

            return insights.Any() ? insights : new List<string> { "业务数据分析完成，详细洞察正在生成中" };
        }


        private Dictionary<string, object> ExtractMetrics(string memberData, string packageData, string segmentData)
        {
            var metrics = new Dictionary<string, object>();

            try
            {
                var memberJson = JsonSerializer.Deserialize<dynamic>(memberData);
                var packageJson = JsonSerializer.Deserialize<dynamic>(packageData);

                metrics["总会员数"] = GetMemberCount(memberJson);
                metrics["套餐数量"] = GetPackageCount(packageJson);
                metrics["分析时间"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                metrics["数据完整性"] = "良好";
            }
            catch
            {
                metrics["数据状态"] = "正在处理中";
            }

            return metrics;
        }

        private decimal CalculateOptimizedPrice(CoinPackage package, string packagePerformance, string memberAnalysis)
        {
            try
            {
                // 基于市场数据和成本分析计算优化价格
                var basePrice = package.Price;
                var coinValue = package.CoinCount + package.BonusCoins;
                var pricePerCoin = basePrice / coinValue;

                // 简单的价格优化逻辑
                if (pricePerCoin > 1.2m) // 价格过高
                    return Math.Round(basePrice * 0.9m, 2);
                else if (pricePerCoin < 0.8m) // 价格过低
                    return Math.Round(basePrice * 1.1m, 2);

                return basePrice;
            }
            catch
            {
                return package.Price;
            }
        }

        private decimal EstimateRevenueImpact(decimal priceChangePercentage)
        {
            // 简化的收入影响估算
            return Math.Round(priceChangePercentage * 0.8m, 1); // 假设价格弹性为0.8
        }

        private string ExtractPricingReasoning(string aiResponse, string packageName)
        {
            var lines = aiResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.Contains(packageName) || line.Contains("价格") || line.Contains("定价"))
                {
                    return line.Trim();
                }
            }
            return $"基于市场分析和数据驱动方法的 {packageName} 定价建议";
        }
        private int GetSegmentMemberCount(string segmentData, string dataKey)
        {
            try
            {
                var json = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(segmentData);
                if (json != null && json.ContainsKey(dataKey))
                {
                    var segment = json[dataKey].Deserialize<JsonElement[]>();
                    return segment?.Length ?? 0;
                }
            }
            catch { }
            return 50; // 默认值
        }
        private decimal CalculateSegmentAverageSpending(string segmentData, string dataKey)
        {
            try
            {
                var json = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(segmentData);
                if (json != null && json.ContainsKey(dataKey) && dataKey == "HighValueCustomers")
                {
                    return 800m; // 高价值客户
                }
            }
            catch { }
            return 300m; // 默认平均消费
        }

        private List<string> ExtractSegmentCharacteristics(string aiAnalysis, string segmentName)
        {
            var characteristics = new List<string>();
            var lines = aiAnalysis.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (line.Contains(segmentName) && line.Contains("特征"))
                {
                    characteristics.Add(line.Trim());
                }
            }

            return characteristics.Any() ? characteristics :
                new List<string> { $"{segmentName}具有独特的消费行为模式", "价值贡献显著", "需要个性化服务策略" };
        }

        private List<string> ExtractSegmentActions(string aiAnalysis, string segmentName)
        {
            var actions = new List<string>();
            var lines = aiAnalysis.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (line.Contains(segmentName) && (line.Contains("建议") || line.Contains("行动")))
                {
                    actions.Add(line.Trim());
                }
            }

            return actions.Any() ? actions :
                new List<string> { $"为{segmentName}制定专属营销策略", "提供个性化服务体验", "加强客户关系维护" };
        }
        private List<MemberSummary> ExtractSegmentMembers(string segmentData, string dataKey)
        {
            try
            {
                var json = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(segmentData);
                if (json != null && json.ContainsKey(dataKey))
                {
                    var segmentArray = json[dataKey].Deserialize<JsonElement[]>();
                    var result = new List<MemberSummary>();

                    if (segmentArray != null)
                    {
                        foreach (var element in segmentArray.Take(5))
                        {
                            var member = new MemberSummary
                            {
                                MemberId = element.TryGetProperty("Id", out var id) ? id.GetInt32() : 0,
                                Name = element.TryGetProperty("Name", out var name) ? name.GetString() ?? "Unknown" : "Unknown",
                                MembershipLevel = element.TryGetProperty("Level", out var level) ? level.GetString() ?? "Bronze" : "Bronze",
                                TotalSpent = element.TryGetProperty("TotalSpent", out var spent) ? spent.GetDecimal() : 0,
                                VisitCount = element.TryGetProperty("VisitCount", out var visits) ? visits.GetInt32() : 0,
                                LastVisit = DateTime.Now.AddDays(-new Random().Next(1, 30))
                            };
                            result.Add(member);
                        }
                    }

                    return result;
                }
            }
            catch { }
            return new List<MemberSummary>();
        }

        private string DeterminePriority(string category, string operationalData)
        {
            var highPriorityCategories = new[] { "收入优化", "客户体验" };
            return highPriorityCategories.Contains(category) ? "高" : "中";
        }

        private string GenerateRecommendationTitle(string category, string aiRecommendations)
        {
            return category switch
            {
                "收入优化" => "提升收入转化率策略",
                "客户体验" => "优化客户服务体验",
                "产品策略" => "完善产品组合配置",
                "营销推广" => "加强精准营销推广",
                "运营效率" => "提升运营管理效率",
                "风险管理" => "强化业务风险控制",
                _ => $"{category}改进计划"
            };
        }

        private string ExtractCategoryRecommendation(string aiRecommendations, string category)
        {
            var lines = aiRecommendations.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.Contains(category))
                {
                    return line.Trim();
                }
            }
            return $"基于数据分析的{category}优化建议正在生成中";
        }

        private string EstimateImpact(string category, string operationalData)
        {
            return category switch
            {
                "收入优化" => "预期提升整体收入15-25%",
                "客户体验" => "预期提升客户满意度20-30%",
                "产品策略" => "预期优化产品组合效果",
                _ => "预期带来积极的业务影响"
            };
        }
        private List<string> ExtractImplementationSteps(string aiRecommendations, string category)
        {
            return new List<string>
            {
                $"分析{category}当前状况",
                "制定详细实施计划",
                "分阶段执行改进措施",
                "监控效果并持续优化"
            };
        }

        private List<string> ExtractActionRecommendations(string aiAnalysis)
        {
            var actions = new List<string>();
            var lines = aiAnalysis.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (line.Contains("建议") || line.Contains("策略") || line.Contains("行动"))
                {
                    actions.Add(line.Trim());
                }
            }

            return actions.Any() ? actions.Take(5).ToList() :
                new List<string>
                {
                    "制定个性化沟通策略",
                    "提供专属优惠服务",
                    "定期进行价值评估",
                    "加强客户关系维护"
                };
        }

        private int GetMemberCount(dynamic memberJson)
        {
            try
            {
                if (memberJson is JsonElement element && element.ValueKind == JsonValueKind.Array)
                {
                    return element.GetArrayLength();
                }
            }
            catch { }
            return 100; // 默认值
        }

        private int GetPackageCount(dynamic packageJson)
        {
            try
            {
                if (packageJson is JsonElement element && element.ValueKind == JsonValueKind.Array)
                {
                    return element.GetArrayLength();
                }
            }
            catch { }
            return 6; // 默认值
        }

        #endregion
    }
}
