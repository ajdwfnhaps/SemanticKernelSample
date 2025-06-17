using System.Text.Json.Serialization;

namespace AmusementParkRecommendationSystem.Models;

/// <summary>
/// 业务分析结果
/// </summary>
public class BusinessAnalysisResult
{
    [JsonPropertyName("analysis_type")]
    public string AnalysisType { get; set; } = string.Empty;

    [JsonPropertyName("insights")]
    public List<string> Insights { get; set; } = new();

    [JsonPropertyName("recommendations")]
    public List<string> Recommendations { get; set; } = new();

    [JsonPropertyName("metrics")]
    public Dictionary<string, object> Metrics { get; set; } = new();

    [JsonPropertyName("generated_at")]
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// 定价优化结果
/// </summary>
public class PricingOptimizationResult
{
    [JsonPropertyName("package_id")]
    public int PackageId { get; set; }

    [JsonPropertyName("current_price")]
    public decimal CurrentPrice { get; set; }

    [JsonPropertyName("recommended_price")]
    public decimal RecommendedPrice { get; set; }

    [JsonPropertyName("price_change_percentage")]
    public decimal PriceChangePercentage { get; set; }

    [JsonPropertyName("expected_revenue_impact")]
    public decimal ExpectedRevenueImpact { get; set; }

    [JsonPropertyName("reasoning")]
    public string Reasoning { get; set; } = string.Empty;
}

/// <summary>
/// 客户细分结果
/// </summary>
public class CustomerSegmentationResult
{
    [JsonPropertyName("segment_name")]
    public string SegmentName { get; set; } = string.Empty;

    [JsonPropertyName("member_count")]
    public int MemberCount { get; set; }

    [JsonPropertyName("avg_spending")]
    public decimal AverageSpending { get; set; }

    [JsonPropertyName("characteristics")]
    public List<string> Characteristics { get; set; } = new();

    [JsonPropertyName("recommended_actions")]
    public List<string> RecommendedActions { get; set; } = new();

    [JsonPropertyName("members")]
    public List<MemberSummary> Members { get; set; } = new();
}

/// <summary>
/// 会员摘要信息
/// </summary>
public class MemberSummary
{
    [JsonPropertyName("member_id")]
    public int MemberId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("membership_level")]
    public string MembershipLevel { get; set; } = string.Empty;

    [JsonPropertyName("total_spent")]
    public decimal TotalSpent { get; set; }

    [JsonPropertyName("visit_count")]
    public int VisitCount { get; set; }

    [JsonPropertyName("last_visit")]
    public DateTime LastVisit { get; set; }
}

/// <summary>
/// 运营建议结果
/// </summary>
public class OperationalRecommendation
{
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("priority")]
    public string Priority { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("expected_impact")]
    public string ExpectedImpact { get; set; } = string.Empty;

    [JsonPropertyName("implementation_steps")]
    public List<string> ImplementationSteps { get; set; } = new();
}

/// <summary>
/// 客户生命周期价值结果
/// </summary>
public class CustomerLifetimeValueResult
{
    [JsonPropertyName("member_id")]
    public int MemberId { get; set; }

    [JsonPropertyName("current_value")]
    public decimal CurrentValue { get; set; }

    [JsonPropertyName("predicted_lifetime_value")]
    public decimal PredictedLifetimeValue { get; set; }

    [JsonPropertyName("risk_score")]
    public decimal RiskScore { get; set; }

    [JsonPropertyName("retention_probability")]
    public decimal RetentionProbability { get; set; }

    [JsonPropertyName("recommended_actions")]
    public List<string> RecommendedActions { get; set; } = new();
}
