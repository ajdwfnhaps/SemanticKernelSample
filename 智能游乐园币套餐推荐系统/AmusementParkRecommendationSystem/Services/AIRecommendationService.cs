using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using AmusementParkRecommendationSystem.Models;
using AmusementParkRecommendationSystem.Services;
using AmusementParkRecommendationSystem.Plugins;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics;
using static AmusementParkRecommendationSystem.TracingConstants;
using static AmusementParkRecommendationSystem.Services.OpenTelemetryExtensions;

namespace AmusementParkRecommendationSystem.Services;

/// <summary>
/// AI推荐服务
/// </summary>
public class AIRecommendationService
{
    private readonly Kernel _kernel;
    private readonly DataService _dataService;
    private readonly SemanticKernelTelemetryService? _telemetryService;

    public AIRecommendationService(Kernel kernel, DataService dataService, SemanticKernelTelemetryService? telemetryService = null)
    {
        _kernel = kernel;
        _dataService = dataService;
        _telemetryService = telemetryService;
    }

    /// <summary>
    /// 为会员推荐币套餐
    /// </summary>
    public async Task<RecommendationResult> RecommendCoinPackageAsync(int memberId)
    {
        using var activity = TracingConstants.ActivitySource.StartActivity("RecommendCoinPackage");
        activity?.SetTag("member.id", memberId);
        
        var stopwatch = Stopwatch.StartNew();
        var success = false;
        
        try
        {
            var member = _dataService.GetMember(memberId);
            if (member == null)
            {
                throw new ArgumentException($"会员 {memberId} 不存在");
            }

            activity?.SetTag("member.name", member.Name);
            activity?.SetTag("member.level", member.MembershipLevel);

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
- 消费习惯：基于历史消费数据推荐适合的套餐规模

请调用相应的插件函数来获取数据并进行分析。";

            var executionSettings = new OpenAIPromptExecutionSettings
            {
                MaxTokens = 2000,
                Temperature = 0.7,
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            var result = await _kernel.InvokePromptAsync(prompt, new(executionSettings));
            var response = result.ToString();

            // 尝试解析AI响应中的JSON
            var aiResult = TryParseAIResponse(response);
            if (aiResult != null)
            {
                var recommendation = BuildRecommendationResult(aiResult, memberId);
                success = true;
                return recommendation;
            }

            // 如果AI响应不包含有效的JSON，使用备用推荐算法
            var fallbackRecommendation = GenerateFallbackRecommendation(memberId);
            success = true;
            return fallbackRecommendation;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            
            // 记录推荐调用的遥测数据
            _telemetryService?.TrackRecommendation("CoinPackage", memberId, stopwatch.Elapsed, success);
            
            activity?.SetTag("recommendation.duration_ms", stopwatch.ElapsedMilliseconds);
            activity?.SetTag("recommendation.success", success);
        }
    }

    /// <summary>
    /// 尝试解析AI响应中的JSON
    /// </summary>
    private AIRecommendationResponse? TryParseAIResponse(string response)
    {
        try
        {
            // 尝试从响应中提取JSON
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}');
            
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonContent = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
                
                var aiResult = JsonSerializer.Deserialize<AIRecommendationResponse>(jsonContent, options);
                return aiResult;
            }
        }
        catch (Exception)
        {
            // 解析失败，返回null
        }
        
        return null;
    }

    /// <summary>
    /// 构建推荐结果
    /// </summary>
    private RecommendationResult BuildRecommendationResult(AIRecommendationResponse aiResult, int memberId)
    {
        var packages = _dataService.GetAvailableCoinPackages();
        var recommendedPackage = packages.FirstOrDefault(p => p.Id == aiResult.RecommendedPackageId);
        
        if (recommendedPackage == null)
        {
            return GenerateFallbackRecommendation(memberId);
        }
        
        var alternativePackages = packages.Where(p => aiResult.AlternativePackageIds.Contains(p.Id)).ToList();
        
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
    /// 生成备用推荐（当AI推荐失败时使用）
    /// </summary>
    private RecommendationResult GenerateFallbackRecommendation(int memberId)
    {
        var member = _dataService.GetMember(memberId);
        var packages = _dataService.GetAvailableCoinPackages();
        
        // 基于会员等级的简单推荐逻辑
        var recommendedPackage = member?.MembershipLevel switch
        {
            "Bronze" => packages.OrderBy(p => p.Price).First(),
            "Silver" => packages.OrderBy(p => Math.Abs(p.Price - 100)).First(),
            "Gold" => packages.OrderBy(p => Math.Abs(p.Price - 200)).First(),
            "Diamond" => packages.OrderByDescending(p => p.Price).First(),
            _ => packages.OrderBy(p => p.Price).First()
        };
        
        var alternativePackages = packages.Where(p => p.Id != recommendedPackage.Id).Take(2).ToList();
        
        return new RecommendationResult
        {
            RecommendedPackage = recommendedPackage,
            ConfidenceScore = 75.0,
            Reason = $"基于您的会员等级（{member?.MembershipLevel}）和消费历史，这个套餐具有最佳的性价比。",
            Benefits = new List<string>
            {
                $"获得 {recommendedPackage.CoinCount + recommendedPackage.BonusCoins} 个游戏币",
                $"享受 {recommendedPackage.DiscountPercentage}% 的会员折扣",
                "适合您的消费水平和游戏习惯"
            },
            PotentialSavings = Math.Round((recommendedPackage.CoinCount + recommendedPackage.BonusCoins) * 0.8m - recommendedPackage.Price, 2),
            AlternativePackages = alternativePackages
        };
    }

    /// <summary>
    /// AI推荐响应模型
    /// </summary>
    public class AIRecommendationResponse
    {
        [JsonPropertyName("recommendedPackageId")]
        public int RecommendedPackageId { get; set; }

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
