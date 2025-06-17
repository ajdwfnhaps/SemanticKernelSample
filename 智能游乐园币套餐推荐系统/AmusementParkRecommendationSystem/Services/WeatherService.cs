using AmusementParkRecommendationSystem.Models;
using Microsoft.Extensions.Caching.Memory;

namespace AmusementParkRecommendationSystem.Services;

/// <summary>
/// 天气服务 - 获取天气信息和预报
/// </summary>
public class WeatherService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly string _apiKey;

    public WeatherService(HttpClient httpClient, IMemoryCache cache, string apiKey = "demo_key")
    {
        _httpClient = httpClient;
        _cache = cache;
        _apiKey = apiKey;
    }

    /// <summary>
    /// 获取指定位置和日期的天气信息
    /// </summary>
    public async Task<WeatherInfo> GetWeatherInfoAsync(double latitude, double longitude, DateTime date)
    {
        var cacheKey = $"weather_{latitude}_{longitude}_{date:yyyy-MM-dd}";
        
        if (_cache.TryGetValue(cacheKey, out WeatherInfo? cached) && cached != null)
        {
            return cached;
        }

        try
        {
            // 模拟天气API调用 - 实际使用时替换为真实的天气API
            var weatherInfo = SimulateWeatherData(latitude, longitude, date);
            
            // 缓存1小时
            _cache.Set(cacheKey, weatherInfo, TimeSpan.FromHours(1));
            
            return weatherInfo;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"获取天气信息失败: {ex.Message}");
            
            // 返回默认天气信息
            return new WeatherInfo
            {
                Date = date,
                Condition = "未知",
                Temperature = 20,
                Humidity = 60,
                WindSpeed = "10",
                Visibility = 15,
                UVIndex = 5,
                Recommendation = "请关注当地天气预报"
            };
        }
    }

    /// <summary>
    /// 获取未来几天的天气预报
    /// </summary>
    public async Task<List<WeatherInfo>> GetWeatherForecastAsync(double latitude, double longitude, int days = 7)
    {
        var forecast = new List<WeatherInfo>();
        
        for (int i = 0; i < days; i++)
        {
            var date = DateTime.Today.AddDays(i);
            var weather = await GetWeatherInfoAsync(latitude, longitude, date);
            forecast.Add(weather);
        }
        
        return forecast;
    }

    /// <summary>
    /// 根据天气条件获取活动建议
    /// </summary>
    public string GetActivityRecommendation(WeatherInfo weather)
    {
        var recommendations = new List<string>();
        
        // 根据天气条件提供建议
        switch (weather.Condition.ToLower())
        {
            case var condition when condition.Contains("晴"):
                recommendations.Add("适合户外活动，建议游玩过山车等刺激项目");
                if (weather.Temperature > 25)
                {
                    recommendations.Add("天气较热，建议携带防晒用品和充足水分");
                    recommendations.Add("可以考虑水上项目降温");
                }
                break;
                
            case var condition when condition.Contains("多云"):
                recommendations.Add("天气舒适，适合各种户外活动");
                recommendations.Add("是游园的好天气");
                break;
                
            case var condition when condition.Contains("雨"):
                recommendations.Add("建议优先选择室内项目");
                recommendations.Add("携带雨具，注意防滑");
                recommendations.Add("4D影院、室内游乐场是不错的选择");
                break;
                
            case var condition when condition.Contains("雪"):
                recommendations.Add("注意保暖，穿着防滑鞋");
                recommendations.Add("室内项目为主，户外活动需格外小心");
                break;
                
            case var condition when condition.Contains("雾"):
                recommendations.Add("能见度较低，注意安全");
                recommendations.Add("建议选择室内或近距离项目");
                break;
        }
        
        // 根据温度提供建议
        if (weather.Temperature < 0)
        {
            recommendations.Add("天气严寒，注意防寒保暖");
            recommendations.Add("建议减少户外活动时间");
        }
        else if (weather.Temperature < 10)
        {
            recommendations.Add("天气较冷，建议穿着保暖衣物");
        }
        else if (weather.Temperature > 35)
        {
            recommendations.Add("高温天气，避免长时间暴晒");
            recommendations.Add("多喝水，适当休息");
        }
          // 根据风速提供建议
        if (int.TryParse(weather.WindSpeed, out int windSpeedValue) && windSpeedValue > 30)
        {
            recommendations.Add("风力较大，高空项目可能暂停");
            recommendations.Add("注意固定好随身物品");
        }
        
        // 根据紫外线指数提供建议
        if (weather.UVIndex > 7)
        {
            recommendations.Add("紫外线强烈，做好防晒措施");
            recommendations.Add("建议使用防晒霜和遮阳帽");
        }
        
        return recommendations.Any() ? string.Join("; ", recommendations) : "天气条件良好，适合游园";
    }

    /// <summary>
    /// 检查天气是否适合特定活动
    /// </summary>
    public bool IsWeatherSuitableForActivity(WeatherInfo weather, string activityType)
    {
        return activityType.ToLower() switch
        {
            "outdoor" or "户外" => !weather.Condition.Contains("雨") && !weather.Condition.Contains("雪") && 
                                     int.TryParse(weather.WindSpeed, out int windSpeed) && windSpeed < 25,
            "indoor" or "室内" => true, // 室内活动不受天气影响
            "water" or "水上" => weather.Temperature > 20 && !weather.Condition.Contains("雷"),
            "extreme" or "极限" => weather.Condition.Contains("晴") && 
                                    int.TryParse(weather.WindSpeed, out int extremeWindSpeed) && extremeWindSpeed < 15 && 
                                    weather.Temperature > 10 && weather.Temperature < 35,
            _ => true
        };
    }

    /// <summary>
    /// 模拟天气数据 - 实际使用时替换为真实天气API
    /// </summary>
    private WeatherInfo SimulateWeatherData(double latitude, double longitude, DateTime date)
    {
        var random = new Random();
        var conditions = new[] { "晴", "多云", "阴", "小雨", "中雨", "雾" };
        var condition = conditions[random.Next(conditions.Length)];
        
        // 根据季节调整温度范围
        var month = date.Month;
        var (minTemp, maxTemp) = month switch
        {
            12 or 1 or 2 => (-5, 5),    // 冬季
            3 or 4 or 5 => (10, 25),    // 春季
            6 or 7 or 8 => (25, 35),    // 夏季
            9 or 10 or 11 => (5, 20),   // 秋季
            _ => (10, 25)
        };
        
        var temperature = random.Next(minTemp, maxTemp + 1);
        var humidity = random.Next(40, 90);
        var windSpeed = random.Next(5, 25);
        var visibility = condition.Contains("雾") ? random.Next(1, 5) : random.Next(10, 20);
        var uvIndex = condition.Contains("晴") ? random.Next(5, 11) : random.Next(1, 6);
        
        var weatherInfo = new WeatherInfo
        {
            Date = date,
            Condition = condition,
            Temperature = temperature,
            Humidity = humidity,
            WindSpeed = windSpeed.ToString(),
            Visibility = visibility,
            UVIndex = uvIndex
        };
        
        weatherInfo.Recommendation = GetActivityRecommendation(weatherInfo);
        
        return weatherInfo;
    }
}
