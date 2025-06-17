using System.ComponentModel.DataAnnotations;

namespace AmusementParkRecommendationSystem.Models;

/// <summary>
/// 门店位置信息模型
/// </summary>
public class StoreLocation
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Address { get; set; } = string.Empty;
    
    /// <summary>
    /// 纬度 (WGS84坐标系)
    /// </summary>
    public double Latitude { get; set; }
    
    /// <summary>
    /// 经度 (WGS84坐标系)
    /// </summary>
    public double Longitude { get; set; }
    
    /// <summary>
    /// 门店特色项目
    /// </summary>
    public List<string> Features { get; set; } = new();
    
    /// <summary>
    /// 营业时间
    /// </summary>
    public string BusinessHours { get; set; } = string.Empty;
    
    /// <summary>
    /// 联系电话
    /// </summary>
    public string ContactPhone { get; set; } = string.Empty;
    
    /// <summary>
    /// 门店照片URL
    /// </summary>
    public List<string> PhotoUrls { get; set; } = new();
    
    /// <summary>
    /// 门店评分 (1-5)
    /// </summary>
    public double Rating { get; set; } = 4.5;
    
    /// <summary>
    /// 门店描述
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// 客户位置偏好模型
/// </summary>
public class CustomerLocationPreference
{
    public int CustomerId { get; set; }
    public int MemberId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    
    /// <summary>
    /// 当前位置纬度
    /// </summary>
    public double CurrentLatitude { get; set; }
    
    /// <summary>
    /// 当前位置经度
    /// </summary>
    public double CurrentLongitude { get; set; }
    
    /// <summary>
    /// 客户偏好纬度
    /// </summary>
    public double? PreferredLatitude { get; set; }
    
    /// <summary>
    /// 客户偏好经度
    /// </summary>
    public double? PreferredLongitude { get; set; }
    
    /// <summary>
    /// 偏好区域名称
    /// </summary>
    public string? PreferredArea { get; set; }
    
    /// <summary>
    /// 偏好类别
    /// </summary>
    public List<string> PreferredCategories { get; set; } = new();
    
    /// <summary>
    /// 偏好范围半径（米）
    /// </summary>
    public double PreferredRadius { get; set; } = 5000;
    
    /// <summary>
    /// 偏好访问时间
    /// </summary>
    public DateTime? PreferredVisitTime { get; set; }
    
    /// <summary>
    /// 饮食偏好
    /// </summary>
    public List<string> FoodPreferences { get; set; } = new();
    
    /// <summary>
    /// 活动偏好
    /// </summary>
    public List<string> ActivityPreferences { get; set; } = new();
    
    /// <summary>
    /// 交通方式偏好
    /// </summary>
    public List<string> TransportPreferences { get; set; } = new();
    
    /// <summary>
    /// 预算范围 (元)
    /// </summary>
    public decimal? BudgetRange { get; set; }
    
    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.Now;
}

