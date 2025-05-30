using AmusementParkRecommendationSystem.Models;

namespace AmusementParkRecommendationSystem.Services;

/// <summary>
/// 数据服务，用于获取和管理业务数据
/// </summary>
public class DataService
{
    private readonly List<Member> _members;
    private readonly List<CoinPackage> _coinPackages;

    public DataService()
    {
        _members = GenerateSampleMembers();
        _coinPackages = GenerateSampleCoinPackages();
    }

    /// <summary>
    /// 获取会员信息
    /// </summary>
    public Member? GetMember(int memberId)
    {
        return _members.FirstOrDefault(m => m.Id == memberId);
    }

    /// <summary>
    /// 获取所有会员
    /// </summary>
    public List<Member> GetAllMembers()
    {
        return _members;
    }

    /// <summary>
    /// 获取可用的币套餐
    /// </summary>
    public List<CoinPackage> GetAvailableCoinPackages(string? membershipLevel = null)
    {
        if (string.IsNullOrEmpty(membershipLevel))
            return _coinPackages;

        return _coinPackages.Where(p => 
            p.TargetMembershipLevels.Count == 0 || 
            p.TargetMembershipLevels.Contains(membershipLevel)
        ).ToList();
    }

    /// <summary>
    /// 获取指定ID的币套餐
    /// </summary>
    public CoinPackage? GetCoinPackage(int packageId)
    {
        return _coinPackages.FirstOrDefault(p => p.Id == packageId);
    }

    /// <summary>
    /// 生成示例会员数据
    /// </summary>
    private List<Member> GenerateSampleMembers()
    {
        var members = new List<Member>();
        var random = new Random();
        var activityTypes = new[] { "射击游戏", "投篮游戏", "娃娃机", "电玩游戏", "赛车游戏", "音乐游戏" };
        var membershipLevels = new[] { "普通", "银卡", "金卡", "VIP" };
        var genders = new[] { "男", "女" };
        var names = new[] { "张三", "李四", "王五", "赵六", "钱七", "孙八", "周九", "吴十" };

        for (int i = 1; i <= 10; i++)
        {
            var member = new Member
            {
                Id = i,
                Name = names[random.Next(names.Length)] + i,
                MembershipLevel = membershipLevels[random.Next(membershipLevels.Length)],
                RegistrationDate = DateTime.Now.AddDays(-random.Next(365, 1000)),
                Age = random.Next(18, 65),
                Gender = genders[random.Next(genders.Length)],
                VisitCount = random.Next(5, 50),
                LastVisit = DateTime.Now.AddDays(-random.Next(1, 30))
            };

            // 生成偏好活动
            var preferredCount = random.Next(2, 4);
            member.PreferredActivities = activityTypes
                .OrderBy(x => random.Next())
                .Take(preferredCount)
                .ToList();

            // 生成消费记录
            for (int j = 0; j < random.Next(10, 30); j++)
            {
                var record = new ConsumptionRecord
                {
                    Id = j + 1,
                    MemberId = member.Id,
                    Date = DateTime.Now.AddDays(-random.Next(1, 180)),
                    ActivityType = activityTypes[random.Next(activityTypes.Length)],
                    CoinsUsed = random.Next(10, 100),
                    AmountSpent = random.Next(20, 200),
                    Location = $"游戏区域{random.Next(1, 6)}",
                    Duration = TimeSpan.FromMinutes(random.Next(15, 120)),
                    Won = random.NextDouble() > 0.4
                };
                member.ConsumptionHistory.Add(record);
            }

            member.TotalSpent = member.ConsumptionHistory.Sum(r => r.AmountSpent);
            members.Add(member);
        }

        return members;
    }

    /// <summary>
    /// 生成示例币套餐数据
    /// </summary>
    private List<CoinPackage> GenerateSampleCoinPackages()
    {
        return new List<CoinPackage>
        {
            new CoinPackage
            {
                Id = 1,
                Name = "新手体验包",
                CoinCount = 50,
                Price = 30,
                BonusCoins = 5,
                DiscountPercentage = 0,
                Description = "适合新手玩家的入门套餐",
                IsLimitedTime = false,
                TargetMembershipLevels = new List<string> { "普通" },
                PackageType = "基础"
            },
            new CoinPackage
            {
                Id = 2,
                Name = "银卡专享包",
                CoinCount = 100,
                Price = 55,
                BonusCoins = 15,
                DiscountPercentage = 8,
                Description = "银卡会员专享优惠套餐",
                IsLimitedTime = false,
                TargetMembershipLevels = new List<string> { "银卡", "金卡", "VIP" },
                PackageType = "进阶"
            },
            new CoinPackage
            {
                Id = 3,
                Name = "金卡豪华包",
                CoinCount = 200,
                Price = 100,
                BonusCoins = 40,
                DiscountPercentage = 15,
                Description = "金卡会员豪华套餐，超值优惠",
                IsLimitedTime = false,
                TargetMembershipLevels = new List<string> { "金卡", "VIP" },
                PackageType = "豪华"
            },
            new CoinPackage
            {
                Id = 4,
                Name = "VIP至尊包",
                CoinCount = 500,
                Price = 220,
                BonusCoins = 120,
                DiscountPercentage = 25,
                Description = "VIP会员至尊套餐，最高性价比",
                IsLimitedTime = false,
                TargetMembershipLevels = new List<string> { "VIP" },
                PackageType = "超值"
            },
            new CoinPackage
            {
                Id = 5,
                Name = "周末狂欢包",
                CoinCount = 150,
                Price = 75,
                BonusCoins = 30,
                DiscountPercentage = 20,
                Description = "周末限时特惠，所有会员可购买",
                IsLimitedTime = true,
                ValidUntil = DateTime.Now.AddDays(7),
                TargetMembershipLevels = new List<string>(),
                PackageType = "限时"
            },
            new CoinPackage
            {
                Id = 6,
                Name = "月度畅玩包",
                CoinCount = 300,
                Price = 150,
                BonusCoins = 60,
                DiscountPercentage = 12,
                Description = "月度大容量套餐，适合频繁游玩",
                IsLimitedTime = false,
                TargetMembershipLevels = new List<string>(),
                PackageType = "大容量"
            }
        };
    }
}
