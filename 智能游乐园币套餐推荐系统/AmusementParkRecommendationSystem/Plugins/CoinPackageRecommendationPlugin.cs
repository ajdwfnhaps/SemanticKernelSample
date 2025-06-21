using Microsoft.SemanticKernel;
using System.ComponentModel;
using AmusementParkRecommendationSystem.Models;
using AmusementParkRecommendationSystem.Services;
using System.Text.Json;

namespace AmusementParkRecommendationSystem.Plugins;

/// <summary>
/// 币套餐推荐插件
/// </summary>
public class CoinPackageRecommendationPlugin
{
    private readonly DataService _dataService;

    public CoinPackageRecommendationPlugin(DataService dataService)
    {
        _dataService = dataService;
    }

    [KernelFunction]
    [Description("获取会员列表信息")]
    public string GetAllMembers()
    {
        var members=_dataService.GetAllMembers();
        if (members == null)
        {
            return "没有会员";
        }

        return JsonSerializer.Serialize(members, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }
    

    [KernelFunction]
    [Description("获取会员的消费统计信息")]
    public string GetMemberConsumptionStats(
        [Description("会员ID")] int memberId)
    {
        var member = _dataService.GetMember(memberId);
        if (member == null)
            return "会员不存在";

        var stats = new
        {
            会员姓名 = member.Name,
            会员等级 = member.MembershipLevel,
            年龄 = member.Age,
            性别 = member.Gender,
            注册日期 = member.RegistrationDate.ToString("yyyy-MM-dd"),
            总消费金额 = member.TotalSpent,
            访问次数 = member.VisitCount,
            最后访问 = member.LastVisit.ToString("yyyy-MM-dd"),
            偏好活动 = string.Join(", ", member.PreferredActivities),
            近30天消费次数 = member.ConsumptionHistory.Count(r => r.Date >= DateTime.Now.AddDays(-30)),
            近30天消费金额 = member.ConsumptionHistory.Where(r => r.Date >= DateTime.Now.AddDays(-30)).Sum(r => r.AmountSpent),
            近30天游戏币使用量 = member.ConsumptionHistory.Where(r => r.Date >= DateTime.Now.AddDays(-30)).Sum(r => r.CoinsUsed),
            平均单次消费 = member.ConsumptionHistory.Count > 0 ? member.ConsumptionHistory.Average(r => r.AmountSpent) : 0,
            最常玩游戏类型 = member.ConsumptionHistory
                .GroupBy(r => r.ActivityType)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key ?? "无",
            获胜率 = member.ConsumptionHistory.Count > 0 ? 
                (double)member.ConsumptionHistory.Count(r => r.Won) / member.ConsumptionHistory.Count * 100 : 0
        };

        return JsonSerializer.Serialize(stats, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }

    [KernelFunction]
    [Description("获取可用的币套餐列表")]
    public string GetAvailableCoinPackages(
        [Description("会员等级，可选参数")] string? membershipLevel = null)
    {
        var packages = _dataService.GetAvailableCoinPackages(membershipLevel);
        var packageInfos = packages.Select(p => new
        {
            套餐ID = p.Id,
            套餐名称 = p.Name,
            游戏币数量 = p.CoinCount,
            价格 = p.Price,
            赠送游戏币 = p.BonusCoins,
            折扣百分比 = p.DiscountPercentage,
            描述 = p.Description,
            套餐类型 = p.PackageType,
            是否限时 = p.IsLimitedTime,
            有效期 = p.ValidUntil?.ToString("yyyy-MM-dd") ?? "无限期",
            目标会员等级 = string.Join(", ", p.TargetMembershipLevels),
            性价比 = Math.Round((double)(p.CoinCount + p.BonusCoins) / (double)p.Price, 2)
        }).ToList();

        return JsonSerializer.Serialize(packageInfos, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }

    [KernelFunction]
    [Description("计算套餐的性价比和适合度")]
    public string CalculatePackageSuitability(
        [Description("会员ID")] int memberId,
        [Description("套餐ID")] int packageId)
    {
        var member = _dataService.GetMember(memberId);
        var packages = _dataService.GetAvailableCoinPackages();
        var package = packages.FirstOrDefault(p => p.Id == packageId);

        if (member == null || package == null)
            return "会员或套餐不存在";        // 计算近30天的消费模式
        var recent30Days = member.ConsumptionHistory.Where(r => r.Date >= DateTime.Now.AddDays(-30)).ToList();
        var avgCoinsPerVisit = recent30Days.Count > 0 ? recent30Days.Sum(r => r.CoinsUsed) / (double)recent30Days.Count : 0;
        var avgSpendingPerVisit = recent30Days.Count > 0 ? (double)recent30Days.Sum(r => r.AmountSpent) / recent30Days.Count : 0;
        
        // 计算套餐适合度
        var totalCoins = package.CoinCount + package.BonusCoins;
        var pricePerCoin = package.Price / (decimal)totalCoins;
        var estimatedUsageDays = avgCoinsPerVisit > 0 ? Math.Ceiling(totalCoins / avgCoinsPerVisit) : 30;
        
        // 计算潜在节省
        var regularPrice = totalCoins * 0.8m; // 假设正常币价比例
        var actualSavings = regularPrice - package.Price;

        var suitability = new
        {
            会员信息 = new { 姓名 = member.Name, 等级 = member.MembershipLevel },
            套餐信息 = new { 名称 = package.Name, 总游戏币 = totalCoins, 价格 = package.Price },
            适合度分析 = new
            {
                每币价格 = Math.Round(pricePerCoin, 2),
                预计使用天数 = estimatedUsageDays,
                与会员等级匹配 = package.TargetMembershipLevels.Count == 0 || package.TargetMembershipLevels.Contains(member.MembershipLevel),
                性价比评分 = Math.Round((double)totalCoins / (double)package.Price * 10, 1),
                潜在节省金额 = Math.Round(actualSavings, 2),
                推荐指数 = CalculateRecommendationScore(member, package)
            },
            消费模式匹配 = new
            {
                近30天平均每次游戏币使用量 = Math.Round(avgCoinsPerVisit, 1),
                近30天平均每次消费金额 = Math.Round(avgSpendingPerVisit, 2),
                套餐是否适合使用频率 = estimatedUsageDays <= 45 ? "适合" : "可能过量"
            }
        };

        return JsonSerializer.Serialize(suitability, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }

    /// <summary>
    /// 计算推荐分数
    /// </summary>
    private double CalculateRecommendationScore(Member member, CoinPackage package)
    {
        double score = 50; // 基础分数

        // 会员等级匹配
        if (package.TargetMembershipLevels.Count == 0 || package.TargetMembershipLevels.Contains(member.MembershipLevel))
            score += 20;

        // 性价比评分
        var totalCoins = package.CoinCount + package.BonusCoins;
        var pricePerCoin = (double)package.Price / totalCoins;
        if (pricePerCoin < 0.5) score += 15;
        else if (pricePerCoin < 0.7) score += 10;
        else if (pricePerCoin < 0.9) score += 5;        // 折扣优势
        score += Math.Min((double)package.DiscountPercentage, 15);

        // 消费习惯匹配
        var recent30Days = member.ConsumptionHistory.Where(r => r.Date >= DateTime.Now.AddDays(-30)).ToList();
        if (recent30Days.Any())
        {
            var avgCoinsPerVisit = recent30Days.Sum(r => r.CoinsUsed) / (double)recent30Days.Count;
            var estimatedUsageDays = totalCoins / avgCoinsPerVisit;
            
            if (estimatedUsageDays >= 15 && estimatedUsageDays <= 30) score += 10;
            else if (estimatedUsageDays >= 7 && estimatedUsageDays <= 45) score += 5;
        }

        return Math.Min(Math.Round(score, 1), 100);
    }
}
