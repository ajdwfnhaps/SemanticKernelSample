using Microsoft.SemanticKernel;
using System.ComponentModel;
using AmusementParkRecommendationSystem.Services;
using AmusementParkRecommendationSystem.Models;
using System.Text.Json;

namespace AmusementParkRecommendationSystem.Plugins;

/// <summary>
/// 增强的业务分析插件，专为Planner设计
/// </summary>
public class EnhancedBusinessAnalysisPlugin
{
    private readonly DataService _dataService;

    public EnhancedBusinessAnalysisPlugin(DataService dataService)
    {
        _dataService = dataService;
    }

    [KernelFunction]
    [Description("获取所有会员的详细消费分析")]
    public string GetComprehensiveMemberAnalysis()
    {
        var members = _dataService.GetAllMembers();
        var analysis = members.Select(m => new
        {
            MemberId = m.Id,
            Name = m.Name,
            Level = m.MembershipLevel,
            TotalSpent = m.TotalSpent,
            VisitFrequency = m.VisitCount,
            AverageSpending = m.ConsumptionHistory.Any() ? m.ConsumptionHistory.Average(h => h.AmountSpent) : 0,
            LastVisit = m.LastVisit,
            PreferredGames = m.PreferredActivities.Take(3).ToList(),
            RecentConsumption = m.ConsumptionHistory
                .Where(h => h.Date >= DateTime.Now.AddDays(-30))
                .Sum(h => h.AmountSpent)
        });

        return JsonSerializer.Serialize(analysis, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }

    [KernelFunction]
    [Description("分析套餐销售表现和盈利能力")]
    public string AnalyzePackagePerformance()
    {
        var packages = _dataService.GetAvailableCoinPackages();
        var members = _dataService.GetAllMembers();

        var packageAnalysis = packages.Select(p => new
        {
            PackageId = p.Id,
            PackageName = p.Name,
            Price = p.Price,
            CoinValue = p.CoinCount + p.BonusCoins,
            ProfitMargin = CalculateProfitMargin(p),
            EstimatedSales = EstimatePackageSales(p, members),
            TargetLevels = p.TargetMembershipLevels,
            Popularity = CalculatePopularity(p, members),
            PricePerCoin = Math.Round((decimal)p.Price / (p.CoinCount + p.BonusCoins), 2)
        });

        return JsonSerializer.Serialize(packageAnalysis, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }

    [KernelFunction]
    [Description("识别高价值客户和风险客户")]
    public string IdentifyCustomerSegments()
    {
        var members = _dataService.GetAllMembers();
        var now = DateTime.Now;

        var segments = new
        {
            HighValueCustomers = members
                .Where(m => m.TotalSpent > 1000)
                .Select(m => new { m.Id, m.Name, TotalSpent = m.TotalSpent, Level = m.MembershipLevel })
                .OrderByDescending(m => m.TotalSpent)
                .ToList(),

            AtRiskCustomers = members
                .Where(m => m.ConsumptionHistory.Any() &&
                           (now - m.LastVisit).TotalDays > 30)
                .Select(m => new { m.Id, m.Name, DaysSinceLastVisit = (now - m.LastVisit).TotalDays, Level = m.MembershipLevel })
                .OrderByDescending(m => m.DaysSinceLastVisit)
                .ToList(),

            FrequentVisitors = members
                .Where(m => m.VisitCount >= 10)
                .Select(m => new { m.Id, m.Name, VisitCount = m.VisitCount, TotalSpent = m.TotalSpent })
                .OrderByDescending(m => m.VisitCount)
                .ToList(),

            NewCustomers = members
                .Where(m => m.ConsumptionHistory.Any() &&
                           (now - m.RegistrationDate).TotalDays <= 30)
                .Select(m => new { m.Id, m.Name, DaysSinceRegistration = (now - m.RegistrationDate).TotalDays, Level = m.MembershipLevel })
                .OrderBy(m => m.DaysSinceRegistration)
                .ToList()
        };

        return JsonSerializer.Serialize(segments, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }

    [KernelFunction]
    [Description("生成运营优化建议")]
    public string GenerateOperationalRecommendations()
    {
        var members = _dataService.GetAllMembers();
        var packages = _dataService.GetAvailableCoinPackages();

        var recommendations = new List<string>();

        // 分析会员分布
        var levelDistribution = members.GroupBy(m => m.MembershipLevel)
            .ToDictionary(g => g.Key, g => g.Count());

        // 分析消费趋势
        var allConsumption = members.SelectMany(m => m.ConsumptionHistory).ToList();
        var monthlyRevenue = allConsumption
            .GroupBy(h => new { h.Date.Year, h.Date.Month })
            .Select(g => new { Period = $"{g.Key.Year}-{g.Key.Month:D2}", Revenue = g.Sum(h => h.AmountSpent) })
            .OrderBy(x => x.Period)
            .ToList();

        recommendations.Add($"会员等级分布: {string.Join(", ", levelDistribution.Select(kv => $"{kv.Key}: {kv.Value}人"))}");

        if (monthlyRevenue.Any())
        {
            recommendations.Add($"近期月收入趋势: {string.Join(", ", monthlyRevenue.TakeLast(3).Select(r => $"{r.Period}: ¥{r.Revenue}"))}");
        }

        // 生成具体建议
        var totalMembers = members.Count;
        if (levelDistribution.ContainsKey("Bronze") && levelDistribution["Bronze"] > totalMembers * 0.6)
        {
            recommendations.Add("建议: Bronze会员占比过高，应推出升级激励计划");
        }

        if (monthlyRevenue.Count >= 2)
        {
            var lastMonth = monthlyRevenue.TakeLast(1).First().Revenue;
            var secondLastMonth = monthlyRevenue.TakeLast(2).First().Revenue;
            if (lastMonth < secondLastMonth)
            {
                recommendations.Add("建议: 月收入呈下降趋势，应加强营销活动");
            }
        }

        // 分析套餐使用情况
        var avgPackagePrice = packages.Average(p => p.Price);
        var lowPricePackages = packages.Where(p => p.Price < avgPackagePrice * 0.8m).Count();
        if (lowPricePackages < packages.Count() * 0.3)
        {
            recommendations.Add("建议: 增加更多低价位套餐，吸引新用户");
        }

        return JsonSerializer.Serialize(recommendations, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }

    [KernelFunction]
    [Description("计算会员的生命周期价值")]
    public string CalculateCustomerLifetimeValue([Description("会员ID")] int memberId)
    {
        var member = _dataService.GetMember(memberId);
        if (member == null)
            return "会员不存在";

        var membershipDays = (DateTime.Now - member.RegistrationDate).TotalDays;
        var totalSpent = member.TotalSpent;
        var visitCount = member.VisitCount;

        var dailyValue = membershipDays > 0 ? totalSpent / (decimal)membershipDays : 0;
        var avgSpendingPerVisit = visitCount > 0 ? totalSpent / visitCount : 0;

        // 预测未来6个月价值
        var avgVisitsPerMonth = membershipDays > 0 ? visitCount / (membershipDays / 30) : 0;
        var projectedValue = avgSpendingPerVisit * (decimal)avgVisitsPerMonth * 6;

        var clv = new
        {
            MemberId = memberId,
            MemberName = member.Name,
            MembershipLevel = member.MembershipLevel,
            MembershipDays = Math.Round(membershipDays, 0),
            TotalSpent = totalSpent,
            VisitCount = visitCount,
            DailyValue = Math.Round(dailyValue, 2),
            AvgSpendingPerVisit = Math.Round(avgSpendingPerVisit, 2),
            ProjectedSixMonthValue = Math.Round(projectedValue, 2),
            RiskLevel = GetRiskLevel(member)
        };

        return JsonSerializer.Serialize(clv, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }

    private decimal CalculateProfitMargin(CoinPackage package)
    {
        // 假设游戏币成本为0.5元/币
        var costPerCoin = 0.5m;
        var totalCost = (package.CoinCount + package.BonusCoins) * costPerCoin;
        return (package.Price - totalCost) / package.Price * 100;
    }

    private int EstimatePackageSales(CoinPackage package, List<Member> members)
    {
        // 基于会员等级和历史消费估算销量
        var targetMembers = members.Where(m =>
            package.TargetMembershipLevels.Count == 0 ||
            package.TargetMembershipLevels.Contains(m.MembershipLevel)).Count();

        return Math.Max(1, targetMembers / 10); // 简化的估算逻辑
    }

    private double CalculatePopularity(CoinPackage package, List<Member> members)
    {
        // 基于价格区间的流行度估算
        var avgSpending = members
            .SelectMany(m => m.ConsumptionHistory)
            .Where(h => h.AmountSpent > 0)
            .DefaultIfEmpty()
            .Average(h => h?.AmountSpent ?? 0);

        var priceRatio = (double)package.Price / (double)(avgSpending == 0 ? 100 : avgSpending);
        return Math.Max(0, 100 - Math.Abs(priceRatio - 1) * 50); // 越接近平均消费，流行度越高
    }

    private string GetRiskLevel(Member member)
    {
        var daysSinceLastVisit = (DateTime.Now - member.LastVisit).TotalDays;
        var avgSpending = member.ConsumptionHistory.Any() ? member.ConsumptionHistory.Average(h => h.AmountSpent) : 0;

        if (daysSinceLastVisit > 60) return "高风险";
        if (daysSinceLastVisit > 30) return "中风险";
        if (avgSpending < 50) return "低价值";
        return "正常";
    }
}
