using AmusementParkRecommendationSystem.Models;
using NetTopologySuite.Geometries;
using NetTopologySuite;
using System.Text.Json;

namespace AmusementParkRecommendationSystem.Services;

/// <summary>
/// 位置服务 - 处理地理位置相关操作
/// </summary>
public class LocationService
{
    private readonly GeometryFactory _geometryFactory;
    private readonly MapApiService _mapApiService;
    private readonly POIService _poiService;

    public LocationService(MapApiService mapApiService, POIService poiService)
    {
        var geometryServices = NtsGeometryServices.Instance;
        _geometryFactory = geometryServices.CreateGeometryFactory(srid: 4326); // WGS84
        _mapApiService = mapApiService;
        _poiService = poiService;
    }

    /// <summary>
    /// 计算两点之间的距离（米）
    /// </summary>
    public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var point1 = _geometryFactory.CreatePoint(new Coordinate(lon1, lat1));
        var point2 = _geometryFactory.CreatePoint(new Coordinate(lon2, lat2));
        
        // 使用 Haversine 公式计算地球表面距离
        return CalculateHaversineDistance(lat1, lon1, lat2, lon2);
    }

    /// <summary>
    /// Haversine 公式计算地球表面两点间距离
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

    /// <summary>
    /// 获取指定范围内的门店
    /// </summary>
    public async Task<List<StoreLocation>> GetNearbyStoresAsync(double latitude, double longitude, double radiusMeters)
    {
        // 这里应该连接到实际的数据库或缓存
        // 暂时使用模拟数据
        var allStores = GetSampleStores();
        
        var nearbyStores = allStores.Where(store =>
            CalculateDistance(latitude, longitude, store.Latitude, store.Longitude) <= radiusMeters
        ).OrderBy(store => 
            CalculateDistance(latitude, longitude, store.Latitude, store.Longitude)
        ).ToList();

        return nearbyStores;
    }

    /// <summary>
    /// 获取指定位置附近的兴趣点
    /// </summary>
    public async Task<List<PointOfInterest>> GetNearbyPOIsAsync(double latitude, double longitude, double radiusMeters, string[]? categories = null)
    {
        return await _poiService.GetPOIsAsync(latitude, longitude, radiusMeters, categories);
    }

    /// <summary>
    /// 地址解析为坐标
    /// </summary>
    public async Task<(double Latitude, double Longitude)?> GeocodeAddressAsync(string address)
    {
        return await _mapApiService.GeocodeAsync(address);
    }

    /// <summary>
    /// 坐标反解析为地址
    /// </summary>
    public async Task<string?> ReverseGeocodeAsync(double latitude, double longitude)
    {
        return await _mapApiService.ReverseGeocodeAsync(latitude, longitude);
    }

    /// <summary>
    /// 计算路线和距离
    /// </summary>
    public async Task<(double DistanceMeters, int DurationSeconds, List<(double Lat, double Lon)> Route)?> CalculateRouteAsync(
        double fromLat, double fromLon, double toLat, double toLon, string transportMode = "walking")
    {
        return await _mapApiService.CalculateRouteAsync(fromLat, fromLon, toLat, toLon, transportMode);
    }

    /// <summary>
    /// 检查客户是否在指定门店的邀请范围内
    /// </summary>
    public bool IsCustomerInInvitationRange(CustomerLocationPreference customer, StoreLocation store, double invitationRadiusMeters = 2000)
    {
        var distance = CalculateDistance(customer.CurrentLatitude, customer.CurrentLongitude, 
                                       store.Latitude, store.Longitude);
        return distance <= invitationRadiusMeters;
    }

    /// <summary>
    /// 获取示例门店数据
    /// </summary>
    private List<StoreLocation> GetSampleStores()
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
                Rating = 4.6
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
                Rating = 4.5
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
                Rating = 4.8
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
                Rating = 4.2
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
                Rating = 4.4
            }
        };
    }
}