/// <summary>
/// POI (兴趣点) 信息
/// </summary>
public class PointOfInterest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // 类别：餐厅、景点、游乐设施等
    public string Type { get; set; } = string.Empty; // 餐厅、景点、交通等
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Rating { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public decimal AveragePrice { get; set; }
    public string OpeningHours { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public double DistanceFromStore { get; set; } // 距离门店的距离(公里)
}

/// <summary>
/// 出行方案模型
/// </summary>
public class TripPlan
{
    public string PlanId { get; set; } = Guid.NewGuid().ToString();
    public string PlanTitle { get; set; } = string.Empty;
    public DateTime PlanDate { get; set; }
    public int MemberId { get; set; }
    public StoreLocation TargetStore { get; set; } = new();
    public CustomerLocation CustomerLocation { get; set; } = new();
    public List<TransportationPlan> Transportation { get; set; } = new();
    public List<DiningOption> RecommendedRestaurants { get; set; } = new();
    public List<AttractionOption> NearbyAttractions { get; set; } = new();
    public List<ActivityTimeSlot> TimeSchedule { get; set; } = new();
    public WeatherInfo WeatherForecast { get; set; } = new();
    public string PersonalizedMessage { get; set; } = string.Empty;
    public decimal EstimatedCost { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    /// <summary>
    /// 交通选项
    /// </summary>
    public List<TransportationOption> TransportationOptions { get; set; } = new();
    
    /// <summary>
    /// 天气建议
    /// </summary>
    public WeatherAdvice WeatherAdvice { get; set; } = new();
    
    // 新增缺失的属性
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int TargetStoreId { get; set; }
    public string TargetStoreName { get; set; } = string.Empty;
    public DateTime PlannedDate { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal TotalBudget { get; set; }
    public string WeatherCondition { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<AttractionOption> Attractions { get; set; } = new();
    public List<DiningOption> DiningOptions { get; set; } = new();
    public string AIRecommendations { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    public decimal EstimatedTotalCost { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime? SuggestedStartTime { get; set; }
    public int EstimatedDuration { get; set; } // 预计持续时间(小时)
    public decimal EstimatedBudget { get; set; }
    public List<string> Suggestions { get; set; } = new();
}

/// <summary>
/// 客户位置信息
/// </summary>
public class CustomerLocation
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Address { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
}

/// <summary>
/// 交通方案
/// </summary>
public class TransportationPlan
{
    public string TransportType { get; set; } = string.Empty; // 地铁、公交、出租车、自驾等
    public string Route { get; set; } = string.Empty;
    public int EstimatedDuration { get; set; } // 分钟
    public decimal EstimatedCost { get; set; }
    public List<string> Instructions { get; set; } = new();
    public string MapUrl { get; set; } = string.Empty;
    
    // 新增缺失的属性
    public string FromLocation { get; set; } = string.Empty;
    public string ToLocation { get; set; } = string.Empty;
    public string TransportMode { get; set; } = string.Empty;
    public double Distance { get; set; } // 距离(米)
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public decimal Cost { get; set; }
}

/// <summary>
/// 餐饮选项
/// </summary>
public class DiningOption
{
    public string RestaurantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Cuisine { get; set; } = string.Empty; // 菜系
    public double Rating { get; set; }
    public decimal AveragePrice { get; set; }
    public string Address { get; set; } = string.Empty;
    public double DistanceFromStore { get; set; } // 距离门店距离(公里)
    public List<string> Specialties { get; set; } = new(); // 招牌菜
    public string RecommendationReason { get; set; } = string.Empty;
    
    // 新增缺失的属性
    public string PriceRange { get; set; } = string.Empty; // 价格区间
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string CuisineType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string OpeningHours { get; set; } = string.Empty;
    public int EstimatedWalkingTime { get; set; }
    public List<string> Features { get; set; } = new();
}

/// <summary>
/// 景点选项
/// </summary>
public class AttractionOption
{
    public string AttractionId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // 公园、博物馆、商场等
    public double Rating { get; set; }
    public decimal TicketPrice { get; set; }
    public string Address { get; set; } = string.Empty;
    public double DistanceFromStore { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> Features { get; set; } = new();
    public string RecommendationReason { get; set; } = string.Empty;
    
    // 新增缺失的属性
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Category { get; set; } = string.Empty;
    public string OpeningHours { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int EstimatedVisitDuration { get; set; } // 预计游览时长(分钟)
    public string AttractionType { get; set; } = string.Empty;
    public string AgeRecommendation { get; set; } = string.Empty;
    public int EstimatedWalkingTime { get; set; }
}

/// <summary>
/// 活动时间安排
/// </summary>
public class ActivityTimeSlot
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Activity { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal EstimatedCost { get; set; }
}

/// <summary>
/// 天气信息
/// </summary>
public class WeatherInfo
{
    public DateTime Date { get; set; }
    public string Weather { get; set; } = string.Empty; // 晴、雨、阴等
    public string Condition { get; set; } = string.Empty; // 天气状况
    public int Temperature { get; set; } // 温度
    public int TemperatureMin { get; set; }
    public int TemperatureMax { get; set; }
    public string WindDirection { get; set; } = string.Empty;
    public string WindSpeed { get; set; } = string.Empty;
    public int Humidity { get; set; } // 湿度
    public string Recommendation { get; set; } = string.Empty; // 穿衣建议等
    public int Visibility { get; set; } // 能见度
    public int UVIndex { get; set; } // 紫外线指数
}

/// <summary>
/// 门店推荐结果
/// </summary>
public class StoreRecommendation
{
    public StoreLocation Store { get; set; } = new();
    public double Distance { get; set; } // 距离(公里)
    public int TravelTime { get; set; } // 预计到达时间(分钟)
    public double MatchScore { get; set; } // 匹配分数 (0-1)
    public string RecommendationReason { get; set; } = string.Empty;
    public List<string> Highlights { get; set; } = new(); // 推荐亮点
}

/// <summary>
/// 天气建议模型
/// </summary>
public class WeatherAdvice
{
    public string WeatherCondition { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public string ClothingAdvice { get; set; } = string.Empty;
    public string ActivityAdvice { get; set; } = string.Empty;
    public List<string> Recommendations { get; set; } = new();
    public bool IsGoodForOutdoorActivities { get; set; }
    public string UmbrellaAdvice { get; set; } = string.Empty;
    public string SunscreenAdvice { get; set; } = string.Empty;
    
    // 新增缺失的属性（兼容TripRecommendationService中的使用）
    public string CurrentWeather { get; set; } = string.Empty;
    public int Humidity { get; set; }
    public string WindSpeed { get; set; } = string.Empty;
    public List<string> Suggestions { get; set; } = new();
    public List<string> RecommendedActivities { get; set; } = new();
    public List<string> WarningMessages { get; set; } = new();
}

/// <summary>
/// 交通选项模型
/// </summary>
public class TransportationOption
{
    public string Mode { get; set; } = string.Empty; // 步行、公交、地铁、出租车等
    public int EstimatedTime { get; set; } // 预计时间(分钟)
    public decimal Cost { get; set; } // 费用
    public string Route { get; set; } = string.Empty; // 路线描述
    public List<string> Instructions { get; set; } = new(); // 详细指引
    public double Distance { get; set; } // 距离(米)
    
    // 新增缺失的属性（兼容TripRecommendationService中的使用）
    public string TransportType { get; set; } = string.Empty;
    public int EstimatedDuration { get; set; } // 预计时间(分钟)
    public decimal EstimatedCost { get; set; } // 费用
}

/// <summary>
/// 邀请消息模型
/// </summary>
public class InvitationMessage
{
    /// <summary>
    /// 客户ID
    /// </summary>
    public int CustomerId { get; set; }
    
    /// <summary>
    /// 客户名称
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;
    
    /// <summary>
    /// 门店ID
    /// </summary>
    public int StoreId { get; set; }
    
    /// <summary>
    /// 门店名称
    /// </summary>
    public string StoreName { get; set; } = string.Empty;
    
    /// <summary>
    /// 邀请消息内容
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// 客户到门店的距离(米)
    /// </summary>
    public double Distance { get; set; }
    
    /// <summary>
    /// 门店到客户的距离(米) - 兼容属性
    /// </summary>
    public double DistanceToStore
    {
        get => Distance;
        set => Distance = value;
    }
    
    /// <summary>
    /// 预计响应率(0-1)
    /// </summary>
    public double EstimatedResponseRate { get; set; }
    
    /// <summary>
    /// 发送方式列表
    /// </summary>
    public List<string> DeliveryMethods { get; set; } = new();
    
    /// <summary>
    /// 推荐优惠
    /// </summary>
    public List<string> RecommendedOffers { get; set; } = new();
    
    /// <summary>
    /// 发送时间
    /// </summary>
    public DateTime SendTime { get; set; } = DateTime.Now;
    
    /// <summary>
    /// 有效期至
    /// </summary>
    public DateTime ValidUntil { get; set; }
    
    /// <summary>
    /// 邀请类型
    /// </summary>
    public string InvitationType { get; set; } = string.Empty;
    
    /// <summary>
    /// 客户纬度
    /// </summary>
    public double CustomerLatitude { get; set; }
    
    /// <summary>
    /// 客户经度
    /// </summary>
    public double CustomerLongitude { get; set; }
    
    /// <summary>
    /// AI分析结果
    /// </summary>
    public string AIAnalysis { get; set; } = string.Empty;
    
    /// <summary>
    /// 预计响应概率
    /// </summary>
    public double EstimatedResponseProbability { get; set; }
    
    /// <summary>
    /// 个性化数据
    /// </summary>
    public Dictionary<string, string> PersonalizationData { get; set; } = new();
}


