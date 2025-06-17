using System.ComponentModel;

namespace AmusementParkRecommendationSystem.Models;

/// <summary>
/// 会员信息模型
/// </summary>
public class Member
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MembershipLevel { get; set; } = string.Empty; // VIP, 金卡, 银卡, 普通
    public DateTime RegistrationDate { get; set; }
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;
    public List<ConsumptionRecord> ConsumptionHistory { get; set; } = new();
    public decimal TotalSpent { get; set; }
    public int VisitCount { get; set; }
    public DateTime LastVisit { get; set; }
    public List<string> PreferredActivities { get; set; } = new(); // 偏好活动类型
}

/// <summary>
/// 消费记录模型
/// </summary>
public class ConsumptionRecord
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public DateTime Date { get; set; }
    public string ActivityType { get; set; } = string.Empty; // 游戏类型：射击、投篮、娃娃机、电玩等
    public int CoinsUsed { get; set; } // 消耗的游戏币数量
    public decimal AmountSpent { get; set; } // 花费金额
    public string Location { get; set; } = string.Empty; // 游戏区域
    public TimeSpan Duration { get; set; } // 游戏时长
    public bool Won { get; set; } // 是否获胜/获得奖品
}

/// <summary>
/// 币套餐模型
/// </summary>
public class CoinPackage
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CoinCount { get; set; } // 游戏币数量
    public decimal Price { get; set; } // 价格
    public int BonusCoins { get; set; } // 赠送的游戏币
    public decimal DiscountPercentage { get; set; } // 折扣百分比
    public string Description { get; set; } = string.Empty;
    public bool IsLimitedTime { get; set; } // 是否限时优惠
    public DateTime? ValidUntil { get; set; } // 有效期
    public List<string> TargetMembershipLevels { get; set; } = new(); // 目标会员等级
    public string PackageType { get; set; } = string.Empty; // 套餐类型：基础、进阶、豪华、超值
}

/// <summary>
/// 推荐结果模型
/// </summary>
public class RecommendationResult
{
    public CoinPackage RecommendedPackage { get; set; } = new();
    public double ConfidenceScore { get; set; } // 推荐置信度
    public string Reason { get; set; } = string.Empty; // 推荐理由
    public List<string> Benefits { get; set; } = new(); // 购买此套餐的好处
    public decimal PotentialSavings { get; set; } // 潜在节省金额
    public List<CoinPackage> AlternativePackages { get; set; } = new(); // 备选套餐
}
