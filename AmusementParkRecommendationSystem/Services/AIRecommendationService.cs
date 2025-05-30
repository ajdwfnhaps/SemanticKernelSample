using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using AmusementParkRecommendationSystem.Models;
using AmusementParkRecommendationSystem.Services;
using AmusementParkRecommendationSystem.Plugins;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AmusementParkRecommendationSystem.Services;

/// <summary>
/// AI推荐服务
/// </summary>
public class AIRecommendationService
{
    private readonly Kernel _kernel;
    private readonly DataService _dataService;

    public AIRecommendationService(Kernel kernel, DataService dataService)
    {
        _kernel = kernel;
        _dataService = dataService;
    }

    /// <summary>
    /// 为会员推荐币套餐
    /// </summary>
    public async Task<RecommendationResult> RecommendCoinPackageAsync(int memberId)
    {
        try
        {
            var member = _dataService.GetMember(memberId);
            if (member == null)
            {
                throw new ArgumentException($"会员 {memberId} 不存在");
            }

            // 构建AI推荐提示词
            var prompt = @$"你是一个专业的游乐园币套餐推荐专家。基于以下会员信息和可用套餐，为会员推荐最适合的币套餐。

请按照以下步骤进行分析：

1. 首先获取会员 {memberId} 的详细消费统计信息
2. 获取适合该会员等级的可用币套餐列表
3. 为每个套餐计算适合度评分
4. 基于分析结果选择最佳推荐套餐

推荐标准：
- 性价比：每游戏币的价格要合理
- 使用周期：套餐应该适合会员的消费频率
- 会员等级匹配：套餐应该符合会员的等级权益
- 消费习惯：考虑会员的游戏偏好和消费模式
- 节省金额：计算购买套餐相比零散购买的节省

最后，请提供详细的推荐理由，解释为什么这个套餐最适合该会员，包括具体的好处和潜在节省。

如果有多个不错的选择，也请列出备选套餐。

请用JSON格式返回推荐结果，包含以下字段：
{{
    ""recommendedPackageId"": 推荐套餐ID,
    ""recommendedPackageName"": ""推荐套餐名称"",
    ""confidenceScore"": 推荐置信度(0-100),
    ""reason"": ""详细推荐理由"",
    ""benefits"": [""好处1"", ""好处2"", ""好处3""],
    ""potentialSavings"": 潜在节省金额,
    ""alternativePackageIds"": [备选套餐ID列表]
}}

重要：你可以调用以下函数来获取准确的数据：
- GetMemberConsumptionStats: 获取会员的详细消费统计
- GetAvailableCoinPackages: 获取可用的币套餐列表
- CalculatePackageSuitability: 计算套餐对特定会员的适合度

请务必先调用这些函数获取数据，然后基于实际数据进行推荐。";

            // 启用插件自动调用功能
            var executionSettings = new OpenAIPromptExecutionSettings()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            var response = await _kernel.InvokePromptAsync(prompt, new(executionSettings));
            var result = response.GetValue<string>() ?? "";            // 解析AI响应并构建推荐结果
            return ParseAIResponse(result, memberId);
        }
        catch (Exception)
        {
            // 如果AI推荐失败，使用基于规则的备用推荐
            return GetFallbackRecommendation(memberId);
        }
    }    /// <summary>
         /// 解析AI响应并构建推荐结果
         /// </summary>
    private RecommendationResult ParseAIResponse(string aiResponse, int memberId)
    {
        try
        {
            // 尝试从AI响应中提取JSON
            var jsonStart = aiResponse.IndexOf('{');
            var jsonEnd = aiResponse.LastIndexOf('}');
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonContent = aiResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);

                // 使用配置的JsonSerializer选项
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var aiResult = JsonSerializer.Deserialize<AIRecommendationResponse>(jsonContent, options);

                if (aiResult != null)
                {
                    return BuildRecommendationResult(aiResult, memberId);
                }
            }

            // 如果JSON解析失败，使用备用方案
            return GetFallbackRecommendation(memberId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"解析AI响应时出错: {ex.Message}");
            Console.WriteLine($"AI响应内容: {aiResponse}");
            return GetFallbackRecommendation(memberId);
        }
    }    /// <summary>
         /// 构建最终的推荐结果
         /// </summary>
    private RecommendationResult BuildRecommendationResult(AIRecommendationResponse aiResult, int memberId)
    {
        var member = _dataService.GetMember(memberId)!;
        var packages = _dataService.GetAvailableCoinPackages(member.MembershipLevel);

        var recommendedPackage = packages.FirstOrDefault(p => p.Id == aiResult.RecommendedPackageId);
        var alternativePackages = packages.Where(p => aiResult.AlternativePackageIds.Contains(p.Id)).ToList();

        if (recommendedPackage == null)
        {
            return GetFallbackRecommendation(memberId);
        }

        return new RecommendationResult
        {
            RecommendedPackage = recommendedPackage,
            ConfidenceScore = aiResult.ConfidenceScore,
            Reason = aiResult.Reason,
            Benefits = aiResult.Benefits,
            PotentialSavings = aiResult.PotentialSavings,
            AlternativePackages = alternativePackages
        };
    }

    /// <summary>
    /// 基于规则的备用推荐算法
    /// </summary>
    private RecommendationResult GetFallbackRecommendation(int memberId)
    {
        var member = _dataService.GetMember(memberId);
        if (member == null)
        {
            throw new ArgumentException($"会员 {memberId} 不存在");
        }

        var packages = _dataService.GetAvailableCoinPackages(member.MembershipLevel);

        // 基于简单规则选择推荐套餐
        var recommendedPackage = packages
            .Where(p => p.TargetMembershipLevels.Count == 0 || p.TargetMembershipLevels.Contains(member.MembershipLevel))
            .OrderBy(p => (double)p.Price / (p.CoinCount + p.BonusCoins)) // 按性价比排序
            .First();

        var alternativePackages = packages
            .Where(p => p.Id != recommendedPackage.Id)
            .Take(2)
            .ToList();

        return new RecommendationResult
        {
            RecommendedPackage = recommendedPackage,
            ConfidenceScore = 75.0,
            Reason = $"基于您的会员等级（{member.MembershipLevel}）和消费历史，这个套餐具有最佳的性价比。",
            Benefits = new List<string>
            {
                $"获得 {recommendedPackage.CoinCount + recommendedPackage.BonusCoins} 个游戏币",
                $"享受 {recommendedPackage.DiscountPercentage}% 的会员折扣",
                "适合您的消费水平和游戏习惯"
            },
            PotentialSavings = Math.Round((recommendedPackage.CoinCount + recommendedPackage.BonusCoins) * 0.8m - recommendedPackage.Price, 2),
            AlternativePackages = alternativePackages
        };
    }    /// <summary>
         /// AI推荐响应模型
         /// </summary>
    private class AIRecommendationResponse
    {
        [JsonPropertyName("recommendedPackageId")]
        public int RecommendedPackageId { get; set; }

        [JsonPropertyName("recommendedPackageName")]
        public string RecommendedPackageName { get; set; } = "";

        [JsonPropertyName("confidenceScore")]
        public double ConfidenceScore { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; } = "";

        [JsonPropertyName("benefits")]
        public List<string> Benefits { get; set; } = new();

        [JsonPropertyName("potentialSavings")]
        public decimal PotentialSavings { get; set; }

        [JsonPropertyName("alternativePackageIds")]
        public List<int> AlternativePackageIds { get; set; } = new();
    }
}
