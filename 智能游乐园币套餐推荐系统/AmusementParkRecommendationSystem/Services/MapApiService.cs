using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace AmusementParkRecommendationSystem.Services;

/// <summary>
/// 地图API服务 - 集成百度地图/高德地图API
/// </summary>
public class MapApiService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly string _apiKey;
    private readonly string _baseUrl;

    public MapApiService(HttpClient httpClient, IMemoryCache cache, string apiKey = "demo_key")
    {
        _httpClient = httpClient;
        _cache = cache;
        _apiKey = apiKey;
        _baseUrl = "https://api.map.baidu.com"; // 可以切换为高德地图API
    }

    /// <summary>
    /// 地址解析为坐标（地理编码）
    /// </summary>
    public async Task<(double Latitude, double Longitude)?> GeocodeAsync(string address)
    {
        var cacheKey = $"geocode_{address}";
        if (_cache.TryGetValue(cacheKey, out (double lat, double lon) cached))
        {
            return cached;
        }

        try
        {
            // 模拟百度地图地理编码API调用
            // 实际使用时需要替换为真实的API调用
            var mockResult = SimulateGeocoding(address);
            
            if (mockResult.HasValue)
            {
                _cache.Set(cacheKey, mockResult.Value, TimeSpan.FromHours(24));
            }

            return mockResult;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"地理编码失败: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 坐标反解析为地址（逆地理编码）
    /// </summary>
    public async Task<string?> ReverseGeocodeAsync(double latitude, double longitude)
    {
        var cacheKey = $"reverse_{latitude}_{longitude}";
        if (_cache.TryGetValue(cacheKey, out string? cached))
        {
            return cached;
        }

        try
        {
            // 模拟百度地图逆地理编码API调用
            var mockResult = SimulateReverseGeocoding(latitude, longitude);
            
            if (!string.IsNullOrEmpty(mockResult))
            {
                _cache.Set(cacheKey, mockResult, TimeSpan.FromHours(24));
            }

            return mockResult;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"逆地理编码失败: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 计算路线规划
    /// </summary>
    public async Task<(double DistanceMeters, int DurationSeconds, List<(double Lat, double Lon)> Route)?> CalculateRouteAsync(
        double fromLat, double fromLon, double toLat, double toLon, string transportMode = "walking")
    {
        var cacheKey = $"route_{fromLat}_{fromLon}_{toLat}_{toLon}_{transportMode}";
        if (_cache.TryGetValue(cacheKey, out (double distance, int duration, List<(double, double)> route) cached))
        {
            return cached;
        }

        try
        {
            // 模拟路线规划API调用
            var mockResult = SimulateRouteCalculation(fromLat, fromLon, toLat, toLon, transportMode);
            
            if (mockResult.HasValue)
            {
                _cache.Set(cacheKey, mockResult.Value, TimeSpan.FromHours(1));
            }

            return mockResult;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"路线规划失败: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 获取实时交通信息
    /// </summary>
    public async Task<string> GetTrafficInfoAsync(double latitude, double longitude, double radius = 5000)
    {
        try
        {
            // 模拟实时交通信息
            var random = new Random();
            var trafficLevel = random.Next(1, 4);
            
            return trafficLevel switch
            {
                1 => "畅通",
                2 => "缓慢",
                3 => "拥堵",
                _ => "严重拥堵"
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"获取交通信息失败: {ex.Message}");
            return "未知";
        }
    }

    #region 模拟API响应方法

    /// <summary>
    /// 模拟地理编码 - 实际使用时替换为真实API调用
    /// </summary>
    private (double Latitude, double Longitude)? SimulateGeocoding(string address)
    {
        // 预定义一些常见地址的坐标
        var knownAddresses = new Dictionary<string, (double, double)>
        {
            ["北京市"] = (39.904200, 116.407396),
            ["天安门"] = (39.903840, 116.397477),
            ["故宫"] = (39.918058, 116.397026),
            ["颐和园"] = (39.999415, 116.275626),
            ["鸟巢"] = (39.992958, 116.397228),
            ["水立方"] = (39.994571, 116.388374),
            ["三里屯"] = (39.938784, 116.447968),
            ["王府井"] = (39.909965, 116.416357),
            ["西单"] = (39.906306, 116.373834),
            ["朝阳公园"] = (39.939800, 116.473556)
        };

        foreach (var (key, value) in knownAddresses)
        {
            if (address.Contains(key))
            {
                return value;
            }
        }

        // 如果找不到匹配的地址，返回北京市中心附近的随机坐标
        var random = new Random();
        var lat = 39.904200 + (random.NextDouble() - 0.5) * 0.2; // ±0.1度范围
        var lon = 116.407396 + (random.NextDouble() - 0.5) * 0.2;
        
        return (lat, lon);
    }

    /// <summary>
    /// 模拟逆地理编码
    /// </summary>
    private string? SimulateReverseGeocoding(double latitude, double longitude)
    {
        // 简单的地址模拟
        if (latitude >= 39.8 && latitude <= 40.1 && longitude >= 116.2 && longitude <= 116.8)
        {
            var districts = new[] { "朝阳区", "海淀区", "西城区", "东城区", "丰台区", "石景山区", "通州区", "大兴区" };
            var random = new Random();
            var district = districts[random.Next(districts.Length)];
            var street = $"某某街道{random.Next(1, 100)}号";
            
            return $"北京市{district}{street}";
        }

        return "未知地址";
    }

    /// <summary>
    /// 模拟路线计算
    /// </summary>
    private (double DistanceMeters, int DurationSeconds, List<(double Lat, double Lon)> Route)? SimulateRouteCalculation(
        double fromLat, double fromLon, double toLat, double toLon, string transportMode)
    {
        // 计算直线距离
        var distance = CalculateHaversineDistance(fromLat, fromLon, toLat, toLon);
        
        // 实际路径距离通常比直线距离长20-50%
        var actualDistance = distance * 1.3;
        
        // 根据交通方式估算时间
        var speed = transportMode switch
        {
            "driving" => 30.0, // 30 km/h (考虑城市交通)
            "transit" => 25.0, // 25 km/h (公共交通平均速度)
            "walking" => 5.0,  // 5 km/h
            "cycling" => 15.0, // 15 km/h
            _ => 5.0
        };

        var durationSeconds = (int)(actualDistance / 1000 * 3600 / speed);

        // 生成简单的路径点（直线插值）
        var route = new List<(double Lat, double Lon)>();
        var steps = Math.Max(2, (int)(actualDistance / 500)); // 每500米一个点
        
        for (int i = 0; i <= steps; i++)
        {
            var ratio = (double)i / steps;
            var lat = fromLat + (toLat - fromLat) * ratio;
            var lon = fromLon + (toLon - fromLon) * ratio;
            route.Add((lat, lon));
        }

        return (actualDistance, durationSeconds, route);
    }

    /// <summary>
    /// Haversine 公式计算距离
    /// </summary>
    private double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
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

    #endregion
}
