using AmusementParkRecommendationSystem.Models;

namespace AmusementParkRecommendationSystem.Services;

/// <summary>
/// 数据服务，用于获取和管理业务数据
/// </summary>
public class DataService
{
    private readonly List<Member> _members;
    private readonly List<CoinPackage> _coinPackages;
    private readonly List<StoreLocation> _storeLocations;
    private readonly List<CustomerLocationPreference> _customerLocations;

    public DataService()
    {
        _members = GenerateSampleMembers();
        _coinPackages = GenerateSampleCoinPackages();
        _storeLocations = GenerateSampleStoreLocations();
        _customerLocations = GenerateSampleCustomerLocations();
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
    /// 获取所有门店位置
    /// </summary>
    public List<StoreLocation> GetAllStoreLocations()
    {
        return _storeLocations;
    }

    /// <summary>
    /// 获取指定ID的门店位置
    /// </summary>
    public StoreLocation? GetStoreLocation(int storeId)
    {
        return _storeLocations.FirstOrDefault(s => s.Id == storeId);
    }

    /// <summary>
    /// 获取所有客户位置偏好
    /// </summary>
    public List<CustomerLocationPreference> GetAllCustomerLocationPreferences()
    {
        return _customerLocations;
    }

    /// <summary>
    /// 获取指定会员的位置偏好
    /// </summary>
    public CustomerLocationPreference? GetCustomerLocationPreference(int memberId)
    {
        return _customerLocations.FirstOrDefault(c => c.MemberId == memberId);
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
    }    /// <summary>
    /// 生成示例门店位置数据
    /// </summary>
    private List<StoreLocation> GenerateSampleStoreLocations()
    {
        return new List<StoreLocation>
        {
            new()
            {
                Id = 1,
                Name = "欢乐谷主园区",
                Address = "北京市朝阳区东四环小武基北路",
                Latitude = 39.865895,
                Longitude = 116.497395,
                Features = new() { "过山车", "水上乐园", "儿童乐园", "主题表演" },
                BusinessHours = "09:00-21:00",
                ContactPhone = "010-67389898",
                PhotoUrls = new() { "/images/stores/happyvalley_main.jpg" },
                Rating = 4.6,
                Description = "北京欢乐谷是国家4A级旅游景区，拥有亚洲最大的游乐设备群"
            },
            new()
            {
                Id = 2,
                Name = "方特欢乐世界",
                Address = "北京市丰台区南四环西路",
                Latitude = 39.845123,
                Longitude = 116.287456,
                Features = new() { "科幻体验", "4D影院", "冒险乐园", "亲子游乐" },
                BusinessHours = "09:30-20:30",
                ContactPhone = "010-83065555",
                PhotoUrls = new() { "/images/stores/fantawild.jpg" },
                Rating = 4.5,
                Description = "方特欢乐世界是一个以高科技为主要表现形式的文化科技主题公园"
            },
            new()
            {
                Id = 3,
                Name = "环球影城主题公园",
                Address = "北京市通州区梨园镇",
                Latitude = 39.863456,
                Longitude = 116.723789,
                Features = new() { "哈利波特", "变形金刚", "小黄人", "电影主题" },
                BusinessHours = "08:00-22:00",
                ContactPhone = "400-890-5168",
                PhotoUrls = new() { "/images/stores/universal.jpg" },
                Rating = 4.8,
                Description = "北京环球影城主题公园是环球影城在亚洲的第三座主题公园"
            },
            new()
            {
                Id = 4,
                Name = "石景山游乐园",
                Address = "北京市石景山区石景山路25号",
                Latitude = 39.914567,
                Longitude = 116.187234,
                Features = new() { "经典游乐", "亲子娱乐", "传统项目", "童话城堡" },
                BusinessHours = "09:00-20:00",
                ContactPhone = "010-68875096",
                PhotoUrls = new() { "/images/stores/shijingshan.jpg" },
                Rating = 4.2,
                Description = "北京石景山游乐园是一座以欧洲园林为主要特色的大型现代化游乐园"
            },
            new()
            {
                Id = 5,
                Name = "北京野生动物园",
                Address = "北京市大兴区榆垡镇万亩森林",
                Latitude = 39.562345,
                Longitude = 116.347890,
                Features = new() { "动物观赏", "自驾游览", "亲子教育", "生态体验" },
                BusinessHours = "08:30-17:30",
                ContactPhone = "010-89216606",
                PhotoUrls = new() { "/images/stores/wildlifezoo.jpg" },
                Rating = 4.4,
                Description = "北京野生动物园是集动物保护、科普教育、科学研究为一体的大型自然生态公园"
            }
        };
    }

    /// <summary>
    /// 生成示例客户位置偏好数据
    /// </summary>
    private List<CustomerLocationPreference> GenerateSampleCustomerLocations()
    {
        var preferences = new List<CustomerLocationPreference>();
        var random = new Random();

        for (int i = 0; i < _members.Count; i++)
        {
            var member = _members[i];
            var preference = new CustomerLocationPreference
            {
                CustomerId = member.Id,
                MemberId = member.Id,
                Name = member.Name,
                Age = random.Next(8, 70),
                CurrentLatitude = 39.9042 + (random.NextDouble() - 0.5) * 0.2, // 北京市中心附近随机位置
                CurrentLongitude = 116.4074 + (random.NextDouble() - 0.5) * 0.2,
                PreferredLatitude = 39.9042 + (random.NextDouble() - 0.5) * 0.1,
                PreferredLongitude = 116.4074 + (random.NextDouble() - 0.5) * 0.1,
                PreferredCategories = GetRandomPreferences(random),
                PreferredRadius = random.Next(1000, 8000),
                PreferredVisitTime = DateTime.Today.AddHours(random.Next(9, 18)),
                FoodPreferences = GetRandomFoodPreferences(random),
                ActivityPreferences = GetRandomActivityPreferences(random),
                TransportPreferences = GetRandomTransportPreferences(random),
                BudgetRange = random.Next(100, 800),
                LastUpdated = DateTime.Now.AddDays(-random.Next(0, 30))
            };

            preferences.Add(preference);
        }

        return preferences;
    }

    private List<string> GetRandomPreferences(Random random)
    {
        var allPreferences = new[] { "过山车", "水上项目", "儿童乐园", "主题表演", "科幻体验", "电影主题", "动物观赏", "亲子活动" };
        var count = random.Next(2, 5);
        return allPreferences.OrderBy(x => random.Next()).Take(count).ToList();
    }

    private List<string> GetRandomFoodPreferences(Random random)
    {
        var allFoods = new[] { "中餐", "西餐", "快餐", "小吃", "甜品", "咖啡", "火锅", "烧烤" };
        var count = random.Next(2, 4);
        return allFoods.OrderBy(x => random.Next()).Take(count).ToList();
    }

    private List<string> GetRandomActivityPreferences(Random random)
    {
        var allActivities = new[] { "刺激项目", "温和项目", "亲子活动", "表演秀", "互动体验", "科技项目", "户外活动", "室内项目" };
        var count = random.Next(2, 4);
        return allActivities.OrderBy(x => random.Next()).Take(count).ToList();
    }

    private List<string> GetRandomTransportPreferences(Random random)
    {
        var allTransports = new[] { "地铁", "公交", "出租车", "自驾", "步行", "共享单车" };
        var count = random.Next(1, 3);
        return allTransports.OrderBy(x => random.Next()).Take(count).ToList();
    }
}
