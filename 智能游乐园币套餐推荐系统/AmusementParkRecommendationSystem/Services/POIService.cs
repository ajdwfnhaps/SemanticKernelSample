using AmusementParkRecommendationSystem.Models;

namespace AmusementParkRecommendationSystem.Services;

/// <summary>
/// 兴趣点服务 - 管理景点、餐厅、商店等POI信息
/// </summary>
public class POIService
{
    private readonly List<PointOfInterest> _pois;

    public POIService()
    {
        _pois = InitializeSamplePOIs();
    }

    /// <summary>
    /// 获取指定位置附近的兴趣点
    /// </summary>
    public async Task<List<PointOfInterest>> GetPOIsAsync(double latitude, double longitude, double radiusMeters, string[]? categories = null)
    {
        var nearbyPOIs = _pois.Where(poi =>
        {
            var distance = CalculateDistance(latitude, longitude, poi.Latitude, poi.Longitude);
            var withinRange = distance <= radiusMeters;
            var matchesCategory = categories == null || categories.Length == 0 || categories.Contains(poi.Category);
            
            return withinRange && matchesCategory;
        }).OrderBy(poi => CalculateDistance(latitude, longitude, poi.Latitude, poi.Longitude))
        .ToList();

        return nearbyPOIs;
    }

    /// <summary>
    /// 根据类别获取POI
    /// </summary>
    public async Task<List<PointOfInterest>> GetPOIsByCategoryAsync(string category)
    {
        return _pois.Where(poi => poi.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    /// <summary>
    /// 获取推荐的餐厅
    /// </summary>
    public async Task<List<DiningOption>> GetRecommendedDiningAsync(double latitude, double longitude, double radiusMeters, string? cuisineType = null)
    {
        var diningPOIs = await GetPOIsAsync(latitude, longitude, radiusMeters, new[] { "餐厅", "快餐", "咖啡厅", "小吃" });
        
        var diningOptions = diningPOIs.Select(poi => new DiningOption
        {
            Name = poi.Name,
            Address = poi.Address,
            Latitude = poi.Latitude,
            Longitude = poi.Longitude,
            CuisineType = GetCuisineType(poi.Name, poi.Description),
            PriceRange = GetPriceRange(poi.Rating),
            Rating = poi.Rating,
            Description = poi.Description,
            OpeningHours = poi.OpeningHours,
            EstimatedWalkingTime = (int)(CalculateDistance(latitude, longitude, poi.Latitude, poi.Longitude) / 83.33), // 5km/h步行速度
            Features = poi.Tags
        }).ToList();

        if (!string.IsNullOrEmpty(cuisineType))
        {
            diningOptions = diningOptions.Where(d => d.CuisineType.Contains(cuisineType, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return diningOptions.OrderByDescending(d => d.Rating).ToList();
    }

    /// <summary>
    /// 获取推荐的景点
    /// </summary>
    public async Task<List<AttractionOption>> GetRecommendedAttractionsAsync(double latitude, double longitude, double radiusMeters, string? attractionType = null)
    {
        var attractionCategories = new[] { "景点", "游乐设施", "主题区域", "表演", "展览" };
        var attractionPOIs = await GetPOIsAsync(latitude, longitude, radiusMeters, attractionCategories);
        
        var attractions = attractionPOIs.Select(poi => new AttractionOption
        {
            Name = poi.Name,
            Address = poi.Address,
            Latitude = poi.Latitude,
            Longitude = poi.Longitude,
            AttractionType = poi.Category,
            Rating = poi.Rating,
            Description = poi.Description,
            EstimatedVisitDuration = GetEstimatedVisitDuration(poi.Category),
            TicketPrice = GetEstimatedTicketPrice(poi.Category, poi.Rating),
            OpeningHours = poi.OpeningHours,
            Features = poi.Tags,
            AgeRecommendation = GetAgeRecommendation(poi.Tags),
            EstimatedWalkingTime = (int)(CalculateDistance(latitude, longitude, poi.Latitude, poi.Longitude) / 83.33)
        }).ToList();

        if (!string.IsNullOrEmpty(attractionType))
        {
            attractions = attractions.Where(a => a.AttractionType.Contains(attractionType, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return attractions.OrderByDescending(a => a.Rating).ToList();
    }

    /// <summary>
    /// 计算两点间距离
    /// </summary>
    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000; // 地球半径（米）
        
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        
        return R * c;
    }

    private double ToRadians(double degrees) => degrees * Math.PI / 180;

    /// <summary>
    /// 根据名称和描述推断菜系类型
    /// </summary>
    private string GetCuisineType(string name, string description)
    {
        var content = $"{name} {description}".ToLower();
        
        if (content.Contains("川菜") || content.Contains("四川") || content.Contains("麻辣"))
            return "川菜";
        if (content.Contains("粤菜") || content.Contains("广东") || content.Contains("港式"))
            return "粤菜";
        if (content.Contains("湘菜") || content.Contains("湖南") || content.Contains("辣"))
            return "湘菜";
        if (content.Contains("东北") || content.Contains("锅包肉"))
            return "东北菜";
        if (content.Contains("西餐") || content.Contains("牛排") || content.Contains("意大利"))
            return "西餐";
        if (content.Contains("日料") || content.Contains("寿司") || content.Contains("日本"))
            return "日料";
        if (content.Contains("韩式") || content.Contains("烤肉") || content.Contains("韩国"))
            return "韩料";
        if (content.Contains("快餐") || content.Contains("汉堡") || content.Contains("肯德基") || content.Contains("麦当劳"))
            return "快餐";
        if (content.Contains("咖啡") || content.Contains("星巴克"))
            return "咖啡";
        if (content.Contains("小吃") || content.Contains("零食"))
            return "小吃";
        
        return "中餐";
    }

    /// <summary>
    /// 根据评分估算价格范围
    /// </summary>
    private string GetPriceRange(double rating)
    {
        return rating switch
        {
            >= 4.5 => "$$$ (80-150元/人)",
            >= 4.0 => "$$ (40-80元/人)",
            >= 3.5 => "$ (20-40元/人)",
            _ => "$ (10-30元/人)"
        };
    }

    /// <summary>
    /// 估算游玩时长
    /// </summary>
    private int GetEstimatedVisitDuration(string category)
    {
        return category switch
        {
            "过山车" or "激流勇进" => 15,
            "游乐设施" => 20,
            "主题区域" => 90,
            "表演" => 45,
            "展览" => 60,
            "景点" => 30,
            _ => 30
        };
    }

    /// <summary>
    /// 估算门票价格
    /// </summary>
    private decimal GetEstimatedTicketPrice(string category, double rating)
    {
        var basePrice = category switch
        {
            "过山车" => 50m,
            "游乐设施" => 30m,
            "主题区域" => 0m, // 通常包含在门票内
            "表演" => 20m,
            "展览" => 40m,
            "景点" => 25m,
            _ => 20m
        };

        // 根据评分调整价格
        var ratingMultiplier = rating switch
        {
            >= 4.5 => 1.2m,
            >= 4.0 => 1.0m,
            >= 3.5 => 0.8m,
            _ => 0.6m
        };

        return basePrice * ratingMultiplier;
    }

    /// <summary>
    /// 获取年龄推荐
    /// </summary>
    private string GetAgeRecommendation(List<string> tags)
    {
        if (tags.Contains("儿童"))
            return "3-12岁";
        if (tags.Contains("刺激") || tags.Contains("过山车"))
            return "14岁以上";
        if (tags.Contains("亲子"))
            return "全年龄";
        if (tags.Contains("温和"))
            return "6岁以上";
        
        return "全年龄";
    }

    /// <summary>
    /// 初始化示例POI数据
    /// </summary>
    private List<PointOfInterest> InitializeSamplePOIs()
    {
        return new List<PointOfInterest>
        {
            // 欢乐谷区域POI
            new()
            {
                Id = 1,
                Name = "极速飞车",
                Category = "过山车",
                Address = "欢乐谷主园区",
                Latitude = 39.866000,
                Longitude = 116.497500,
                Rating = 4.7,
                Description = "高速过山车，体验极限刺激",
                OpeningHours = "09:00-21:00",
                Tags = new() { "刺激", "过山车", "热门" }
            },
            new()
            {
                Id = 2,
                Name = "水上乐园",
                Category = "游乐设施",
                Address = "欢乐谷主园区",
                Latitude = 39.865800,
                Longitude = 116.497200,
                Rating = 4.5,
                Description = "夏日清凉水上项目",
                OpeningHours = "10:00-20:00",
                Tags = new() { "清凉", "夏季", "亲子" }
            },
            new()
            {
                Id = 3,
                Name = "儿童梦幻城堡",
                Category = "主题区域",
                Address = "欢乐谷主园区",
                Latitude = 39.866200,
                Longitude = 116.497800,
                Rating = 4.6,
                Description = "专为儿童设计的梦幻游乐区",
                OpeningHours = "09:00-21:00",
                Tags = new() { "儿童", "温和", "亲子", "梦幻" }
            },
            new()
            {
                Id = 4,
                Name = "快乐餐厅",
                Category = "餐厅",
                Address = "欢乐谷主园区内",
                Latitude = 39.865900,
                Longitude = 116.497300,
                Rating = 4.2,
                Description = "园区内中式快餐，提供各种美食",
                OpeningHours = "09:00-21:00",
                Tags = new() { "中餐", "快餐", "园区内" }
            },
            new()
            {
                Id = 5,
                Name = "星巴克咖啡",
                Category = "咖啡厅",
                Address = "欢乐谷主园区入口",
                Latitude = 39.865700,
                Longitude = 116.497100,
                Rating = 4.4,
                Description = "国际知名咖啡连锁店",
                OpeningHours = "08:00-22:00",
                Tags = new() { "咖啡", "休息", "国际品牌" }
            },

            // 方特区域POI
            new()
            {
                Id = 6,
                Name = "飞越极限",
                Category = "游乐设施",
                Address = "方特欢乐世界",
                Latitude = 39.845300,
                Longitude = 116.287600,
                Rating = 4.8,
                Description = "大型球幕飞行体验项目",
                OpeningHours = "09:30-20:30",
                Tags = new() { "球幕", "飞行", "科技", "热门" }
            },
            new()
            {
                Id = 7,
                Name = "4D影院",
                Category = "表演",
                Address = "方特欢乐世界",
                Latitude = 39.845100,
                Longitude = 116.287400,
                Rating = 4.6,
                Description = "沉浸式4D电影体验",
                OpeningHours = "10:00-20:00",
                Tags = new() { "4D", "电影", "科技", "室内" }
            },
            new()
            {
                Id = 8,
                Name = "方特美食广场",
                Category = "餐厅",
                Address = "方特欢乐世界中心",
                Latitude = 39.845200,
                Longitude = 116.287500,
                Rating = 4.3,
                Description = "集合各地美食的大型美食广场",
                OpeningHours = "09:30-20:30",
                Tags = new() { "美食广场", "多选择", "中餐" }
            },

            // 环球影城区域POI
            new()
            {
                Id = 9,
                Name = "哈利波特魔法世界",
                Category = "主题区域",
                Address = "环球影城主题公园",
                Latitude = 39.863600,
                Longitude = 116.723900,
                Rating = 4.9,
                Description = "完全还原的哈利波特魔法世界",
                OpeningHours = "08:00-22:00",
                Tags = new() { "哈利波特", "魔法", "电影主题", "热门" }
            },
            new()
            {
                Id = 10,
                Name = "变形金刚3D对决",
                Category = "游乐设施",
                Address = "环球影城主题公园",
                Latitude = 39.863800,
                Longitude = 116.724100,
                Rating = 4.8,
                Description = "身临其境的变形金刚战斗体验",
                OpeningHours = "08:00-22:00",
                Tags = new() { "变形金刚", "3D", "刺激", "电影主题" }
            },
            new()
            {
                Id = 11,
                Name = "小黄人乐园",
                Category = "主题区域",
                Address = "环球影城主题公园",
                Latitude = 39.863400,
                Longitude = 116.723700,
                Rating = 4.7,
                Description = "可爱的小黄人主题游乐区",
                OpeningHours = "08:00-22:00",
                Tags = new() { "小黄人", "可爱", "亲子", "电影主题" }
            },
            new()
            {
                Id = 12,
                Name = "环球美食汇",
                Category = "餐厅",
                Address = "环球影城主题公园",
                Latitude = 39.863500,
                Longitude = 116.723800,
                Rating = 4.5,
                Description = "国际美食集合地",
                OpeningHours = "08:00-22:00",
                Tags = new() { "国际美食", "西餐", "高档" }
            },

            // 周边景点和餐厅
            new()
            {
                Id = 13,
                Name = "北京野生动物园",
                Category = "景点",
                Address = "大兴区榆垡镇",
                Latitude = 39.562345,
                Longitude = 116.347890,
                Rating = 4.4,
                Description = "近距离观赏野生动物",
                OpeningHours = "08:30-17:30",
                Tags = new() { "动物", "自然", "教育", "亲子" }
            },
            new()
            {
                Id = 14,
                Name = "海底捞火锅",
                Category = "餐厅",
                Address = "朝阳区东四环附近",
                Latitude = 39.866500,
                Longitude = 116.498000,
                Rating = 4.6,
                Description = "知名火锅连锁店，服务优质",
                OpeningHours = "10:00-02:00",
                Tags = new() { "火锅", "川菜", "连锁", "服务好" }
            },
            new()
            {
                Id = 15,
                Name = "必胜客",
                Category = "餐厅",
                Address = "通州区梨园镇",
                Latitude = 39.863000,
                Longitude = 116.723000,
                Rating = 4.2,
                Description = "意式披萨连锁餐厅",
                OpeningHours = "09:00-23:00",
                Tags = new() { "西餐", "披萨", "连锁", "家庭聚餐" }
            }
        };
    }
}
